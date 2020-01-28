using Org.BouncyCastle.Crypto.Parameters;
using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
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

        public static string GetSeedFromCert(string CertSubjectName)
        {
            X509Store store = new X509Store(StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadOnly);
            X509Certificate2Collection certs = store.Certificates.Find(X509FindType.FindBySubjectName, CertSubjectName, false);
            X509Certificate2 cer;
            string privateCert = "";

            if (certs.Count <= 0)
                throw new Exception(String.Format("Unable To Find Cert: {0}", CertSubjectName));

            cer = certs[0];
            try
            {
                RSACryptoServiceProvider rsa = cer.PrivateKey as RSACryptoServiceProvider;
                RSAParameters rSAParameters = rsa.ExportParameters(true);

                byte[] privateCertRawBytes = new byte[rSAParameters.Modulus.Length +
                                                        rSAParameters.Exponent.Length +
                                                        rSAParameters.D.Length +
                                                        rSAParameters.P.Length +
                                                        rSAParameters.Q.Length +
                                                        rSAParameters.DP.Length +
                                                        rSAParameters.DQ.Length +
                                                        rSAParameters.InverseQ.Length
                                                        ];

                int index = 0;
                rSAParameters.Modulus.CopyTo(privateCertRawBytes, index);
                index += rSAParameters.Modulus.Length;

                rSAParameters.Exponent.CopyTo(privateCertRawBytes, index);
                index += rSAParameters.Exponent.Length;

                rSAParameters.D.CopyTo(privateCertRawBytes, index);
                index += rSAParameters.D.Length;

                rSAParameters.P.CopyTo(privateCertRawBytes, index);
                index += rSAParameters.P.Length;

                rSAParameters.Q.CopyTo(privateCertRawBytes, index);
                index += rSAParameters.Q.Length;

                rSAParameters.DP.CopyTo(privateCertRawBytes, index);
                index += rSAParameters.DP.Length;

                rSAParameters.DQ.CopyTo(privateCertRawBytes, index);
                index += rSAParameters.DQ.Length;

                rSAParameters.InverseQ.CopyTo(privateCertRawBytes, index);

                privateCert = Convert.ToBase64String(privateCertRawBytes);
            }
            catch (Exception e)
            {
                throw new Exception(string.Format("Error Loading Private Key From Cert: {0}", CertSubjectName));
            }

            return privateCert;
        }
    }
}
