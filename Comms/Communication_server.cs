﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using System.Data;

namespace Blackjack.Comms
{
    public class Communication_server
    {
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
            while (true)
            {
                // Receive message.
                var buffer = new byte[1_024];
                var received = await handler.ReceiveAsync(buffer, SocketFlags.None);
                var response = Encoding.UTF8.GetString(buffer, 0, received);

                var eom = "<|EOM|>";
                if (response.IndexOf(eom) > -1 /* is end of message */)
                {
                    Debug.WriteLine(
                        $"Socket server received message: \"{response.Replace(eom, "")}\"");

                    var ackMessage = "<|ACK|>";
                    var echoBytes = Encoding.UTF8.GetBytes(ackMessage);
                    await handler.SendAsync(echoBytes, 0);
                    Debug.WriteLine(
                        $"Socket server sent acknowledgment: \"{ackMessage}\"");

                    break;
                }
                // Sample output:
                //    Socket server received message: "Hi friends 👋!"
                //    Socket server sent acknowledgment: "<|ACK|>"
            }
        }
    }
}
