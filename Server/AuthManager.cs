using DotNetDynDnsSvc.Model;
using System;
using System.Data.SQLite;
using System.IO;
using System.Web;

namespace DotNetDynDnsSvc.Server
{
    public class AuthManager
    {
        private string _dbPath;

        public AuthManager()
        {
            _dbPath = String.Format(@"{0}\auth.db", Path.GetFullPath(Path.Combine(HttpRuntime.AppDomainAppPath, @"..\DB")));
        }

        public User AuthenticateUser(string username, string key)
        {
            SQLite sql = new SQLite(_dbPath);
            User user = sql.GetUser(username, key);

            // see if we found a record
            if (user.username != "" && user.key != "")
                user.isAuthenticated = true;

            return user;
        }

    }
}