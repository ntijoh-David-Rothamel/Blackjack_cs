using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using System.Data;

namespace Blackjack
{
    class Communication_client
    {
        public Communication_client()
        {
            Debug.Write("Comm");   
            this.Communicate("");
        }
        public Communication_client(string adress)
        {
            this.Communicate(adress);
        }

        private async void Communicate(string adress)
        {
            var hostName = Dns.GetHostName();
            IPHostEntry ipHostInfo = await Dns.GetHostEntryAsync(hostName);
            //IPHostEntry ipHostInfo = await Dns.GetHostEntryAsync(adress); //maps adress to an ip-adress
            IPAddress ipAddress = ipHostInfo.AddressList[0]; //saves the ip-adress

            IPEndPoint ipEndPoint = new(ipAddress, 8080); //Adds ip-adress to a port

            using Socket client = new(//Creates the client object, using these arguments: (Which constructor is this?)
                    ipEndPoint.AddressFamily, //Specify what kind of adress
                    SocketType.Stream, //Stream means reliable communication between two ports. Uses TCP. Whatever that means
                    ProtocolType.Tcp); //Specifies protocol? 

            await client.ConnectAsync(ipEndPoint);
            //If ConnectAsync isn't done the code will return to the calling method
            //Therefore it will not continue to the while loop
            //Until ConnectAsync is done

            while (true)//While for communicating with other computer
            {
                // Send message.
                var message = "Hi friends 👋!<|EOM|>"; //Message
                var messageBytes = Encoding.UTF8.GetBytes(message); //maps message to bytes
                _ = await client.SendAsync(messageBytes, SocketFlags.None); //Sends message over and waits for signal that it is done
                Debug.WriteLine($"Socket client sent message: \"{message}\""); //prints to console

                // Receive ack. Acknowledgment
                var buffer = new byte[1_024]; //Don't know
                var received = await client.ReceiveAsync(buffer, SocketFlags.None);//preps for message?
                var response = Encoding.UTF8.GetString(buffer, 0, received);//maps bytes to something readable
                if (response == "<|ACK|>") //If I get answer print answer to console
                {
                    Debug.WriteLine(
                        $"Socket client received acknowledgment: \"{response}\"");
                }
                // Sample output:
                //     Socket client sent message: "Hi friends 👋!<|EOM|>"
                //     Socket client received acknowledgment: "<|ACK|>"
            }

            client.Shutdown(SocketShutdown.Both);
        }


    }
}
