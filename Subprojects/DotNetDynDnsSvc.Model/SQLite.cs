using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DotNetDynDnsSvc.Model
{
    class SQLite
    {
        private string _dbPath;
        public SQLite(string DBPath)
        {
            if (!File.Exists(DBPath))
                throw new Exception(string.Format("SQLite Database Not Found: {0}", DBPath));

            _dbPath = DBPath;
        }



    }
}
