using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using DotNetDynDnsSvc.Common;

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
                            if (arguments.ContainsKey(split[0]))
                            {
                                PrintHelp();
                                Environment.Exit(1);
                            }

                            arguments.Add(split[0].ToLower(), split[1].ToLower());
                        }
                    }
                    else
                    {
                        // the argument does not contain a value
                        arguments.Add(argument, "");
                    }
                }
            }

            if ( arguments.ContainsKey("/?") || arguments.ContainsKey("help") || 
                 arguments.ContainsKey("/help") || arguments.ContainsKey("-help") || arguments.ContainsKey("--help") ||
                 arguments.Count == 0)
            {
                PrintHelp();
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
                    Console.WriteLine(GetRandomString(rand.Next(48, 96)));
                    Console.ForegroundColor = ConsoleColor.White;
                }
            }

            if (arguments.ContainsKey("decrypt"))
            {
                if (arguments.ContainsKey("config"))
                {
                    if (!File.Exists(arguments["config"]))
                        throw new Exception("Invalid Config File");

                    ConfigManager cfg = new ConfigManager(arguments["config"]);

                    if (cfg.Settings.InitialSeed != null && cfg.Settings.InitialSeed != "")
                    {
                        EncryptionManager ekm = new EncryptionManager(Crypto.GenerateSeed(cfg.Settings.InitialSeed));
                        Console.WriteLine();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine("Decrypt Mode");
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write("Enter CipherText to Decrypt: ");
                        string cipherText = Console.ReadLine();
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine(string.Format("    {0}", ekm.Decrypt(cipherText)));
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine();
                    }
                }
                else
                {
                    PrintHelp();
                }
            }

            if (arguments.ContainsKey("encrypt"))
            {
                if (arguments.ContainsKey("config"))
                {
                    if (!File.Exists(arguments["config"]))
                        throw new Exception("Invalid Config File");

                    ConfigManager cfg = new ConfigManager(arguments["config"]);

                    if (cfg.Settings.InitialSeed != null && cfg.Settings.InitialSeed != "" )
                    {
                        EncryptionManager ekm = new EncryptionManager(Crypto.GenerateSeed(cfg.Settings.InitialSeed));
                        if (arguments["encrypt"].Length == 0)
                        {
                            Console.WriteLine();
                            Console.ForegroundColor = ConsoleColor.Blue;
                            Console.WriteLine("Encrypt Mode");
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.Write("Enter Text to Encrypt: ");
                            string toEncrypt = Console.ReadLine();
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine(string.Format("    {0}", ekm.Encrypt(toEncrypt)));
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.WriteLine();
                        }
                        else
                        {
                            Console.WriteLine("Encrypting: {0}", arguments["encrypt"]);
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine(string.Format("    {0}", ekm.Encrypt(arguments["encrypt"])));
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.WriteLine();
                        }
                    }
                }
                else
                {
                    PrintHelp();
                }
            }
        }

        static void PrintHelp()
        {
            string helpText = @"
               DotNetDynDNS Crypto Helper
               --------------------------

        Valid Options:

        /?, /help, help         this help page

        config=<path>           the path to a DotNetDynDnsSvc Yaml config file

        decrypt                 decrypt some text. you will be prompted for the
                                cipherText to decrypt.
                                you must specify a config file (see configuration)

        enrypt                  encrypt some text. you will be prompted for the
                                text to encrypt.
                                you must specify a config file (see config=<path>)

        encrypt=string          <string> is the string that should be encrypted
                                you must specify a config file (see config=<path>)
                                
        
        type=dnskey             Generate a new DotNetDynDnsSvc DNS key
                                Only a single type= argument can be used at a time

        type=seed               Generate a new DotNetDynDnsSvc encryption seed
                                Only a single type= argument can be used at a time

";
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(helpText);
            Console.ForegroundColor = ConsoleColor.White;
        }

        static string GetRandomString(int length)
        {
            RNGCryptoServiceProvider rand = new RNGCryptoServiceProvider();
            byte[] b = new byte[1];

            // 62 chararcters of all uppcase and lowercase letters and 0-9. valid indexes 0-61
            string characters = "qwertyuiopasdfghjklzxcvbnm0123456789QWERTYUIOPASDFGHJKLZXCVBNM";
            string ret = "";
            string previous = "";
            string current = "";
            bool fairRole;
            bool reRole;
            // our sliding window to look for duplicates
            int window = 10;
            for (int i = 0; i < length; i++) 
            {
                current = "";
                do
                {
                    // assume a fair role and no re-roll
                    fairRole = true;
                    reRole = false;

                    // produces a random single byte representing numbers from 0-255.
                    rand.GetBytes(b);
                    int roll = (int)b[0];

                    // figure out which character index we rolled
                    // 0-61 are valid indexes, we use them as is
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
                    {
                        fairRole = false;
                        reRole = true;
                        continue;
                    }

                    // assign our current character
                    current = characters[roll].ToString();
                    
                    // drop off the first character (the oldest) from the string
                    // until the string is the smaller than our window
                    while (previous.Length >= window)
                        previous = previous.Substring(1);

                    // make sure we don't have any repetition within our window
                    if (previous.ToLower().Contains(current.ToLower()))
                        reRole = true;
                } while (reRole == true);

                // add the new character to our string, and append current to previous to make sure we don't duplicate characters within our window
                ret += current;
                previous += current;
            }

            return ret;
        }
    }
}
