using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DotNetDynDnsSvc.Class
{
    public class LogData
    {

        public string username { get; set; }
        public string password { get; set; }
        public string responseCode { get; set; }
        public string responseString { get; set; }

        public LogData()
        {
            responseCode = "200";
            responseString = "OK";
            username = "";
            password = "";
        }
    }
}