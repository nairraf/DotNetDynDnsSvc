using DotNetDynDnsSvc.Model;
using System;
using System.Data.SQLite;
using System.IO;
using System.Web;

namespace DotNetDynDnsSvc.Server
{
    public class AuthenticationManager
    {
        private string _dbSource;

        public AuthenticationManager()
        {
            _dbSource = String.Format(@"URI=file:{0}\auth.db", Path.GetFullPath(Path.Combine(HttpRuntime.AppDomainAppPath, @"..\DB")));
        }

        public User AuthenticateUser(string username, string key)
        {
            string sql = String.Format("SELECT username,key,resourceRecord,zone,allowedActions FROM users WHERE username = '{0}' and key = '{1}'", username.Trim(), key.Trim());
            var dbConnection = new SQLiteConnection(_dbSource);
            dbConnection.Open();

            var sqlCmd = new SQLiteCommand(sql, dbConnection);
            SQLiteDataReader sqlReader = sqlCmd.ExecuteReader();

            User dbUser = new User();
            while (sqlReader.Read())
            {
                // see if we have a user name
                if ( (sqlReader.GetString(0)).Length > 0 )
                {
                    dbUser.isAuthenticated = true;
                    dbUser.username = sqlReader.GetString(0);
                    dbUser.key = sqlReader.GetString(1);
                    dbUser.resourceRecord = sqlReader.GetString(2);
                    dbUser.zone = sqlReader.GetString(3);
                    dbUser.LoadActions(sqlReader.GetString(4));
                }
            }

            sqlReader.Close();
            sqlCmd.Dispose();
            dbConnection.Close();
            dbConnection.Dispose();

            return dbUser;
        }

    }
}