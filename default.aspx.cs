using DotNetDynDnsSvc.Data;
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


            // just echo out the username and password for now.
            AuthenticationModel dbAuth = new AuthenticationModel();
            User dbuser = dbAuth.AuthenticateUser(manager.userName, manager.userPassword);
            if (dbuser.isAuthenticated == false)
            {
                manager.ReturnError(403, "Invalid Username and/or password. Access is denied");
            }

            Message.Controls.Add(new Literal() { Text = String.Format("<div>Access Granted <br /> User: {0}, Password: {1}</div>", dbuser.username, dbuser.key) });

            // build the data we want to log and log it
            LogData log = new LogData();
            log.username = manager.userName;
            log.password = manager.userPassword;

            LogWriter logWriter = new LogWriter(Request);
            logWriter.WriteLine(log);
        }
    }
}