using Ayehu.Sdk.ActivityCreation.Interfaces;
using Ayehu.Sdk.ActivityCreation.Extension;
using System.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Data;
using System.Management;

namespace Ayehu.Sdk.ActivityCreation
{
    public class ActivityClass : IActivity
    {
        public string HostName;
        public string UserName;
        public string Password;
        public string Path;

        public ICustomActivityResult Execute()

        {
            StringWriter sw = new StringWriter();
            DataTable dt = new DataTable("resultSet");
            dt.Columns.Add("Result", typeof(string));

            if (string.IsNullOrEmpty(Path))
            {
                throw new Exception("Folder not found");
            }

            ConnectionOptions connectionOptions = new ConnectionOptions();
            connectionOptions.Username = UserName;
            connectionOptions.Password = Password;
            connectionOptions.Authentication = AuthenticationLevel.PacketPrivacy;
            connectionOptions.Impersonation = ImpersonationLevel.Impersonate;
            connectionOptions.EnablePrivileges = true;
            ManagementScope oms;

            if (HostName.ToLower() == "localhost" || HostName.ToLower() == "127.0.0.1")
            {
                oms = new ManagementScope(@"\\.\root\cimv2");
            }
            else
            {
                oms = new ManagementScope(string.Format(@"\\{0}\root\cimv2", HostName), connectionOptions);
            }

            Path = Path.Trim();
            string dirName = System.IO.Path.GetDirectoryName(Path);
            if (Path.EndsWith(@"\") && !string.IsNullOrEmpty(dirName))
            {
                Path = Path.Substring(0, Path.LastIndexOf(@"\"));
            }

            if (Path.StartsWith(@"\\"))
            {
                if (!Directory.Exists(Path))
                {
                    throw new Exception("Folder not found");
                }
                DirectoryInfo RemotePath = new DirectoryInfo(Path);
                FileInfo[] Files = RemotePath.GetFiles();
                dt.Rows.Add(Files.Length.ToString());
            }
            else
            {
                string Volume = System.IO.Path.GetPathRoot(Path);
                string PathOnlyNoVolume = string.Empty;
                if (!string.IsNullOrEmpty(Volume))
                {
                    PathOnlyNoVolume = Path.Replace(Volume, "");
                }
                PathOnlyNoVolume = PathOnlyNoVolume.Replace(@"\", @"\\");

                ObjectQuery oQueryDir = new System.Management.ObjectQuery("SELECT Name FROM Win32_Directory WHERE Name = '" + Path.Replace(@"\", @"\\") + "'");
                ManagementObjectSearcher oSearcherDir = new ManagementObjectSearcher(oms, oQueryDir);

                var allObjDir = oSearcherDir.Get();
                bool isFound = false;
                foreach (ManagementObject ob in allObjDir)
                {
                    isFound = true;
                }

                if (!isFound)
                {
                    throw new Exception("Folder not found");
                }

                ObjectQuery oQuery = null;
                if (string.IsNullOrEmpty(PathOnlyNoVolume))
                {
                    oQuery = new System.Management.ObjectQuery("SELECT Name FROM CIM_DataFile WHERE Drive = '" + Volume.Replace(@"\", "") + @"' and path = '\\'");
                }
                else
                {
                    oQuery = new System.Management.ObjectQuery("SELECT Name FROM CIM_DataFile WHERE Drive = '" + Volume.Replace(@"\", "") + @"' and path = '\\" + PathOnlyNoVolume + @"\\'");
                }
                ManagementObjectSearcher oSearcher = new ManagementObjectSearcher(oms, oQuery);

                var allObj = oSearcher.Get();
                dt.Rows.Add(allObj.Count.ToString());
            }
            return this.GenerateActivityResult(dt);
        }

    }

}
