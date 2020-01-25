using DotNetDynDnsSvc.Model;
using DotNetDynDnsSvc.Server;
using System;
using System.Net;
using System.Web.UI.WebControls;

namespace DotNetDynDnsSvc
{
    public partial class _default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // create our http manager, and validate the incoming HTTP request
            // this makes sure that there is some kind of username/password present in the basic authentication HTTP header.
            httpManager manager = new httpManager(Request, Context);
            manager.Validate();

            // try and authenticatethe user. if we can't, error out and end the sesssion
            AuthManager dbAuth = new AuthManager();
            User dbuser = dbAuth.AuthenticateUser(manager.userName, manager.userPassword);
            if (dbuser.isAuthenticated == false)
            {
                manager.ReturnError(403, "Invalid username and/or password. Access is denied");
            }

            // user is now authenticated (valid user/key combo)

            // build the data we want to log and log this request
            // TODO: log the DNS resource record that was updated, and if the update was successful or not.
            LogData log = new LogData();
            log.username = manager.userName;

            // see if we have a query string
            var QueryStrings = manager.Request.QueryString;
            string htmlToReturn = "";

            // get our configuration instance
            ConfigurationManagerSingleton config = ConfigurationManagerSingleton.Instance;

            if (QueryStrings["action"].ToLower() == "updatedns")
            {
                if (!dbuser.IsPermitted("updatedns"))
                    manager.ReturnError(403, "Access is denied");

                string clientIP;
                // which host header should we check for
                string HttpClientIpHostHeader = string.Format("HTTP_{0}", config.Settings.RealClientIpHostHeader);

                if (manager.Request.ServerVariables.Get(HttpClientIpHostHeader) != null && manager.Request.ServerVariables.Get(HttpClientIpHostHeader).Length >0)
                {
                    // Real Client IP Host header detected - using it to determine the real client IP

                    htmlToReturn = String.Format(@"<div>
                                                    Access Granted <br /> 
                                                    User: {0} <br />
                                                    Updated DNS Record: {1}; HTTP Header: {2}; IP: {3}<br />
                                                </div>", 
                                                dbuser.username, String.Format("{0}.{1}", dbuser.resourceRecord, dbuser.zone), config.Settings.RealClientIpHostHeader, manager.Request.ServerVariables.Get(HttpClientIpHostHeader));

                    clientIP = manager.Request.ServerVariables.Get(HttpClientIpHostHeader);
                }
                else
                {
                    htmlToReturn = String.Format(@"<div>
                                                    Access Granted <br /> 
                                                    User: {0} <br />
                                                    Updated DNS Record: {1} Using IP: {2}<br />
                                                </div>",
                                                dbuser.username, String.Format("{0}.{1}", dbuser.resourceRecord, dbuser.zone), manager.Request.UserHostAddress);

                    clientIP = manager.Request.UserHostAddress;
                }

                // log that we are trying to update a dns record
                log.dnsRecord = dbuser.resourceRecord;
                log.dnsZone = dbuser.zone;

                // make sure the IP Address is IPv4
                var testIP = IPAddress.Parse(clientIP);
                if (testIP.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork)
                    manager.ReturnError(500, "only IPv4 Addresses can be used");

                // try to update the DNS record using ipAddress if it isn't current
                DnsUpdateManager dnsUpdater = new DnsUpdateManager(clientIP);
                if (!dnsUpdater.DnsRecordIsCurrent(dnsUpdater.GetDnsEntry(dbuser.resourceRecord, dbuser.zone)))
                {
                    bool dnsUpdatedSuccessfully = dnsUpdater.UpdateDnsEntry(dbuser.resourceRecord, dbuser.zone);
                    
                    if (!dnsUpdatedSuccessfully)
                        manager.ReturnError(500, string.Format("Error Updating DNS entry. DNS Update failed for {0}.{1}", dbuser.resourceRecord, dbuser.zone));

                    log.dnsUpdateStatus = string.Format("DNS Updated with IP: {0}", clientIP);
                } 
                else
                {
                    htmlToReturn = String.Format(@"<div>
                                                    Access Granted <br /> 
                                                    User: {0} <br />
                                                    DNS Record: {1} does not require updating. Already using IP: {2}<br />
                                                </div>",
                                                dbuser.username, String.Format("{0}.{1}", dbuser.resourceRecord, dbuser.zone), clientIP);

                    log.dnsUpdateStatus = string.Format("No Dns Update IP Is Current: {0}", clientIP);
                }
            }

            if (QueryStrings["action"].ToLower() == "test")
            {
                if (!dbuser.IsPermitted("test"))
                    manager.ReturnError(403, "Access is denied");

                htmlToReturn = String.Format(@"<div>
                                                Access Granted <br /> 
                                                User: {0} <br />
                                                Config Read Test: Dns Server to update: {1}
                                            </div>",
                                            dbuser.username, config.Settings.DnsServer);
            }

            if (QueryStrings["action"].ToLower() == "reloadconfiguration")
            {
                if (!dbuser.IsPermitted("reloadconfiguration"))
                    manager.ReturnError(403, "Access is denied");

                htmlToReturn = String.Format(@"<div>
                                                Access Granted <br /> 
                                                User: {0} <br />
                                                Reloading Configuration.
                                            </div>",
                                            dbuser.username);

                config.ReloadConfiguration();
            }

            if (QueryStrings["action"].ToLower() == "testerror")
            {
                if (!dbuser.IsPermitted("testerror"))
                    manager.ReturnError(403, "Access is denied");

                manager.ReturnError(500, "This is a test error");
            }

            Message.Controls.Add(new Literal() { Text = htmlToReturn });

            // TODO: update the associated DNS entry for this key.

            

            LogWriter logWriter = new LogWriter(Request);
            logWriter.WriteLine(log);
        }
    }
}