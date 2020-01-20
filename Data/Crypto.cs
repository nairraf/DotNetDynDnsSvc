using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace DotNetDynDnsSvc.Data
{
    public class Crypto
    {
        private string _secret;
        private byte[] _secretBytes;

        public Crypto()
        {
            _secret = File.ReadLines(String.Format(@"{0}\secret.key", Path.GetFullPath(Path.Combine(HttpRuntime.AppDomainAppPath, @"..\Conf")))).First();
            _secretBytes = Encoding.UTF8.GetBytes(_secret);
        }

        public string Encrypt(string data)
        {

            var dataBytes = Encoding.UTF8.GetBytes(data);

            TripleDESCryptoServiceProvider csp = new TripleDESCryptoServiceProvider();

            csp.Key = _secretBytes;
            csp.Mode = CipherMode.CBC;

            string encodedData = Convert.ToBase64String(csp.CreateEncryptor().TransformFinalBlock(dataBytes, 0, dataBytes.Length));
            csp.Dispose();
            return encodedData;
        }

        public string Decrypt(string encryptedData)
        {

            TripleDESCryptoServiceProvider csp = new TripleDESCryptoServiceProvider();

            csp.Key = _secretBytes;
            csp.Mode = CipherMode.CBC;

            byte[] dataBytes = Convert.FromBase64String(encryptedData);

            string plaintext = Encoding.UTF8.GetString(csp.CreateDecryptor().TransformFinalBlock(dataBytes, 0, dataBytes.Length));
            csp.Dispose();
            return plaintext;
        }
    }
}