using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Security.Cryptography;
using System.IO;
using System.Windows.Forms;

namespace TeebTablesSyncing
{
    class GetData
    {
        public string Id;
        public string GetProductId(MySqlConnection MySqlCon, string Category, string Password, Boolean Decrypt = true)
        {
            MySqlCommand MySqlCmd = new MySqlCommand();
            MySqlCmd.Connection = MySqlCon;
            try
            {
                
                MySqlCmd.CommandText = "SELECT SERIAL FROM KEYINFO WHERE CATEGORY= '" + Category + "'";
                if(Decrypt)
                {
                    string ProductId = (string)MySqlCmd.ExecuteScalar();//
                    Id = DecryptString((string)MySqlCmd.ExecuteScalar(), Password);
                }
                
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            finally
            {
                MySqlCmd.Dispose();
            }
            return Id;
            
        }
       
        public  string DecryptString(string InputText, string Password)
            //dec data from table

        {
            // streamReader.BaseStream.ReadTimeout = 2000;
            try
            {
                InputText.Length.ToString();
                RijndaelManaged RijndaelCipher = new RijndaelManaged();
                //RijndaelCipher.Padding = PaddingMode.Zeros;
                byte[] EncryptedData = Convert.FromBase64String(InputText);
                byte[] Salt = Encoding.ASCII.GetBytes(Password.Length.ToString());
                //Making of the key for decryption
                PasswordDeriveBytes SecretKey = new PasswordDeriveBytes(Password, Salt);
                //Creates a symmetric Rijndael decryptor object.
                ICryptoTransform Decryptor = RijndaelCipher.CreateDecryptor(SecretKey.GetBytes(32), SecretKey.GetBytes(16));
                MemoryStream memoryStream = new MemoryStream(EncryptedData);
                //Defines the cryptographics stream for decryption.THe stream contains decrpted data
                CryptoStream cryptoStream = new CryptoStream(memoryStream, Decryptor, CryptoStreamMode.Read);
               // cryptoStream.FlushFinalBlock();
               // cryptoStream.Write(EncryptedData, 0, EncryptedData.Length);
                byte[] PlainText = new byte[EncryptedData.Length];
                int DecryptedCount = cryptoStream.Read(PlainText, 0, PlainText.Length);
                memoryStream.Close();
                cryptoStream.Close();
                //Converting to string
                string DecryptedData = Encoding.Unicode.GetString(PlainText, 0, DecryptedCount);
                return DecryptedData;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                string error = ex.Message;
                return "";
            }
           // return SerialNo;
        }

        public  string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }
        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }
    }
}
