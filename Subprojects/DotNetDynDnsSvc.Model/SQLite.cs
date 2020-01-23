using System;
using System.Data.SQLite;
using System.IO;

namespace DotNetDynDnsSvc.Model
{
    public class SQLite
    {
        private string _dbPath;
        public SQLite(string DBPath)
        {
            if (!File.Exists(DBPath))
                throw new Exception(string.Format("SQLite Database Not Found: {0}", DBPath));

            _dbPath = DBPath;
        }

        public User GetUser(string username, string key)
        {
            User dbUser = new User();

            try
            {
                string sql = String.Format("SELECT username,key,resourceRecord,zone,allowedActions FROM users WHERE username = '{0}' and key = '{1}'", username.Trim(), key.Trim());
                var dbConnection = new SQLiteConnection(string.Format(@"URI=file:{0}", _dbPath));
                dbConnection.Open();

                var sqlCmd = new SQLiteCommand(sql, dbConnection);
                SQLiteDataReader sqlReader = sqlCmd.ExecuteReader();

                while (sqlReader.Read())
                {
                    // see if we have a user name
                    if ((sqlReader.GetString(0)).Length > 0)
                    {
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
            }
            catch
            { 
                // fail silently, we will catch blank users at a higher level 
            }

            return dbUser;
        }



    }
}
