using DotNetDynDnsSvc.Data;
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
            AuthenticationModel dbAuth = new AuthenticationModel();
            User dbuser = dbAuth.AuthenticateUser(manager.userName, manager.userPassword);
            if (dbuser.isAuthenticated == false)
            {
                manager.ReturnError(403, "Invalid username and/or password. Access is denied");
            }

            ConfigurationManagerSingleton config = ConfigurationManagerSingleton.Instance;

            // user is valid, display access granted on page and process DNS update
            Message.Controls.Add(new Literal() { Text = String.Format(@"<div>Access Granted <br /> User: {0}<br />{1}</div>", dbuser.username, config.Settings.DnsServer) });

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