using DnsClient;
using DotNetDynDnsSvc.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Net;
using System.Web;

namespace DotNetDynDnsSvc.Server
{
    public class DnsUpdateManager
    {
        public string IpAddress { get; set; }
        private ConfigurationManagerSingleton _config;
        
        public DnsUpdateManager(string ipAddress) 
        {
            IpAddress = ipAddress;
            _config = ConfigurationManagerSingleton.Instance;
        }

        public bool UpdateDnsEntry(string ARecord, string zone)
        {
            bool updatedSuccessfully = false;

            try
            {
                // get our encryption manager to help us decrypt the cipherText
                string seed;
                if (_config.Settings.InitialSeed.Contains("Cert="))
                {
                    string[] initialSeedSplit = _config.Settings.InitialSeed.Split('=');
                    
                    if (initialSeedSplit.Count() != 2)
                        return false;

                    seed = Crypto.GetSeedFromCert(initialSeedSplit[1]);
                }
                else
                {
                    if (_config.Settings.InitialSeed.Length <= 0)
                        return false;
                    
                    seed = Crypto.GenerateSeed(_config.Settings.InitialSeed);
                }

                EncryptionManager ekm = new EncryptionManager(seed);

                ConnectionOptions options = new ConnectionOptions();
                options.Username = _config.Settings.DnsServerUserName;
                options.Password = ekm.Decrypt(_config.Settings.DnsServerUserPasswordCipher);

                PutOptions putOptions = new PutOptions();
                putOptions.Type = PutType.UpdateOnly;


                ManagementScope Scope = new ManagementScope(String.Format(@"\\{0}\root\MicrosoftDNS", _config.Settings.DnsServer), options);
                Scope.Connect();

                ObjectQuery query = new ObjectQuery(string.Format("select * from MicrosoftDNS_AType where OwnerName='{0}.{1}'", ARecord, zone));

                ManagementObjectSearcher searcher = new ManagementObjectSearcher(Scope, query);
                ManagementObjectCollection queryCollection = searcher.Get();
                foreach (ManagementObject m in queryCollection)
                {
                    if (m["OwnerName"].ToString().ToLower() == string.Format("{0}.{1}", ARecord.ToLower(), zone.ToLower()))
                    {
                        ManagementBaseObject properties = m.GetMethodParameters("Modify");
                        properties["IPAddress"] = IpAddress;
                        m.InvokeMethod("Modify", properties, null);
                    }
                }

                queryCollection.Dispose();
                searcher.Dispose();

                // check with DNS server if the update succedded
                updatedSuccessfully = DnsRecordIsCurrent(GetDnsEntry(ARecord, zone));
            }
            catch 
            {
                updatedSuccessfully = false;
            }

            return updatedSuccessfully;
        }

        public IEnumerable<DnsClient.Protocol.ARecord> GetDnsEntry(string ARecord, string zone)
        {
            var dnsServerHostName = Dns.GetHostEntry(_config.Settings.DnsServer);
            var dnsServerIP = new IPEndPoint(dnsServerHostName.AddressList[0], 53);
            var dnsClient = new LookupClient(dnsServerIP);

            var dnsQuery = dnsClient.Query(String.Format("{0}.{1}", ARecord, zone), QueryType.A);

            return dnsQuery.Answers.ARecords();
        }

        public bool DnsRecordIsCurrent(IEnumerable<DnsClient.Protocol.ARecord> ARecords)
        {
            foreach (var record in ARecords)
            {
                if (record.Address.ToString() == IpAddress)
                    return true;
            }

            return false;
        }
    }
}