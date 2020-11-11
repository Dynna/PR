using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace UDPServer
{
    class Program
    {
        public static DiffieHellman server = new DiffieHellman();

        public static byte[] serverPublicKey = server.PublicKey;

        public static byte[] serverIV = server.IV;

        static void Main(string[] args)
        {
            Console.Title = "SERVER";
            const string ip = "127.0.0.1";
            const int port = 8081;

            var udpEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);

            var udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            udpSocket.Bind(udpEndPoint);

            while (true)
            {
                var buffer = new byte[1600];
                var size = 0;
                var data = new StringBuilder();
                EndPoint senderEndPoint = new IPEndPoint(IPAddress.Any, 0);

               do
               {
                    size = udpSocket.ReceiveFrom(buffer, ref senderEndPoint);
                    data.Append(Encoding.UTF8.GetString(buffer));
               }
               while (udpSocket.Available > 0);
                // TODO: threads? Retransmissions? limit nr of retries for retransmission
               try 
               {
                    string msg = "Message received!";
                    var client = new DiffieHellman();
                   
                    byte[] encryptedMessage = server.Encrypt(UDPClient.Program.clientPublicKey, msg);

                    char[] sentMessage = Encoding.Unicode.GetChars(encryptedMessage);
                    udpSocket.SendTo(Encoding.UTF8.GetBytes(sentMessage), senderEndPoint);
                    
                    Thread.Sleep(50);

                    Console.WriteLine("Received data: " + data);

                   /* byte[] dataToByte = Encoding.ASCII.GetBytes(data.ToString());
                    string decryptedMessage = server.Decrypt(UDPClient.Program.ClientPublicKey, dataToByte, UDPClient.Program.ClientIV);
                    Console.WriteLine("Decrypted data: " + decryptedMessage);*/

                }
               catch(SystemException e)
               {
                    Console.WriteLine("Error while sending packets!" + e);
                    Thread.Sleep(50);
               }
            }
        }

        public static DiffieHellman Server
        {
            get { return server; }
        }

        public static byte[] ServerPublicKey
        {
            get { return serverPublicKey; }
        }

        public static byte[] ServerIV
        {
            get { return serverIV; }
        }
    }
}
