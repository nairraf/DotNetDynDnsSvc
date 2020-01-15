using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DotNetDynDnsSvc.Controllers
{
    public class Authenticator
    {
        private HttpRequest _request;
        private HttpContext _context;

        public string userName { get; set; }
        public string userPassword { get; set; }

        public Authenticator(HttpRequest httpRequest, HttpContext httpContext)
        {
            _context = httpContext;
            _request = httpRequest;
        }

        public bool Validate()
        {
            string authHeader = _request.ServerVariables.Get("HTTP_AUTHORIZATION");
            if (authHeader == null || authHeader == "")
            {
                _context.Response.Status = "403 Forbidden";
                _context.Response.StatusCode = 403;
                _context.Response.Clear();
                _context.Response.End();
                return false;
            }

            string encodedAuthHeader, decodedAuthHeader;
            bool authError = false;

            if (authHeader.ToLower().Contains("basic"))
            {
                encodedAuthHeader = (authHeader.Split(' '))[1];
                decodedAuthHeader = System.Text.ASCIIEncoding.ASCII.GetString(System.Convert.FromBase64String(encodedAuthHeader));
                if (decodedAuthHeader.Contains(":"))
                {
                    string[] decodedSplit = decodedAuthHeader.Split(':');
                    userName = decodedSplit[0];
                    userPassword = decodedSplit[1];
                    if (userName.Length <= 0 || userPassword.Length <= 0 || userPassword == null || userName == null)
                    {
                        authError = true;
                    }
                }
            }
            else
            {
                authError = true;
            }

            if (authError)
            {
                _context.Response.Status = "400 Bad Request";
                _context.Response.StatusCode = 400;
                _context.Response.Clear();
                _context.Response.End();
                return false;
            }

            return true;
        }

    }
}