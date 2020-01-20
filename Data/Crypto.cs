using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace DotNetDynDnsSvc.Data
{
    public class Crypto : IDisposable
    {
        private readonly string _key;
        private readonly byte[] _keyBytes;
        private TripleDESCryptoServiceProvider _tdesCrypto;
        private MD5CryptoServiceProvider _md5;

        public Crypto()
        {
            _key = File.ReadLines(String.Format(@"{0}\secret.key", Path.GetFullPath(Path.Combine(HttpRuntime.AppDomainAppPath, @"..\Conf")))).First();
            _keyBytes = Encoding.UTF8.GetBytes(_key);
            _tdesCrypto = new TripleDESCryptoServiceProvider();
            _md5 = new MD5CryptoServiceProvider();
            _tdesCrypto.Key = _md5.ComputeHash(_keyBytes);
            _tdesCrypto.Mode = CipherMode.CBC;
        }

        public string BasicEncrypt(string data)
        {
            byte[] buffer = Encoding.ASCII.GetBytes(data);
            string ret = "";
            try
            {
                string cipherText = Convert.ToBase64String(_tdesCrypto.CreateEncryptor().TransformFinalBlock(buffer, 0, buffer.Length));
                string iv = Convert.ToBase64String(_tdesCrypto.IV);
                // remove the = in the iv string
                iv = iv.Substring(0, iv.Length - 1);
                // return the iv and the cipherText to the caller
                ret = String.Format("{0}{1}", iv, cipherText);
            }
            catch
            {
                // do nothing for now
            }

            return ret;
        }

        public string BasicDecrypt(string encryptedData)
        {
            // we have to get our IV from the encryptedData string. the IV string is 11 characters followed by an equal sign to make 12 characters in length
            // we removed the = sign when we decrypt, so get the first 11 characters from the string and put back the = sign.
            // convert to byte array and assign as our IV for this decryption process.
            string iv = String.Format("{0}=", encryptedData.Remove(11));
            _tdesCrypto.IV = Convert.FromBase64String(iv);

            // get our cipher text, starting at position 11 until the end and convert to a byte array, and decrypt
            string cipherText = encryptedData.Substring(11);
            byte[] buffer = Convert.FromBase64String(cipherText);
            string ret = "";
            try
            {
                ret = Encoding.UTF8.GetString(_tdesCrypto.CreateDecryptor().TransformFinalBlock(buffer, 0, buffer.Length));
            }
            catch
            {
                // do nothing for now
            }
            return ret;
        }

        public void Dispose()
        {
            _md5.Dispose();
            _tdesCrypto.Dispose();
        }
    }
}