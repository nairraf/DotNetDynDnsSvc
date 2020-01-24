using System;
using System.Security.Cryptography;
using System.Text;

namespace DotNetDynDnsSvc.Common
{
    public static class Crypto
    {
        static Crypto() { }

        public static string GenerateSeed(string initialSeed)
        {
            if (!initialSeed.Contains("+") && !initialSeed.Contains("/"))
                throw new Exception("Invalid initialSeed");

            // this will generate 4 different strings. 
            // compute the hashes for each part, scamble and put back together.

            string[] split1 = initialSeed.Split('+');
            string[] split2 = initialSeed.Split('/');

            byte[] part1, part2, part3, part4;

            using (SHA256CryptoServiceProvider sha = new SHA256CryptoServiceProvider())
            {
                part1 = sha.ComputeHash(Encoding.UTF8.GetBytes(split1[0]));
                part2 = sha.ComputeHash(Encoding.UTF8.GetBytes(split1[1]));
                part3 = sha.ComputeHash(Encoding.UTF8.GetBytes(split2[0]));
                part4 = sha.ComputeHash(Encoding.UTF8.GetBytes(split2[1]));
            }

            byte[] seedBytes = new byte[part1.Length + part2.Length + part3.Length + part4.Length];
            part3.CopyTo(seedBytes, 0);
            part1.CopyTo(seedBytes, part3.Length);
            part4.CopyTo(seedBytes, part1.Length);
            part2.CopyTo(seedBytes, part4.Length);

            return Convert.ToBase64String(seedBytes);
        }
    }
}
