using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System.Collections;
using System.Net;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;
using System.Linq;

namespace TeebTablesSyncing
{
    public class DataAccess
    {
        #region Variable Declarations
        //public string Location = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        public DBInfo dbInfo = new DBInfo();
        public DBConnection dbConnection = new DBConnection();

        public DataSet ds = new DataSet();
        public DataTable dtUpdatedData = new DataTable();
        public DataTable dtCratedData = new DataTable();
        public DataTable dtReadData = new DataTable();
        public DataTable dtPrimaryKey = new DataTable();
        public DataTable dtblDatabaseTables = new DataTable();
        public DataTable dtblTableDetails = new DataTable();
        public DataTable dttblCustomerDetails = new DataTable();


        public int flag = 0, Utbl=0,Ctbl=0;
        public string tableName = "", path = "", mode = "", LastModifiedDate = "",LastCreatedDate="", Modified_Date, CreatedDate;
        public string xmlPath, xmlSetingsFile, ConnectionString, FileName="", Updated, Created;
        public string[] Tables = new string[100] ;
        public string product_id, Password,SerialNo,Code;
        public Boolean Decrypt = true;

        public List<ItemMasterInfo> UpdatedItem = new List<ItemMasterInfo>();
        public List<ItemMasterInfo> CreatedItem = new List<ItemMasterInfo>();
        public List<ItemMasterInfo> ReadItem = new List<ItemMasterInfo>();
        JsonSerializer serializer = new JsonSerializer();

        public string FolderName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TableSynching");

        GetData objGetData = new GetData();
        Encode objEncode = new Encode();
        #endregion

        #region Functions

        /*Read the Database and tables
         `````````````````````````````
        */

        public void ReadDatabase()
        {
            try
            {
                 //xmlPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                xmlPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                if (Directory.Exists(xmlPath))
                {
                    string[] subDirectories = Directory.GetDirectories(xmlPath);
                    for (int ini = 0; ini < subDirectories.Length - 1; ini++)
                    {
                        var DirName = subDirectories[ini];
                        xmlSetingsFile = DirName + "\\Settings.xml";
                        if ( DirName == xmlPath +"\\"+ "ePlus Basic Edition")
                        {
                            ds.Clear();
                            ds.ReadXml(xmlSetingsFile);
                            for (int i = 0; i <= ds.Tables[0].Rows.Count - 1; i++)
                            {
                                dbInfo.ServerName = ds.Tables[0].Rows[i]["SERVER"].ToString();
                                dbInfo.DBName = ds.Tables[0].Rows[i]["DATABASE"].ToString();
                                dbInfo.DBPort = ds.Tables[0].Rows[i]["PORT"].ToString();
                                Password = dbInfo.DBName;
                            }
                            ConnectionString = dbConnection.GetConnectionString(dbInfo);
                            MySqlConnection MysqlCon = new MySqlConnection(ConnectionString);
                            MysqlCon.Open();
                            //product_id = objGetData.GetProductId(MysqlCon, "R01", Password, Decrypt);
                            
                           // 
                            dtblDatabaseTables = GetTableData(string.Format("SELECT DISTINCT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = " + "\'" + dbInfo.DBName + "\'")); // Reading the tables
                            GetDetailsFromServer();
                            GetJsonFile();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                MessageBox.Show("Database-Can't Synchronize the data, Please try again");
            }
            finally
            {

            }
        }

        public void GetDetailsFromServer()
        {
            string response = "";
            try
            {
                 Code = objEncode.Encoding("CFF3249224E04E279EC6F295B0E3266C");
                //Code = objClsWeb.Encoding(product_id);
                //string BaseAddress = "http://milestoneit.net/api/keycode/encode?name="+Code;
                string BaseAddress = "http://milestoneit.net/api/pd/interact/" + Code;
                // Create a request using a URL that can receive a post. 
                HttpWebRequest webreq = (HttpWebRequest)WebRequest.Create(BaseAddress);
                webreq.Method = "GET";
                webreq.ContentType = "Text";
                
                HttpWebResponse webresp = (HttpWebResponse)webreq.GetResponse();
                // Get the stream associated with the response.
                Stream receiveStream = webresp.GetResponseStream();
                // Pipes the stream to a higher level stream reader with the required encoding format. 
                StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8);
                var encoding = ASCIIEncoding.ASCII;
                using (var reader = new System.IO.StreamReader(webresp.GetResponseStream(), encoding))
                {
                     response = reader.ReadToEnd();
                    //object Object = new JavaScriptSerializer().DeserializeObject(response);
                    dttblCustomerDetails = JsonStringToDataTable(response);
                    dtblTableDetails = GetDataTableFromJsonString(response);
                }
                //ReadDatabase();
            }
            catch(Exception ex)
            {
               
                MessageBox.Show(ex.ToString());
                }
        }



        public DataTable JsonStringToDataTable(string jsonString)
        {
            DataTable dt = new DataTable();
            string[] jsonStringArray = Regex.Split(jsonString.Replace("[", "").Replace("]", ""), "},{");
            List<string> ColumnsName = new List<string>();
            foreach (string jSA in jsonStringArray)
            {
                string[] jsonStringData = Regex.Split(jSA.Replace("{", "").Replace("}", ""), ",");
                foreach (string ColumnsNameData in jsonStringData)
                {
                    try
                    {
                        int idx = ColumnsNameData.IndexOf(":");
                        string ColumnsNameString = ColumnsNameData.Substring(0, idx - 1).Replace("\"", "");
                        if (!ColumnsName.Contains(ColumnsNameString))
                        {
                            ColumnsName.Add(ColumnsNameString);
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(string.Format("Error Parsing Column Name : {0}", ColumnsNameData));
                    }
                }
                break;
            }
            foreach (string AddColumnName in ColumnsName)
            {
                dt.Columns.Add(AddColumnName);
            }
            foreach (string jSA in jsonStringArray)
            {
                string[] RowData = Regex.Split(jSA.Replace("{", "").Replace("}", ""), ",");
                DataRow nr = dt.NewRow();
                foreach (string rowData in RowData)
                {
                    try
                    {
                        int idx = rowData.IndexOf(":");
                        string RowColumns = rowData.Substring(0, idx - 1).Replace("\"", "");
                        string RowDataString = rowData.Substring(idx + 1).Replace("\"", "");
                        nr[RowColumns] = RowDataString;
                    }
                    catch (Exception ex)
                    {
                        continue;
                    }
                }
                dt.Rows.Add(nr);
            }
            return dt;
        }





        public DataTable GetDataTableFromJsonString(string json)
        {
            var jsonLinq = JObject.Parse(json);

            // Find the first array using Linq
            var srcArray = jsonLinq.Descendants().Where(d => d is JArray).First();
            var trgArray = new JArray();
            foreach (JObject row in srcArray.Children<JObject>())
            {
                var cleanRow = new JObject();
                foreach (JProperty column in row.Properties())
                {
                    // Only include JValue types
                    if (column.Value is JValue)
                    {
                        cleanRow.Add(column.Name, column.Value);
                    }
                }
                trgArray.Add(cleanRow);
            }

            return JsonConvert.DeserializeObject<DataTable>(trgArray.ToString());
        }
        

        /* Fetch values from Database
          ``````````````````````````
       */

        private DataTable GetTableData(string qury)
        {
            DataTable dt = new DataTable();
            try
            {
                
                MySqlCommand sqlCmd = new MySqlCommand(qury);
                using (MySqlConnection mysqlCon = new MySqlConnection(ConnectionString))
                {
                  using (MySqlDataAdapter mysqlDa = new MySqlDataAdapter())
                  {
                        mysqlCon.Open();
                        sqlCmd.Connection = mysqlCon;
                        mysqlDa.SelectCommand = sqlCmd;
                        mysqlDa.Fill(dt);
                  }
                }
            }
            catch (Exception ex)
            { MessageBox.Show("Datatable-Can't Synchronize the data, Please try again"); }
            finally { }
            return dt;
        }

        /* Convert tables' data to Json format
           ``````````````````````````````````
        */
       
        public void GetJsonFile()
        {         
            try
            {              
                for (int i = 0; i < dtblTableDetails.Rows.Count; i++)
                {
                    tableName = dtblTableDetails.Rows[i]["table"].ToString();
                    for (int j = 0; j < dtblDatabaseTables.Rows.Count; j++)
                     {
                        string tb = dtblDatabaseTables.Rows[j]["table_name"].ToString();
                        if (tableName == tb)
                         {
                            LastCreatedDate = dtblTableDetails.Rows[0]["last_created"].ToString();
                            LastModifiedDate = dtblTableDetails.Rows[0]["last_updated"].ToString();
                            FetchTableData();
                         }
                     }
                }
                //Updated = objEncode.Encoding(Updated);
               

                if (UpdatedItem.Count != 0)
                {
                    JsonConvert.SerializeObject(UpdatedItem);

                    PostJson(UpdatedItem);
                    UpdatedItem.Clear();
                }
                if (CreatedItem.Count != 0)
                {
                    JsonConvert.SerializeObject(CreatedItem);
                    
                    PostJson(CreatedItem);
                    CreatedItem.Clear();
                }
                if (ReadItem.Count != 0)
                {
                     JsonConvert.SerializeObject(ReadItem);
                    
                    PostJson(ReadItem);
                    ReadItem.Clear();
                }
                flag = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                MessageBox.Show("Json-Can't Synchronize the data, Please try again");
            }
        }

        /* Upload to Url
           `````````````
        */
        public class WebClientEx : WebClient
        {
            public int Timeout { get; set; }

            protected override WebRequest GetWebRequest(Uri address)
            {
                var request = base.GetWebRequest(address);
                request.Timeout = Timeout;
                return request;
            }

           
        }
        public void PostJson(List<ItemMasterInfo> Json)
        {
            try
            {
                string FileName = Json.ToString();
                var BaseAddress = "http://demopd.milestoneit.online/interact";
                var wc = new WebClientEx();
                wc.Headers.Add("Content-type", "application/json");
                wc.Timeout = 9000000;
                wc.Encoding = Encoding.UTF8;
                byte[] response = wc.UploadFile(BaseAddress, FileName);
                string result = Encoding.UTF8.GetString(response);                 
            }
            catch (WebException ex)
            {
                 MessageBox.Show(ex.ToString());
                var pageContent = new StreamReader(ex.Response.GetResponseStream())
                          .ReadToEnd();
            }
        }

        /* To retrieve the latest modified and created dates
           `````````````````````````````````````````````````
        */
        public void RetrieveLatestDates()
        {
            try
            {
                DataTable dtLatestModifiedDates = GetTableData(("SELECT DATE_FORMAT(MAX(MODIFIED_DATE),'%Y-%m-%d %T') AS Modified_Date  FROM " + tableName + " WHERE MODIFIED_USER IS NOT NULL").ToString());
                LastModifiedDate = dtLatestModifiedDates.Rows[0]["Modified_Date"].ToString();
                DataTable dtLatestCreatedDate = GetTableData(string.Format("SELECT DATE_FORMAT(MAX(CREATED_DATE),'%Y-%m-%d %T') AS Created_Date  FROM " + tableName + " WHERE MODIFIED_USER IS NULL"));
                LastCreatedDate = dtLatestCreatedDate.Rows[0]["Created_Date"].ToString();
                if(LastCreatedDate != "")
                {
                    if (Utbl == 0)
                    {
                         Created = "id="+dttblCustomerDetails.Rows[0]["id"].ToString() + "&" + tableName + "_Created"+ "= " + LastCreatedDate;
                    }
                    if(Utbl!=0)
                    {
                        Updated = Updated + "&" + tableName+"_Created"+ "= " + LastCreatedDate;
                    }Utbl++;
                }
                if(LastModifiedDate!="")
                {
                    if(Ctbl==0)
                    {
                        Updated = dttblCustomerDetails.Rows[0]["id"].ToString() + "&" + tableName+"_Updated"+ "= " + LastModifiedDate;
                    }
                    else if(Ctbl!=0)
                    {
                        Created = Created + "&" + tableName + "_Updated" + LastModifiedDate;
                    }
                    Ctbl++;
                }
                
               
                
            }
            catch (Exception ex)
            {
                MessageBox.Show("Can't Synchronize the data, Please try again");
            }
        }

        /* Fetching data from the tables and save into List
           ````````````````````````````````````````````````
        */
        public void FetchTableData()
        {
           
            ArrayList Primary = new ArrayList();
            dtPrimaryKey = GetTableData(string.Format("SHOW KEYS FROM " + tableName + " WHERE KEY_NAME = 'PRIMARY'"));// Getting primarykeys from the tables

            if (LastCreatedDate==string.Empty||LastModifiedDate==string.Empty)
            {
                dtCratedData = GetTableData(string.Format("SELECT * FROM " + tableName));
            }
            else
            {
                dtUpdatedData = GetTableData(string.Format("SELECT * FROM " + tableName + " WHERE MODIFIED_DATE>" + "\'" + LastModifiedDate + "\'" + " AND MODIFIED_USER IS NOT NULL").ToString());
                dtCratedData = GetTableData(string.Format("SELECT * FROM " + tableName + " WHERE CREATED_DATE>" + "\'" + LastCreatedDate + "\'" + " AND MODIFIED_USER IS NULL").ToString());
            }

            for (int j = 0; j < dtPrimaryKey.Rows.Count; j++)
            {
            Primary.Add(dtPrimaryKey.Rows[j]["COLUMN_NAME"]);
            }
            if (dtUpdatedData.Rows.Count > 0)
            {
                int co = dtUpdatedData.Rows.Count;
                mode = "update";
                UpdatedItem.Add(new ItemMasterInfo
                {
                    table = tableName,
                    primary_key = Primary,
                    mode = mode,
                    data = dtUpdatedData,

                });
            }
            if (dtCratedData.Rows.Count > 0)
            {
                int co = dtCratedData.Rows.Count;
                mode = "create";
                CreatedItem.Add(new ItemMasterInfo
                {
                    table = tableName,
                    primary_key = Primary,
                    mode = mode,
                    data = dtCratedData,
                });
            }
            if (dtCratedData.Rows.Count <= 0 && dtUpdatedData.Rows.Count <= 0)
            {
                dtReadData = GetTableData(string.Format("SELECT * FROM " + tableName));
                int co =dtReadData.Rows.Count;
                mode = "read";
                ReadItem.Add(new ItemMasterInfo
                {
                    table = tableName,
                    primary_key = Primary,
                    mode = mode,
                    data = dtReadData,
                });
            }
            RetrieveLatestDates();
        }
        
      #endregion
    }
}





