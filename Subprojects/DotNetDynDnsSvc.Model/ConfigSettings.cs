using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetDynDnsSvc.Model
{
    public class ConfigSettings
    {
        public string InitialSeed { get; set; }
        public string DnsServer { get; set; }
        public string DnsServerUserName { get; set; }
        public string DnsServerUserPasswordCipher { get; set; }
        public string RealClientIpHostHeader { get; set; }
        public int TTLSeconds { get; set; }
    }
}
