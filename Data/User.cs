using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DotNetDynDnsSvc.Data
{
    public class User
    {
        public string username { get; set; }
        public string key { get; set; }
        public string resourceRecord { get; set; }
        public string zone { get; set; }
        public bool isAuthenticated { get; set; }

        public User()
        {
            username = "";
            key = "";
            resourceRecord = "";
            zone = "";
            isAuthenticated = false;
        }
    }
}