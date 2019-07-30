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
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Ayehu.Sdk.ActivityCreation
{
    public class ActivityClass : IActivity
    {
        public enum DeviceType { DISK_DRIVE = 0x0, PRINT_QUEUE = 0x1, DEVICE = 0x2, IPC = 0x3/*,DISK_DRIVE_ADMIN = 0x80000000,PRINT_QUEUE_ADMIN = 0x80000001,DEVICE_ADMIN = 0x80000002,IPC_ADMIN = 0x8000003*/};
        public string HostName;
        public string UserName;
        public string Password;
        public string ShareName;
        public string SharePath;
        public string ShareDescription;
        public string ShareAccessRights;

        public ICustomActivityResult Execute()

        {
            StringWriter sw = new StringWriter();
            DataTable dt = new DataTable("resultSet");
            dt.Columns.Add("Result", typeof(String));
            string sResult = "";

            try
            {

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
                    oMs = new ManagementScope(@"\\.\root\cimv2", connectionOptions);
                }
                else
                {
                    oMs = new ManagementScope(@"\\" + HostName + @"\root\cimv2", connectionOptions);
                }
                oMs.Options.EnablePrivileges = true;
                oMs.Connect();

                // Create a ManagementClass object
                ManagementClass managementClass = new ManagementClass(oMs, new ManagementPath("Win32_Share"), null);
                if (managementClass == null) { throw new Exception("Failed to initialize managment class"); }

                // Create ManagementBaseObjects for in and out parameters
                ManagementBaseObject inParams = managementClass.GetMethodParameters("Create");

                ManagementBaseObject outParams;

                // Set the input parameters
                inParams["Description"] = ShareDescription;
                inParams["Name"] = ShareName;
                inParams["Path"] = SharePath;
                inParams["Type"] = DeviceType.DISK_DRIVE;
                //inParams["MaximumAllowed"] = int maxConnectionsNum;

                // Invoke the method on the ManagementClass object
                outParams = managementClass.InvokeMethod("Create", inParams, null);

                // Check to see if the method invocation was successful
                if (((uint)outParams.Properties["ReturnValue"].Value) != 0)
                {
                    throw new Exception("Unable to share directory. Error code:" + ((uint)outParams.Properties["ReturnValue"].Value).ToString());
                }

                try
                {
                    DirectoryInfo dInfo = null;
                    if (HostName.ToLower() == "localhost" || HostName == "127.0.0.1")
                    {
                        dInfo = new DirectoryInfo(SharePath);
                    }
                    else
                    {
                        dInfo = new DirectoryInfo(@"\\" + HostName + @"\" + SharePath.Replace(":", "$"));
                    }

                    if (!String.IsNullOrEmpty(ShareAccessRights))
                    {
                        DirectorySecurity dSecurity = dInfo.GetAccessControl();

                        //if (!String.IsNullOrEmpty(ShareAccessRights))
                        //{
                        string[] rights = ShareAccessRights.Split(";,".ToCharArray());
                        foreach (String s in rights)
                        {
                            dSecurity.AddAccessRule(new FileSystemAccessRule(s, FileSystemRights.FullControl, InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit, PropagationFlags.InheritOnly, AccessControlType.Allow));
                        }
                        //}
                        //else
                        //{
                        //	dSecurity.AddAccessRule(new FileSystemAccessRule("everyone", FileSystemRights.FullControl, InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit, PropagationFlags.InheritOnly, AccessControlType.Allow));
                        //}
                        dInfo.SetAccessControl(dSecurity);
                    }

                }
                catch (Exception ex)
                {
                    if (((uint)outParams.Properties["ReturnValue"].Value) == 0)
                    {
                        object outParam = null;
                        // Invoke the method on the ManagementClass object
                        ManagementObjectCollection col = managementClass.GetInstances();
                        foreach (ManagementObject o in col)
                        {
                            if (o.Path.RelativePath.EndsWith("\"" + ShareName + "\""))
                            {
                                outParam = o.InvokeMethod("Delete", null);
                                break;
                            }
                        }
                    }
                    throw;// new Exception("Failed to set permissions");
                }

                DataRow r = dt.NewRow();
                r["Result"] = "Success";
                dt.Rows.Add(r);
            }
            catch (Exception ex)
            {
                throw;
            }

            return this.GenerateActivityResult(dt);
        }
    }
}
