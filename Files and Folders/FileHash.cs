using Ayehu.Sdk.ActivityCreation.Interfaces;
using Ayehu.Sdk.ActivityCreation.Extension;
using System.Text;
using System;
using System.Xml;
using System.Collections.Generic;
using System.IO;
using System.Data;
using System.Management;
using System.Security.Cryptography;

namespace Ayehu.Sdk.ActivityCreation
{
    public class ActivityClass : IActivity
    {
        public string HostName;
        public string UserName;
        public string Password;
        public string Path;
        public string HashAlgorithm;

        public ICustomActivityResult Execute()

        {

            StringWriter sw = new StringWriter();
            DataTable dt = new DataTable("resultSet");
            dt.Columns.Add("Result", typeof(string));

            if (!File.Exists(Path))
            {
                throw new Exception("File not found");
            }

            using (FileStream reader = new System.IO.FileStream(Path, FileMode.Open, FileAccess.Read))
            {
                byte[] hash = null;

                switch (HashAlgorithm)
                {
                    case "0":

                        using (MD5 csp = new MD5CryptoServiceProvider())
                        {
                            hash = csp.ComputeHash(reader);
                        }
                        break;
                    case "1":
                        using (SHA1 csp = new SHA1CryptoServiceProvider())
                        {
                            hash = csp.ComputeHash(reader);
                        }

                        break;
                    case "2":
                        using (SHA256 csp = new SHA256CryptoServiceProvider())
                        {
                            hash = csp.ComputeHash(reader);
                        }
                        break;
                    case "3":
                        using (SHA384 csp = new SHA384CryptoServiceProvider())
                        {
                            hash = csp.ComputeHash(reader);
                        }
                        break;
                    case "4":
                        using (SHA512 csp = new SHA512CryptoServiceProvider())
                        {
                            hash = csp.ComputeHash(reader);
                        }
                        break;
                    case "5":
                        using (RIPEMD160 csp = new RIPEMD160Managed())
                        {
                            hash = csp.ComputeHash(reader);
                        }
                        break;
                    default:
                        throw new Exception("Invalid hashing algorithm.");

                }

                dt.Rows.Add(ByteArrayToString(hash));
            }

            return this.GenerateActivityResult(dt);
        }

        private string ByteArrayToString(byte[] arrInput)
        {
            StringBuilder sb = new StringBuilder();

            foreach (byte b in arrInput)
            {
                sb.Append(b.ToString("X2"));
            }

            return sb.ToString().ToLower();
        }
    }

}






