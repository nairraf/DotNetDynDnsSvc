using DotNetDynDnsSvc.Class;
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
        private string _userName = "";
        private string _userPassword = "";

        public string userName { get { return _userName; } }
        public string userPassword { get { return _userPassword; } }

        public Authenticator(HttpRequest httpRequest, HttpContext httpContext)
        {
            _request = httpRequest;
            _context = httpContext;
        }

        public bool Validate()
        {
            // validate that this request is a get request, we only process get requests, all others are forbidden
            if ( _request.ServerVariables.Get("REQUEST_METHOD") != "GET" )
            {
                _context.Response.Status = "400 Request Type Not Supported";
                _context.Response.StatusCode = 400;
                _context.Response.Clear();

                // build the data we want to log and log it
                LogData log = new LogData();
                log.responseCode = "400";
                log.responseString = "Request Type Not Supported";

                LogWriter logWriter = new LogWriter(_request);
                logWriter.WriteLine(log);

                // end our connection with the browser
                _context.Response.End();
                return false;
            }

            // get the raw authentication header from the HTTP stream
            // validate that it's not empty
            // and that it can be decoded and parsed into a username and password
            // make sure that we have set the _userName and _userPassword appropriately
            // return true on a successful validation
            //      NOTE: this does not authenticate the user
            //            it just validates that there is a username/password in the HTTTP request.
            // return false if not successful for any reason
            // if not successful, return 403 forbidden or 400 bad request if
            // the HTTP request cannot be validated for some reason.
            string authHeader = _request.ServerVariables.Get("HTTP_AUTHORIZATION");
            if (authHeader == null || authHeader == "")
            {
                _context.Response.Status = "403 Forbidden";
                _context.Response.StatusCode = 403;
                _context.Response.Clear();
                
                // build the data we want to log and log it
                LogData log = new LogData();
                log.responseCode = "403";
                log.responseString = "Forbidden";

                LogWriter logWriter = new LogWriter(_request);
                logWriter.WriteLine(log);

                // end our connection with the browser
                _context.Response.End();
                return false;
            }

            string encodedAuth, decodedAuth;
            bool authError = false;

            if (authHeader.ToLower().Contains("basic"))
            {
                encodedAuth = (authHeader.Split(' '))[1];
                decodedAuth = System.Text.ASCIIEncoding.ASCII.GetString(System.Convert.FromBase64String(encodedAuth));
                if (decodedAuth.Contains(":"))
                {
                    string[] decodedSplit = decodedAuth.Split(':');
                    if (decodedSplit.Count() == 2)
                    {
                        if (decodedSplit[0] != null)
                        {
                            if (decodedSplit[0].Length > 0)
                                _userName = decodedSplit[0];
                        }

                        if (decodedSplit[1] != null)
                        {
                            if (decodedSplit[1].Length > 0)
                                _userPassword = decodedSplit[1];
                        }
                    }
                    
                    if (userName.Length <= 0 || userPassword.Length <= 0 )
                    {
                        authError = true;
                    }
                }
            }
            else
            {
                authError = true;
            }

            // return error 400 bad request and return false if the HTTP request cannot be successfully validated
            if (authError)
            {
                _context.Response.Status = "400 Bad Request";
                _context.Response.StatusCode = 400;
                _context.Response.Clear();
                
                // build the data we want to log and log it
                LogData log = new LogData();
                log.responseCode = "400";
                log.responseString = "Bad Request";

                LogWriter logWriter = new LogWriter(_request);
                logWriter.WriteLine(log);

                // end our connection with the browser
                _context.Response.End();
                return false;
            }

            // if we get here, then we return true as the userName and userPassword properties have been successfully set.
            return true;
        }
    }
}