using Microsoft.VisualStudio.TestTools.UnitTesting;
using Xunit.Sdk;

namespace SecureKeyExchange.Tests
{
    [TestClass()]
    class DiffieHellmanTests
    {
        internal static string msg;

        [TestMethod()]
        public void Encrypt_Decrypt()
        {
            string text = "Test message!";

            using (var client = new DiffieHellman())
            {
                using (var server = new DiffieHellman())
                {
                    // client uses server's public key to encrypt his message
                    byte[] secretMessage = client.Encrypt(server.PublicKey, text);

                    // server uses client's public key and IV to decrypt the secret message
                    string decryptedMessage = server.Decrypt(client.PublicKey, secretMessage, client.IV);
                }
            }
        }
    }
}
