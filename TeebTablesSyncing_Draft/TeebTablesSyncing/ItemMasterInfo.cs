using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeebTablesSyncing
{
   public class ItemMasterInfo
    {
        public string table { get; set; }
        public ArrayList primary_key { get; set; }
        public string mode { get; set; }
      
       public DataTable data { get; set; }

       
    }
}
