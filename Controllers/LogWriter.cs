using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DotNetDynDnsSvc.Controllers
{
    public class LogWriter
    {
        private string _logFileName;
        private string _dateStamp;
        private string _timeStamp;
        private string _dateTimeStamp;
        private DateTime _now;

        public string dateStamp { get { return _dateStamp; }  }
        public string timeStamp { get { return _timeStamp; }  }
        public string dateTimeStamp { get { return _dateTimeStamp; } }
        public DateTime nowTime { get { return _now; } }
        public LogWriter()
        {
            UpdateTime();
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
            _logFileName = String.Format(@"{0}\Logs\{1}.log", HttpRuntime.AppDomainAppPath, dateStamp);
        }

        public void WriteLine(List<string> msg)
        {
            string logline = "";
            for (int i = 0; i < msg.Count(); i++)
            {
                if (i == 0)
                    logline += msg[i];
                else
                    logline += String.Format(",{0}", msg[i]);
            }

            logline = String.Format("{0},{1}", dateTimeStamp, logline);

            using (System.IO.StreamWriter file = new System.IO.StreamWriter(_logFileName, true))
            {
                file.WriteLine(logline);
            }
        }

    }
}