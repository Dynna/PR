using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

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

            int timeout = 5000;
            int maxNrRetries = 5;
            bool isReceived = false;
            int nrTries = 0;

            while (true && (!isReceived) && (nrTries < maxNrRetries))
            {
                try
                {
                    Console.WriteLine("ATM current state: NO CARD");
                    Thread.Sleep(5000);
                    Console.WriteLine("Processing request . . .");
                    Thread.Sleep(5000);
                    Console.WriteLine("### Card inserted! ###");
                    Thread.Sleep(3000);
                    Console.WriteLine("Type in your password: ");
                    var message = Console.ReadLine();

                    byte[] encryptedMessage = client.Encrypt(UDPServer.Program.serverPublicKey, message);
                    char[] sentMessage = Encoding.Unicode.GetChars(encryptedMessage);

                    var serverEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8081);
                    udpSocket.SendTo(Encoding.UTF8.GetBytes(sentMessage), serverEndPoint);
                    udpSocket.SendTimeout = timeout;
                    udpSocket.ReceiveTimeout = timeout;

                    var buffer = new byte[512];
                    var size = 0;
                    var data = new StringBuilder();
                    EndPoint senderEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8081);


                do
                {
                    size = udpSocket.ReceiveFrom(buffer, ref senderEndPoint);
                    data.Append(Encoding.UTF8.GetString(buffer));
                    isReceived = true;
                }

                while (udpSocket.Available > 0);

                    byte[] dataToByte = Encoding.ASCII.GetBytes(data.ToString());
                    string decryptedMessage = client.Decrypt(UDPServer.Program.ServerPublicKey, dataToByte, UDPServer.Program.ServerIV);
                    Console.WriteLine(decryptedMessage);

                    using (SHA256 sha256Hash = SHA256.Create())
                    {
                        string hash = ErrorChecking.GetHash(sha256Hash, data.ToString());

                        if (!ErrorChecking.VerifyHash(sha256Hash, data.ToString(), hash))
                        {
                            Console.WriteLine("Errors while sending packets.");
                        }
                    }

                    Console.ReadLine();

                }
                catch(SystemException e) 
                {
                    nrTries += 1;
                    Console.WriteLine("No response, try " + (maxNrRetries - nrTries) + " more times.");
                  //  Console.WriteLine(e);
                }
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
