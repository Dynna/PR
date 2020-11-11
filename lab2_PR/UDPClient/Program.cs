using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace UDPClient
{
    public class Program
    {
        public static DiffieHellman client = new DiffieHellman();

        public static byte[] clientPublicKey = client.PublicKey;

        public static byte[] clientIV = client.IV;

        static void Main(string[] args)
        {
            Console.Title = "CLIENT";
            const string ip = "127.0.0.1";
            const int port = 8082;

            var udpEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);

            var udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            udpSocket.Bind(udpEndPoint);

            while (true)
            {
                Console.WriteLine("Type something:");
                var message = Console.ReadLine();

                byte[] encryptedMessage = client.Encrypt(UDPServer.Program.serverPublicKey, message);

                char[] sentMessage = Encoding.Unicode.GetChars(encryptedMessage);
                var serverEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8081);
                udpSocket.SendTo(Encoding.UTF8.GetBytes(sentMessage), serverEndPoint);

                var buffer = new byte[1600];
                var size = 0;
                var data = new StringBuilder();
                EndPoint senderEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8081);

                do
                {
                    size = udpSocket.ReceiveFrom(buffer, ref senderEndPoint);
                    data.Append(Encoding.UTF8.GetString(buffer));
                }
                while (udpSocket.Available > 0);

                Console.WriteLine(data);
                Console.ReadLine();
            }
        }

        public static DiffieHellman Client
        {
            get { return client; }
        }

        public static byte[] ClientPublicKey
        {
            get { return clientPublicKey; }
        }

        public static byte[] ClientIV
        {
            get { return clientIV; }
        }
    }
}
