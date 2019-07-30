using Ayehu.Sdk.ActivityCreation.Interfaces;
using Ayehu.Sdk.ActivityCreation.Extension;
using System.Text;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Data;
using System.Diagnostics;

namespace Ayehu.Sdk.ActivityCreation
{
    public class ActivityClass : IActivity
    {
        public string HostName;
        public string UserName;
        public string Password;
        public string VMName;
        public string VMAnnotation;
        public string Datastore;
        public string DiskSize;
        public string MemorySize;
        public string NumberOfCpus;
        public string GuestOSId;
        public string MemorySizeDim;
        public string DiskSizeDim;

        public ICustomActivityResult Execute()

        {
            StringWriter sw = new StringWriter();
            DataTable dt = new DataTable("resultSet");
            dt.Columns.Add("Result", typeof(String));
            string sResult = "";


            string command_path = "VMWare.exe";
            DataTable dtParams = new DataTable("Params");
            dtParams.Columns.Add("Command");
            dtParams.Columns.Add("UserName");
            dtParams.Columns.Add("Password");
            dtParams.Columns.Add("HostName");
            dtParams.Columns.Add("VMName");
            dtParams.Columns.Add("VMAnnotation");
            dtParams.Columns.Add("DatastoreName");
            dtParams.Columns.Add("DiskSize");
            dtParams.Columns.Add("GuestId");
            dtParams.Columns.Add("CpuCount");
            dtParams.Columns.Add("MemorySize");

            DataRow rParams = dtParams.NewRow();
            rParams["Command"] = "VMCreate";
            rParams["UserName"] = UserName;
            rParams["Password"] = Password;
            rParams["HostName"] = HostName;
            rParams["VMName"] = VMName;
            rParams["VMAnnotation"] = VMAnnotation;
            rParams["DatastoreName"] = Datastore;
            rParams["DiskSize"] = ConvertToMegabytes(DiskSize, DiskSizeDim);
            rParams["GuestId"] = GuestOSId;
            rParams["MemorySize"] = ConvertToMegabytes(MemorySize, MemorySizeDim);
            rParams["CpuCount"] = NumberOfCpus;
            dtParams.Rows.Add(rParams);

            dtParams.WriteXml(sw, XmlWriteMode.WriteSchema, false);

            Process prVMWare = new Process();
            prVMWare.StartInfo.FileName = command_path;
            prVMWare.StartInfo.Arguments = "\"" + sw.ToString().Replace("\"", "\\\"") + "\"";
            prVMWare.StartInfo.UseShellExecute = false;
            prVMWare.StartInfo.CreateNoWindow = true;
            prVMWare.StartInfo.RedirectStandardError = true;
            prVMWare.StartInfo.RedirectStandardInput = true;
            prVMWare.StartInfo.RedirectStandardOutput = true;

            prVMWare.Start();
            StreamReader srResult = prVMWare.StandardOutput;
            sResult = srResult.ReadToEnd();

            srResult.Close();
            prVMWare.Close();



            if (sResult == "")
            {
                return this.GenerateActivityResult(dt);
            }
            else
            {
                return this.GenerateActivityResult(sResult);
            }
        }

        public string ConvertToMegabytes(string byteNumber, string dim)
        {
            string result = string.Empty;
            switch (dim)
            {
                case "MB":
                    result = byteNumber;
                    break;
                case "GB":
                    result = (Convert.ToDouble(byteNumber) * 1024.0).ToString();
                    break;
                case "TB":
                    result = (Convert.ToDouble(byteNumber) * (1024.0 * 1024.0)).ToString();
                    break;
                default:
                    break;
            }
            return result;
        }
    }
}
