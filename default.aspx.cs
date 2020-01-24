using DotNetDynDnsSvc.Model;
using DotNetDynDnsSvc.Server;
using System;
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
            AuthenticationManager dbAuth = new AuthenticationManager();
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

                if (manager.Request.ServerVariables.Get("HTTP_X_FORWARDED_FOR") != null && manager.Request.ServerVariables.Get("HTTP_X_FORWARDED_FOR").Length >0)
                {
                    // X_FORWARDED_FOR detected - using that as real client IP

                    htmlToReturn = String.Format(@"<div>
                                                    Access Granted <br /> 
                                                    User: {0} <br />
                                                    Updated DNS Record: {1} Using X-FORWARDED-FOR: {2}<br />
                                                </div>", 
                                                dbuser.username, String.Format("{0}.{1}", dbuser.resourceRecord, dbuser.zone) ,manager.Request.ServerVariables.Get("HTTP_X_FORWARDED_FOR"));

                    string ipAddress = manager.Request.ServerVariables.Get("HTTP_X_FORWARDED_FOR");
                }
                else
                {
                    htmlToReturn = String.Format(@"<div>
                                                    Access Granted <br /> 
                                                    User: {0} <br />
                                                    Updated DNS Record: {1} Using IP: {2}<br />
                                                </div>",
                                                dbuser.username, String.Format("{0}.{1}", dbuser.resourceRecord, dbuser.zone), manager.Request.UserHostAddress);

                    string ipAddress = manager.Request.UserHostAddress;
                }

                // log that we are trying to update a dns record
                log.dnsRecord = dbuser.resourceRecord;
                log.dnsZone = dbuser.zone;

                // try to update the DNS record using ipAddress

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