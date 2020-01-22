using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DotNetDynDnsSvc.Data
{
    public class Settings
    {
        public string InitialSeed { get; set; }
        public string DnsServer { get; set; }
        public string DnsServerUserName { get; set; }
        public string DnsServerUserPasswordCipher { get; set; }
    }
}