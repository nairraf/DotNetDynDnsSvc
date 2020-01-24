using System;
using Farrworks.Crypto;
using Farrworks.Crypto.Basic;

namespace DotNetDynDnsSvc.Common
{
    public class EncryptionManager
    {
        private string _seed;
        private TripleDesProvider _tdes;
        public EncryptionManager(string seed)
        {
            if (seed.Length <= 0)
                throw new Exception("invalid seed");

            _seed = seed;
            _tdes = new TripleDesProvider(_seed);
        }

        public string Encrypt(string toEncrypt)
        {
            return _tdes.Encrypt(toEncrypt);
        }

        public string Decrypt(string cipherTextToDecrypt)
        {
            return _tdes.Decrypt(cipherTextToDecrypt);
        }
    }
}
