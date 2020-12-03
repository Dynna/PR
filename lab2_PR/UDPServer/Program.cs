using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Security.Cryptography;
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
                var buffer = new byte[512];
                var size = 0;
                var data = new StringBuilder();
                EndPoint senderEndPoint = new IPEndPoint(IPAddress.Any, 0);

               do
               {
                    size = udpSocket.ReceiveFrom(buffer, ref senderEndPoint);
                    data.Append(Encoding.UTF8.GetString(buffer));
               }
               while (udpSocket.Available > 0);

               try 
               {
                    string request = data.ToString();
                    string reqResponse;
                    if (request.StartsWith("1922"))
                    {
                        reqResponse = "Password correct! \n Choose an operation to continue: \n 1. Verify balance     2. Extract money";
                    }
                    else
                    {
                        reqResponse = "Wrong password! Repeat operation.";
                    }

                    byte[] encryptedMessage = server.Encrypt(UDPClient.Program.clientPublicKey, reqResponse);
                    char[] sentMessage = Encoding.Unicode.GetChars(encryptedMessage);

                    udpSocket.SendTo(Encoding.UTF8.GetBytes(sentMessage), senderEndPoint);

                    byte[] dataToByte = Encoding.ASCII.GetBytes(data.ToString());
                    string decryptedMessage = server.Decrypt(UDPClient.Program.ClientPublicKey, dataToByte, UDPClient.Program.ClientIV);
                    Console.WriteLine("Received data: " + decryptedMessage);

                    using (SHA256 sha256Hash = SHA256.Create())
                    {
                        string hash = ErrorChecking.GetHash(sha256Hash, data.ToString());

                        if (!ErrorChecking.VerifyHash(sha256Hash, data.ToString(), hash))
                        {
                            Console.WriteLine("Errors while sending packets.");
                        }
                    }

                }
               catch(SystemException e)
               {
                    Console.WriteLine(e);
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
