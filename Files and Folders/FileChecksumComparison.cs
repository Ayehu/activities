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
        public string SrcHostName;
        public string SrcUserName;
        public string SrcPassword;
        public string SrcPath;
        public string DstHostName;
        public string DstUserName;
        public string DstPassword;
        public string DstPath;
        public string HashAlgorithm;

        public ICustomActivityResult Execute()

        {

            StringWriter sw = new StringWriter();
            DataTable dt = new DataTable("resultSet");
            dt.Columns.Add("Result", typeof(string));

            if (!File.Exists(SrcPath) || !File.Exists(DstPath))
            {
                throw new Exception("File not found");
            }

            using (FileStream SrcStream = new System.IO.FileStream(SrcPath, FileMode.Open, FileAccess.Read))
            {
                using (FileStream DstStream = new System.IO.FileStream(DstPath, FileMode.Open, FileAccess.Read))
                {
                    byte[] Srcsum = null;
                    byte[] Dstsum = null;

                    switch (HashAlgorithm)
                    {
                        case "0":

                            using (MD5 csp = new MD5CryptoServiceProvider())
                            {
                                Srcsum = csp.ComputeHash(SrcStream);
                                Dstsum = csp.ComputeHash(DstStream);
                            }
                            break;
                        case "1":
                            using (SHA1 csp = new SHA1CryptoServiceProvider())
                            {
                                Srcsum = csp.ComputeHash(SrcStream);
                                Dstsum = csp.ComputeHash(DstStream);
                            }

                            break;
                        case "2":
                            using (SHA256 csp = new SHA256CryptoServiceProvider())
                            {
                                Srcsum = csp.ComputeHash(SrcStream);
                                Dstsum = csp.ComputeHash(DstStream);
                            }
                            break;
                        case "3":
                            using (SHA384 csp = new SHA384CryptoServiceProvider())
                            {
                                Srcsum = csp.ComputeHash(SrcStream);
                                Dstsum = csp.ComputeHash(DstStream);
                            }
                            break;
                        case "4":
                            using (SHA512 csp = new SHA512CryptoServiceProvider())
                            {
                                Srcsum = csp.ComputeHash(SrcStream);
                                Dstsum = csp.ComputeHash(DstStream);
                            }
                            break;
                        case "5":
                            using (RIPEMD160 csp = new RIPEMD160Managed())
                            {
                                Srcsum = csp.ComputeHash(SrcStream);
                                Dstsum = csp.ComputeHash(DstStream);
                            }
                            break;
                        default:
                            throw new Exception("Invalid hashing algorithm.");

                    }

                    bool isEqual = true;
                    if (Srcsum.Length != Dstsum.Length)
                    {
                        isEqual = false;
                    }
                    else
                    {
                        int i = 0;
                        while (i < Srcsum.Length && isEqual == true)
                        {
                            if (!Srcsum[i].Equals(Dstsum[i]))
                            {
                                isEqual = false;
                            }
                            i++;
                        }
                    }
                    if (isEqual)
                    {
                        dt.Rows.Add("True");
                    }
                    else
                    {
                        dt.Rows.Add("False");
                    }

                }
            }

            return this.GenerateActivityResult(dt);
        }

    }

}

