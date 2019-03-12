using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace TeebTablesSyncing
{
   
    public class Encode
    {
        public string connectionstring, KeyEncoded, InputText, errorstring;
        //public struct Section
        //{
        //    public string custid;
        //    public string seqno;
        //    public string prd;
        //    public string edn;
        //    public string version;

        //}
        //public clsWeb(xmlSettings xmlsetting)
        //{
        //    //settings = xmlsetting;
        //    //KeyEncoded = begin();
        //   // DecryptedValue = Decode();         //Testing
        //}

        public Encode()
        {
        }

       
        public  Boolean DecryptedValue,Verified_Customer;
        public  string Server, password, serial, HexKeyArrLen, HexValArrLen;
      
        public  string code, codeString, ArrayString = null, MergeString, KeyValueMerged, Code, response, mergedstring, custid;
        
        public string Encodedkey, Encodedvalue, KeyArrayString, ValueArrayString;
        public  int keyArrayLength, valueArrayLength, MergedLength, intNum, i = 2, index = 0, pos = 0,pos1=0;

        public string[] keyArray;
        public string[] valueArray;
        public string[] CodeArray;
        public string[] keyName;
        public  string[] codeArray;
        public  string[] valueArray1 = new string[500];
        public  string[] codeStringArray = new string[500];
        public  int randomNumber;
        Random r = new Random();
        GetData objGetData = new GetData();
        
        public string Encoding(string product_id)
        {
            
            
            randomNumber = r.Next(2, 5);          
           
            try
            {
                    keyArray = new string[] { "product_id" };
                    valueArray = new string[] { product_id };
               
                for (int i = 0; i < valueArray.Length; i++)
                {
                    if (string.IsNullOrEmpty(valueArray[i]))
                    {
                        valueArray[i] = "0000";
                    }
                }

                KeyArrayString = AppendArrayToString(keyArray);
                ValueArrayString = AppendArrayToString(valueArray);
                Encodedkey = objGetData.Base64Encode(KeyArrayString);
                Encodedvalue = objGetData.Base64Encode(ValueArrayString);               
                keyArray = splitByLength(Encodedkey, randomNumber);
                valueArray = splitByLength(Encodedvalue, randomNumber);
                keyArrayLength = keyArray.Length;
                valueArrayLength = valueArray.Length;
                KeyValueMerged = MergeKeyValueArray(keyArray, valueArray);
                MergedLength = KeyValueMerged.Length;
                intNum = MergedLength / 3;
                if (intNum > 15)
                {
                    intNum = 11;
                }

                string HexintNum = intNum.ToString("x");                             // Converting into hexadecimal
                string[] stringArray = splitByIntNum(KeyValueMerged, intNum);
                HexKeyArrLen =convertKeyToHex(keyArrayLength);
                HexValArrLen = convertValueToHex(valueArrayLength);
                CodeArray = generateCodeArray(intNum, HexintNum, stringArray, HexKeyArrLen, HexValArrLen);
                int len = CodeArray.Length;
                string Code = generateCode(CodeArray);

            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
                string error = ex.Message;
                return "";
            }
            return code;
        }
  

        public  string getValue(string[] arr1, string[] arr2, string strn)
        {
            int index = 0;
            index = Array.IndexOf(arr1,strn);
            string cust = arr2[index];

            return cust;
        }
        public  string AppendArrayToString(string[] Array)
        {
            ArrayString = null;
            for (int i = 0; i < Array.Length; i++)
            {
                ArrayString += Array[i];
                if (i < Array.Length - 1)
                    ArrayString += "|";
            }
            return ArrayString;
        }
        public  string MergeKeyValueArray(string[] keyArray, string[] valueArray)
        {
            int i = 0, j = 0, n;
            int maxIndex = keyArray.Length;

            while (i == j)
            {
                if (i != maxIndex)
                {
                    MergeString += keyArray[i] + valueArray[j];
                    i++; j++;
                }
                if (i == maxIndex)
                    break;
            }
            n = valueArray.Length - keyArray.Length;

            Array.Copy(valueArray, i, valueArray1, 0, n);                              //Copy values from one array to another array
            valueArray1 = valueArray1.Where(x => !string.IsNullOrEmpty(x)).ToArray();
            for (i = 0; i <= valueArray1.Length - 1; i++)
                MergeString += valueArray1[i];
            return MergeString;
        }
        public  string[] splitByLength(string str, int chunksize)
        {
            string[] substr = new string[500];
            int len = str.Length;
            while (!String.IsNullOrEmpty(str))
            {
                for (int j = 0; j <= len; j++)
                {
                    if (str.Length < chunksize)
                    {
                        for (int i = str.Length + 1; i <= chunksize; i++)
                        {
                            str += "$";
                        }
                    }
                    substr[j] = str.Substring(0, chunksize);
                    str = str.Remove(0, chunksize);
                    if (str == "")
                    {
                        substr = substr.Where(x => !string.IsNullOrEmpty(x)).ToArray();   //Remove the blank values in the array
                        return substr;
                    }
                }
            }
            return substr;
        }
        public  string[] splitByIntNum(string str, int chunksize)
        {
            string[] substr = new string[500];
            int len = str.Length;
            while (!String.IsNullOrEmpty(str))
            {
                for (int j = 0; j <= len; j++)
                {

                    if (str.Length < chunksize)
                    {
                        substr[j] = str;
                        str = str.Remove(0, str.Length);
                    }
                    else
                    {
                        substr[j] = str.Substring(0, chunksize);
                        str = str.Remove(0, chunksize);
                    }

                    if (str == "")
                    {
                        substr = substr.Where(x => !string.IsNullOrEmpty(x)).ToArray();   //Remove the blank values in the array
                        return substr;
                    }
                }
            }
            return substr;
        }
       
        public  string convertKeyToHex(int keyArrayLength)
        {
            string HexKeyArrayLength = keyArrayLength.ToString("x");                  // Converting into hexadecimal
            return HexKeyArrayLength;
        }
        public  string convertValueToHex(int valueArrayLength)
        {
            string HexvalueArrayLength = valueArrayLength.ToString("x");              // Converting into hexadecimal
            return HexvalueArrayLength;
        }
        public  string[] generateCodeArray(int intNum, string HexintNum, string[] stringArray, string HexKeyArrayLength, string HexvalueArrayLength)
        {
            Array.Copy(stringArray, 2, codeStringArray, 0, stringArray.Length - 2);    //Copy values from one array to another array
            for (int i = 0; i < codeStringArray.Length; i++)
                codeString += codeStringArray[i];
            string[] codeArray = { HexintNum, stringArray[0], randomNumber.ToString(), stringArray[1], HexKeyArrayLength, "g", codeString, "h", HexvalueArrayLength };

            return codeArray;
        }
        public  string generateCode(string[] codeArray)
        {
            code = "";
            for (int i = 0; i <= codeArray.Length - 1; i++)
                code += codeArray[i];
            return code;
        } 

   
    }
}
