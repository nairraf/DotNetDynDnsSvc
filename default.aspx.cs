using DotNetDynDnsSvc.Model;
using DotNetDynDnsSvc.Server;
using DotNetDynDnsSvc.Common;
using System;
using System.Web.UI.WebControls;
using Farrworks.Crypto.Basic;

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

            // see if we have a query string
            var QueryStrings = manager.Request.QueryString;
            string htmlToReturn = "";

            // get our configuration instance
            ConfigurationManagerSingleton config = ConfigurationManagerSingleton.Instance;

            if (QueryStrings["action"].ToLower() == "updatedns")
            {
                if (!dbuser.IsPermitted("updatedns"))
                    manager.ReturnError(403, "Access is denied");

                htmlToReturn = String.Format(@"<div>
                                                Access Granted <br /> 
                                                User: {0} <br />
                                                Updated DNS Using IP: {1}
                                            </div>", 
                                            dbuser.username, manager.Request.UserHostAddress);
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

            if (QueryStrings["action"].ToLower() == "error")
            {
                if (!dbuser.IsPermitted("error"))
                    manager.ReturnError(403, "Access is denied");

                manager.ReturnError(500, "This is a test error");
            }

            Message.Controls.Add(new Literal() { Text = htmlToReturn });

            // TODO: update the associated DNS entry for this key.

            // build the data we want to log and log this request
            // TODO: log the DNS resource record that was updated, and if the update was successful or not.
            LogData log = new LogData();
            log.username = manager.userName;

            LogWriter logWriter = new LogWriter(Request);
            logWriter.WriteLine(log);
        }
    }
}