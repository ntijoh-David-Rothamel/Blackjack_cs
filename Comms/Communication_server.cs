using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using System.Data;
using Blackjack;

namespace Blackjack.Comms
{
    public class Communication_server
    {
        //Får jag göra så?
        public Socket handler_ = null;
        public Communication_client Client = null;
        public bool read_response = false;
        public String response = null;

        public Communication_server(Communication_client _Client)
        {
            Client = _Client;
            this.Server("");
        }

        public Communication_server(String adress)
        {
            this.Server(adress);
        }

        private async void Server(String adress)
        {
            var hostName = Dns.GetHostName();
            IPHostEntry ipHostInfo = await Dns.GetHostEntryAsync(hostName);
            //IPHostEntry ipHostInfo = await Dns.GetHostEntryAsync(adress); //maps adress to an ip-adress
            IPAddress ipAddress = ipHostInfo.AddressList[0]; //saves the ip-adress

            IPEndPoint ipEndPoint = new(ipAddress, 8080); //Adds ip-adress to a port

            using Socket listener = new(
                ipEndPoint.AddressFamily,
                SocketType.Stream,
                ProtocolType.Tcp);

            listener.Bind(ipEndPoint);
            listener.Listen(100);

            var handler = await listener.AcceptAsync();
            handler_ = handler;

            this.ServerSetUpLoop();
        }

        private async void ServerSetUpLoop()
        {
            while (true)
            {
                // Receive message.
                var buffer = new byte[1_024];
                var received = await handler_.ReceiveAsync(buffer, SocketFlags.None);
                response = Encoding.UTF8.GetString(buffer, 0, received);

                var eom = "<|EOM|>";
                if (response.IndexOf("Hello") > -1 /* is end of message */)
                {
                    Console.WriteLine(
                        $"Socket server received message: \"{response.Replace(eom, "")}\"");

                    var ackMessage = "<|ACK|>";
                    var echoBytes = Encoding.UTF8.GetBytes(ackMessage);
                    await handler_.SendAsync(echoBytes, 0);
                    Console.WriteLine(
                        $"Socket server sent acknowledgment: \"{ackMessage}\"");

                    break;
                }

                if (response.IndexOf("client") > -1)
                {
                    read_response = true;
                }
                // Sample output:
                //    Socket server received message: "Hi friends 👋!"
                //    Socket server sent acknowledgment: "<|ACK|>"
            }
        }

        private async void SendMessage(String message)
        {
            var echoBytes = Encoding.UTF8.GetBytes(message);
            await handler_.SendAsync(echoBytes, 0);
            Console.WriteLine(
                $"Socket server sent message: \"{message}\"");
        }

        public String AskForInput()
        {
            String input = "";
            this.SendMessage("Do you want a card ? (y / n)");
            Client.Send_message();
            //Code for response
            if (read_response)
            {
                input = response;
            }
            return input;
        }
    }
}
