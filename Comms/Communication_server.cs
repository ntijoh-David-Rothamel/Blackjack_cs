using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Blackjack.Comms
{
    public class Communication_server
    {
        //Får jag göra så?
        public Socket handler_ = null;
        public String response = null;
        public bool done = false;
        public String old_response = null;
        /// <summary>
        /// Overloadad konstruktör
        /// Vilket innebär att det finns två metoder av samma namn
        /// men med olika argument
        /// Denna används för tillfället
        /// Då det var enklare att justera address i Server(Sträng) metoden
        /// 
        /// Kallar på Server(Sträng) för att skapa upp servern
        /// </summary>
        public Communication_server()
        {
            this.Server("");
        }
        /// <summary>
        /// Resultatet är för tillfället samma som för den andra konstruktören
        /// då Server(Sträng) inte gör något med argumentet
        /// </summary>
        /// <param name="adress">
        /// Sträng, En IP-adress
        /// </param>
        public Communication_server(String adress)
        {
            this.Server(adress);
        }
        /// <summary>
        /// Skapar upp servern
        /// Den är async, men körs i praktiken synkront
        /// Då resten av programmet väntar på att den ska skapas
        /// Först öppnar den upp kommunikation, på alla dess möjliga IP-adresser
        /// Därefter instantierar den en IPendpoint
        /// Vilket är en punkt som andra datorer kan koppla upp sig till
        /// Den används sedan i instantierandet av Socket
        /// Där kommunikationssätt också avgörs
        /// Efter det lystnar Socketen tills den får en connection
        /// som den kallar för handler
        /// Det sista den gör är att kalla på nästa metod
        /// ServerLoop()
        /// </summary>
        /// <param name="adress">
        /// Sträng, Argument för att välja IP-adress
        /// Gör inget för tillfället 
        /// och lär inte heller göra det i framtiden
        /// </param>
        private async void Server(String adress)
        {
            Console.WriteLine("Server");
            var hostName = Dns.GetHostName();
            IPHostEntry ipHostInfo = await Dns.GetHostEntryAsync(hostName); //maps adress to an ip-adress
            //Console.WriteLine(ipHostInfo.AddressList[0]);
            //IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPAddress ipAddress = IPAddress.Any; //saves the ip-adress

            IPEndPoint ipEndPoint = new(ipAddress, 1234); //Adds ip-adress to a port

            using Socket listener = new(
                ipEndPoint.AddressFamily,
                SocketType.Stream,
                ProtocolType.Tcp);

            listener.Bind(ipEndPoint);
            listener.Listen(100);
            while (true)
            {
                var handler = await listener.AcceptAsync();
                if (handler != null)
                {
                    handler_ = handler;
                    done = true;
                    break;
                }
            }
            this.ServerLoop();
        }

        /// <summary>
        /// Lystnar efter meddelanden från clienten
        /// Jobbar egentligen inte heller asynkront
        /// då resten av programmet väntar på att servern har fått konfirmation
        /// eller svar från clienten
        /// så att den kan fortsätta utan att clienten sackar efter
        /// 
        /// Det den gör är att den tar emot ett meddelande från clienten
        /// Sedan dekrypterar den det till en sträng
        /// Som resten av servern sedan kan använda
        /// </summary>
        private async void ServerLoop()
        {
            while (true)
            {
                // Receive message.
                var buffer = new byte[1_024];
                var received = await handler_.ReceiveAsync(buffer, SocketFlags.None);
                response = Encoding.UTF8.GetString(buffer, 0, received);
                /*
                                var eom = "<|EOM|>";
                                if (response.IndexOf("") > -1  is end of message )
                                {
                                    //Console.WriteLine(
                                    //    $"Socket server received message: \"{response.Replace(eom, "")}\"");

                                    var ackMessage = "<|ACK|>";
                                    var echoBytes = Encoding.UTF8.GetBytes(ackMessage);
                                    //await handler_.SendAsync(echoBytes, 0);
                                    //Console.WriteLine(
                                    //    $"Socket server sent acknowledgment: \"{ackMessage}\"");

                                }
                                // Sample output:
                                //    Socket server received message: "Hi friends 👋!"
                                //    Socket server sent acknowledgment: "<|ACK|>"*/
            }
        }
        /// <summary>
        /// Skickar iväg meddelanden till clienten
        /// 
        /// Den gör det genom att argumentet
        /// enkryptera det
        /// och sedan skicka iväg
        /// Därefter väntar den till den får en konfirmation från klienten
        /// </summary>
        /// <param name="message">
        /// sträng, Ett meddelande
        /// används bl.a för att skicka korten, och fråga om input
        /// </param>
        /// <returns>
        /// Task<bool>, Returnerar löftet om en bool
        /// Returneringen används för att kunna använda wait() metoden
        /// </returns>
        private async Task<bool> SendMessage(String message)
        {
            Console.WriteLine("Sending message");
            var echoBytes = Encoding.UTF8.GetBytes(message);
            await handler_.SendAsync(echoBytes, 0);
            while (response == old_response) { }
            old_response = response;
            Console.WriteLine(
                $"Socket server sent message: \"{message}\"");
            return true;
        }
        /// <summary>
        /// Används för att fråga clienten om input
        /// 
        /// Använder SendMessage(Sträng) med argumentet "input"
        /// vilket clienten sen kan tolka
        /// SendMessage(Sträng) har också en Wait() metod
        /// för att programmet inte ska gå vidare
        /// </summary>
        /// <returns>
        /// Sträng, Det den returnerar är första symbolen i response
        /// Vilket förhoppningsvis är y eller n
        /// Detta innebär också att man kan skriva yes
        /// och ändå uppnå samma mål
        /// </returns>
        public String AskForInput()
        {
            Console.WriteLine("Asking for input");
            Task.Delay(1000).Wait();
            this.SendMessage("input").Wait();
            Console.WriteLine("Repsonse is: " + response);
            old_response = response;
            return response.Substring(0, 1);
        }

        /// <summary>
        /// Används för att skicka spelarnas kort till clienten
        /// 
        /// kallar på metoden SendMessage(Sträng)
        /// Som argument passerar den "c" + argumentet
        /// Anledningen till det är att clienten då kan tolka det som kort
        /// </summary>
        /// <param name="hand_in_code">
        /// Sträng, Symboliserar en spelares kort
        /// samt värdet på dess hand
        /// </param>
        public void SendHand(String hand_in_code)
        {
            Console.WriteLine(hand_in_code);
            Console.WriteLine("Sending hand");
            this.SendMessage("c" + hand_in_code).Wait();
            Console.WriteLine("Sent hand");
        }
        /// <summary>
        /// Används för att skicka vem som har vunnit
        /// 
        /// Fungerar på liknande sätt som SendHand(Sträng)
        /// men prependar med "w" istället så att clienten tolkar det som information om vinnaren
        /// </summary>
        /// <param name="winner">
        /// Sträng, Är namnet på vinnaren
        /// </param>
        public void SendWinner(String winner)
        {
            Console.WriteLine("Sending winner");
            this.SendMessage("w" + winner).Wait();
            Console.WriteLine("Sent winner");
        }
    }
}
