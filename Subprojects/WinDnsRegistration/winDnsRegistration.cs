using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Security.Cryptography;
using System.Text;

namespace WinDnsRegistration
{
    class winDnsRegistration
    {
        static void Main(string[] args)
        {
            //AddARecord("testWMI", "millercom.net", "1.1.2.2", "montremdns01.na.future.ca");
            UpdateARecord("testWMI", "millercom.net", "1.1.2.20", "montremdns01.na.future.ca");

        }

        public static void AddARecord(string hostName, string zone, string ipAddress, string dnsServerName)
        {
            ConnectionOptions options = new ConnectionOptions();
            options.Username = @"montremdns01\dnsadmin";
            options.Password = "password";
            ManagementScope scope = new ManagementScope(String.Format(@"\\{0}\root\MicrosoftDNS", dnsServerName), options);
            scope.Connect();

            ManagementClass wmiClass = new ManagementClass(scope, new ManagementPath("MicrosoftDNS_AType"), null);

            ManagementBaseObject propertyData = wmiClass.GetMethodParameters("CreateInstanceFromPropertyData");

            propertyData["DnsServerName"] = dnsServerName;
            propertyData["ContainerName"] = zone;
            propertyData["OwnerName"] = String.Format("{0}.{1}", hostName, zone);
            propertyData["IPAddress"] = ipAddress;

            wmiClass.InvokeMethod("CreateInstanceFromPropertyData", propertyData, null);

            wmiClass.Dispose();
        }
        
        public static void UpdateARecord(string hostName, string zone, string ipAddress, string dnsServerName)
        {
            ConnectionOptions options = new ConnectionOptions();
            options.Username = @"montremdns01\dnsadmin";
            options.Password = "password";

            PutOptions putOptions = new PutOptions();
            putOptions.Type = PutType.UpdateOnly;


            ManagementScope Scope = new ManagementScope(String.Format(@"\\{0}\root\MicrosoftDNS", dnsServerName), options);
            Scope.Connect();

            ObjectQuery query = new ObjectQuery(string.Format("select * from MicrosoftDNS_AType where OwnerName='{1}.{0}'", zone, hostName));

            ManagementObjectSearcher searcher = new ManagementObjectSearcher(Scope, query);
            ManagementObjectCollection queryCollection = searcher.Get();
            foreach (ManagementObject m in queryCollection)
            {
                if (m["OwnerName"].ToString().ToLower() == string.Format("{0}.{1}", hostName.ToLower(), zone.ToLower()))
                {
                    ManagementBaseObject properties = m.GetMethodParameters("Modify");
                    properties["IPAddress"] = ipAddress;
                    m.InvokeMethod("Modify", properties, null);
                }
            }
        }

    }
}
