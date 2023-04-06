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
    public class Communication_client
    {
        Socket client = null;

        public Communication_client()
        {
            Console.WriteLine("Comm");   
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

            this.client = new(//Creates the client object, using these arguments: (Which constructor is this?)
                    ipEndPoint.AddressFamily, //Specify what kind of adress
                    SocketType.Stream, //Stream means reliable communication between two ports. Uses TCP. Whatever that means
                    ProtocolType.Tcp); //Specifies protocol? 

            await this.client.ConnectAsync(ipEndPoint);
            //If ConnectAsync isn't done the code will return to the calling method
            //Therefore it will not continue to the while loop
            //Until ConnectAsync is done

            this.Send_message("Hello World!");
        }

        public async void Send_message(string _message)
        {
            while (true)//While for communicating with other computer
            {
                // Send message.
                var message = _message; //Message
                var messageBytes = Encoding.UTF8.GetBytes(message); //maps message to bytes
                _ = await this.client.SendAsync(messageBytes, SocketFlags.None); //Sends message over and waits for signal that it is done
                Console.WriteLine($"Socket client sent message: \"{message}\""); //prints to console

                // Receive ack. Acknowledgment
                var buffer = new byte[1_024]; //Don't know
                var received = await this.client.ReceiveAsync(buffer, SocketFlags.None);//preps for message?
                var response = Encoding.UTF8.GetString(buffer, 0, received);//maps bytes to something readable
                if (response == "<|ACK|>") //If I get answer print answer to console
                {
                    Console.WriteLine(
                        $"Socket client received acknowledgment: \"{response}\"");
                }
                // Sample output:
                //     Socket client sent message: "Hi friends 👋!<|EOM|>"
                //     Socket client received acknowledgment: "<|ACK|>"
            }
        }

        public async void Send_message()
        {
            var _message = Console.ReadLine();

            while (true)//While for communicating with other computer
            {
                // Send message.
                var message = _message; //Message
                var messageBytes = Encoding.UTF8.GetBytes(message); //maps message to bytes
                _ = await this.client.SendAsync(messageBytes, SocketFlags.None); //Sends message over and waits for signal that it is done
                Console.WriteLine($"Socket client sent message: \"{message}\""); //prints to console

                // Receive ack. Acknowledgment
                var buffer = new byte[1_024]; //Don't know
                var received = await this.client.ReceiveAsync(buffer, SocketFlags.None);//preps for message?
                var response = Encoding.UTF8.GetString(buffer, 0, received);//maps bytes to something readable
                if (response == "<|ACK|>") //If I get answer print answer to console
                {
                    Console.WriteLine(
                        $"Socket client received acknowledgment: \"{response}\"");
                }
                // Sample output:
                //     Socket client sent message: "Hi friends 👋!<|EOM|>"
                //     Socket client received acknowledgment: "<|ACK|>"
            }
        }

        public void Shutdown()
        {
            this.client.Shutdown(SocketShutdown.Both);
        }

    }
}
