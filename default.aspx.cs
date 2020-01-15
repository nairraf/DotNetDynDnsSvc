using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using DotNetDynDnsSvc.Controllers;

namespace DotNetDynDnsSvc
{
    public partial class _default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Authenticator auth = new Authenticator(Request, Context);
            if (auth.Validate())
            {
                Message.Controls.Add(new Literal() { Text = String.Format("<div>Access Granted <br /> User: {0}, Password: {1}</div>", auth.userName, auth.userPassword) });
            }
        }
    }
}