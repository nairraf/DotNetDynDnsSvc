namespace DotNetDynDnsSvc.Model
{
    public class LogData
    {

        public string username { get; set; }
        public string password { get; set; }
        public string responseCode { get; set; }
        public string responseString { get; set; }
        public string dnsRecord { get; set; }
        public string dnsZone { get; set; }

        public LogData()
        {
            responseCode = "200";
            responseString = "OK";
            username = "";
            password = "";
        }
    }
}