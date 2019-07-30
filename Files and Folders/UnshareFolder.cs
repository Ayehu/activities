using Ayehu.Sdk.ActivityCreation.Interfaces;
using Ayehu.Sdk.ActivityCreation.Extension;
using System.Text;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Data;
using System.Security.AccessControl;
using System.Management;
using System.Security.Principal;

namespace Ayehu.Sdk.ActivityCreation
{
    public class ActivityClass : IActivity
    {
        public enum DeviceType { DISK_DRIVE = 0x0, PRINT_QUEUE = 0x1, DEVICE = 0x2, IPC = 0x3/*,DISK_DRIVE_ADMIN = 0x80000000,PRINT_QUEUE_ADMIN = 0x80000001,DEVICE_ADMIN = 0x80000002,IPC_ADMIN = 0x8000003*/};
        public string HostName;
        public string UserName;
        public string Password;
        public string ShareName;

        public ICustomActivityResult Execute()

        {
            StringWriter sw = new StringWriter();
            DataTable dt = new DataTable("resultSet");
            dt.Columns.Add("Result", typeof(String));
            string sResult = "";



            ConnectionOptions connectionOptions = new ConnectionOptions();
            if (!String.IsNullOrEmpty(UserName))
            {
                if (UserName.Contains("\\"))
                {
                    connectionOptions.Authority = "NTLMDOMAIN:" + UserName.Split('\\')[0];
                    connectionOptions.Username = UserName.Split('\\')[1];
                }
                else
                {
                    connectionOptions.Username = UserName;
                }
            }
            if (!String.IsNullOrEmpty(Password))
            { connectionOptions.Password = Password; }
            connectionOptions.Authentication = AuthenticationLevel.PacketPrivacy;
            ManagementScope oMs;
            if (HostName.ToLower() == "localhost" || HostName == "127.0.0.1")
            {
                oMs = new ManagementScope(@"\\.\root\cimv2:Win32_Share.Name=" + "\"" + ShareName + "\"", connectionOptions);
            }
            else
            {
                oMs = new ManagementScope("\\\\" + HostName + @"\root\cimv2:Win32_Share.Name=" + "\"" + ShareName + "\"", connectionOptions);
            }

            oMs.Options.EnablePrivileges = true;
            oMs.Connect();

            // Create a ManagementClass object
            ManagementClass managementClass = new ManagementClass(oMs, new ManagementPath("Win32_Share"), null);
            if (managementClass == null) { throw new Exception("Failed to initialize managment class"); }

            object outParam = null;
            // Invoke the method on the ManagementClass object
            ManagementObjectCollection col = managementClass.GetInstances();
            foreach (ManagementObject o in col)
            {
                if (o.Path.RelativePath.ToLower().EndsWith("\"" + ShareName.ToLower() + "\""))
                {
                    outParam = o.InvokeMethod("Delete", null);
                    break;
                }
            }

            if (outParam == null)
            {
                throw new Exception("Share not found: " + ShareName);
            }

            // Check to see if the method invocation was successful
            if (((uint)outParam) != 0)
            {
                throw new Exception("Unable to delete share. Error code:" + ((uint)outParam).ToString());
            }

            DataRow r = dt.NewRow();
            r["Result"] = "Success";
            dt.Rows.Add(r);


            return this.GenerateActivityResult(dt);
        }
    }
}
