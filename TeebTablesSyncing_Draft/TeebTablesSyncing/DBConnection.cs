using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeebTablesSyncing
{
    public struct DBInfo
    {
        public string DBCode;
        public string ServerName;
        public string DBName;
        public string DBPort;
        public string DBUser;
        public string DBPwd;
        public string ConnectionString;
    }

   public class DBConnection
    {
        public string GetConnectionString(DBInfo dbInfo)
        {
            return "Data Source=" + dbInfo.ServerName + ";Initial Catalog=" + dbInfo.DBName + ";User ID=root" + ";Password=metalic;";
        }
    }
}
