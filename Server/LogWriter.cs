using DotNetDynDnsSvc.Model;
using System;
using System.IO;
using System.Threading;
using System.Web;

namespace DotNetDynDnsSvc.Server
{
    public class LogWriter
    {
        // private static member to help control synchronous writing to the log file
        private static ReaderWriterLockSlim _readWriteLock = new ReaderWriterLockSlim();

        private string _logFileName;
        private string _dateStamp;
        private string _timeStamp;
        private string _dateTimeStamp;
        private HttpRequest _httpRequest;
        private string _requestMethod;
        private string _httpUserAgent;
        private string _remoteHost;
        private string _remoteAddress;
        private string _queryString;
        private string _url;
        private string _serverProtocol;
        private DateTime _now;
        

        public string dateStamp { get { return _dateStamp; }  }
        public string timeStamp { get { return _timeStamp; }  }
        public string dateTimeStamp { get { return _dateTimeStamp; } }
        public DateTime nowTime { get { return _now; } }

        public LogWriter(HttpRequest request)
        {
            _httpRequest = request;
            UpdateTime();
            _requestMethod = _httpRequest.ServerVariables.Get("REQUEST_METHOD");
            _httpUserAgent = _httpRequest.ServerVariables.Get("HTTP_USER_AGENT");
            _remoteHost = _httpRequest.ServerVariables.Get("REMOTE_HOST");
            _remoteAddress = _httpRequest.ServerVariables.Get("REMOTE_ADDR");
            _queryString = _httpRequest.ServerVariables.Get("QUERY_STRING");
            _url = _httpRequest.ServerVariables.Get("URL");
            _serverProtocol = _httpRequest.ServerVariables.Get("SERVER_PROTOCOL");
        }

        public void UpdateTime()
        {
            DateTime _now = DateTime.Now;
            string day = (_now.Day).ToString();
            string month = (_now.Month).ToString();
            string year = (_now.Year).ToString();
            string hour = (_now.Hour).ToString();
            string minute = (_now.Minute).ToString();
            string second = (_now.Second).ToString();

            _dateStamp = String.Format("{0}-{1}-{2}", year, month, day);
            _timeStamp = String.Format("{0}:{1}:{2}", hour, minute, second);
            _dateTimeStamp = String.Format("{0}-{1}", _dateStamp, _timeStamp);
            _logFileName = String.Format(@"{0}\{1}.log", Path.GetFullPath(Path.Combine(HttpRuntime.AppDomainAppPath, @"..\Logs")), dateStamp);
        }

        public void WriteLine(LogData logData)
        {
            // we want the log line to look like:
            //      dateTimeStamp,RemoteAddress,RemoteHost,ServerProtocol,RequestMethod,HttpUserAgent,url,querystring,responseCode,responseString,username,password

            string logline = String.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11}", 
                            dateTimeStamp, _remoteAddress, _remoteHost, _serverProtocol, _requestMethod, _httpUserAgent, _url, _queryString, 
                            logData.responseCode, logData.responseString ,logData.username, logData.password );
            
            // see if the file exists, if it doesn't exist, create it and fill it with the csv header line
            if (File.Exists(_logFileName) == false)
            {
                DoWrite("dateTimeStamp,remoteAddress,remoteHost,serverProtocol,requestMethod,httpUserAgent,url,querstring,responseCode,responseString,username,password");
            }

            //write the new log entry
            DoWrite(logline);
        }

        private void DoWrite(string msg)
        {
            // write the log message, try and set the lock, write, and release lock
            _readWriteLock.EnterWriteLock();
            try
            {
                using (System.IO.StreamWriter file = new StreamWriter(_logFileName, true))
                {
                    file.WriteLine(msg);
                    file.Close();
                }
            }
            finally
            {
                _readWriteLock.ExitWriteLock();
            }
        }
    }
}