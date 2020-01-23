using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace KeyGenerator
{
    class KeyGenerator
    {
        static void Main(string[] args)
        {
            var arguments = new Dictionary<string, string>();
            if (args.Length > 0)
            {
                foreach (string argument in args)
                {
                    if (argument.Contains('='))
                    {
                        string[] split = argument.Split('=');
                        if (split.Length == 2)
                        {
                            arguments.Add(split[0].ToLower(), split[1].ToLower());
                        }
                    }
                }
            }

            // valid args to test for
            // type = [ seed | dnskey ]

            if ( arguments.ContainsKey("type") )
            {
                // generate a new seed
                if ( arguments["type"] == "seed" )
                {
                    // we will make 3 parts of random lenghts and concatenate them together
                    Random rand = new Random();
                    int r1 = rand.Next(20, 40);
                    int r2 = rand.Next(15, 30);
                    int r3 = rand.Next(30, 50);
                    string first = GetRandomString(r1);
                    string second = GetRandomString(r2);
                    string third = GetRandomString(r3);

                    Console.WriteLine();
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write("New seed value: ");
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(string.Format("{0}+{1}/{2}", first, second, third));
                    Console.ForegroundColor = ConsoleColor.White;
                }

                // generate a new DNS dynamic update key
                if ( arguments["type"] == "dnskey" )
                {
                    //we will create a random string of a random lengh and return it
                    Random rand = new Random();
                    Console.WriteLine();
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write("New DNS Key: ");
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(GetRandomString(rand.Next(72, 128)));
                    Console.ForegroundColor = ConsoleColor.White;
                }
            }
        }

        static string GetRandomString(int length)
        {
            RNGCryptoServiceProvider rand = new RNGCryptoServiceProvider();
            byte[] b = new byte[1];

            // 62 chararcters of all uppcase and lowercase letters and 0-9. valid indexes 0-61
            string characters = "qwertyuiopasdfghjklzxcvbnm0123456789QWERTYUIOPASDFGHJKLZXCVBNM";
            string ret = "";
            string previous = "";
            for (int i = 0; i < length; i++) 
            {
                string current = "";
                bool fairRole;
                do
                {
                    // produces a random single byte representing numbers from 0-255.
                    rand.GetBytes(b);
                    int roll = (int)b[0];

                    // assume a fair role
                    fairRole = true;

                    // first repetition of 0-61. subtract 62 from the number to get the correct index
                    if (roll >= 62 && roll <= 123)
                        roll -= 62;
                    // second repetition of 0-61. subtract 124 from the number to get the correct index
                    if (roll >= 124 && roll <= 185)
                        roll -= 124;
                    // third repetition of 0-61. subtract 186 from the number to get the correct index
                    if (roll >= 186 && roll <= 247)
                        roll -= 186;
                    // not a fairRole - re-roll
                    if (roll >= 248 && roll <= 255)
                        fairRole = false;
                    // if the role is good, assign the current character
                    if (fairRole)
                        current = characters[roll].ToString();

                } while (previous == current || fairRole == false);

                // add the new character to our string, and assign current as previous to make sure we don't get back-to-back characters
                ret += current;
                previous = current;
            }

            return ret;
        }
    }
}
