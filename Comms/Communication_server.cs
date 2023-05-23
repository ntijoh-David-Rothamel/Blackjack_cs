﻿using System;
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

        public Communication_server()
        {
            this.Server("");
        }

        public Communication_server(String adress)
        {
            this.Server(adress);
        }

        private async void Server(String adress)
        {
            var hostName = Dns.GetHostName();
            //IPHostEntry ipHostInfo = await Dns.GetHostEntryAsync(hostName);
            IPHostEntry ipHostInfo = await Dns.GetHostEntryAsync(hostName); //maps adress to an ip-adress
            IPAddress ipAddress = ipHostInfo.AddressList[0]; //saves the ip-adress

            IPEndPoint ipEndPoint = new(ipAddress, 8080); //Adds ip-adress to a port

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
                if (response.IndexOf("") > -1 /* is end of message */)
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
                //    Socket server sent acknowledgment: "<|ACK|>"
            }
        }

        private async Task<bool> SendMessage(String message)
        {
            Console.WriteLine("Sending message");
            var echoBytes = Encoding.UTF8.GetBytes(message);
            await handler_.SendAsync(echoBytes, 0);
            Console.WriteLine(
                $"Socket server sent message: \"{message}\"");
            return true;
        }

        public String AskForInput()
        {
            Console.WriteLine("Asking for input");
            Task.Delay(1000).Wait();
            this.SendMessage("input").Wait();
            while (response == old_response) { }
            Console.WriteLine("Repsonse is: " + response);
            old_response = response;
            return response.Substring(0, 1);
        }
        public void SendHand(String hand_in_code)
        {
            Console.WriteLine("Sending hand");
            this.SendMessage("c" + hand_in_code).Wait();
            Console.WriteLine("Sent hand");
        }
        public void SendWinner(String winner)
        {
            Console.WriteLine("Sending winner");
            this.SendMessage("w" + winner).Wait();
            Console.WriteLine("Sent winner");
        }
    }
}
