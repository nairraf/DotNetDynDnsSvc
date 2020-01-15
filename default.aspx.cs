using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using DotNetDynDnsSvc.Class;
using DotNetDynDnsSvc.Controllers;

namespace DotNetDynDnsSvc
{
    public partial class _default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // create our authenticator, and validate the incoming HTTP request
            // this makes sure that there is some kind of username/password present in the basic authentication HTTP header.
            Authenticator auth = new Authenticator(Request, Context);
            auth.Validate();


            // just echo out the username and password for now.
            Message.Controls.Add(new Literal() { Text = String.Format("<div>Access Granted <br /> User: {0}, Password: {1}</div>", auth.userName, auth.userPassword) });

            // build the data we want to log and log it
            LogData log = new LogData();
            log.username = auth.userName;
            log.password = auth.userPassword;

            LogWriter logWriter = new LogWriter(Request);
            logWriter.WriteLine(log);
        }
    }
}