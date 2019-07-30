using Ayehu.Sdk.ActivityCreation.Interfaces;
using Ayehu.Sdk.ActivityCreation.Extension;
using System.Text;
using System;
using System.Data;
using System.IO;
using System.Management;
using ActivitiesUtilsLib;

namespace Ayehu.Sdk.ActivityCreation
{
    public class ActivityClass : IActivity
    {
        public string HostName;
        public string UserName;
        public string Password;
        public string Path;
        public int ContentsOnly;

        public ICustomActivityResult Execute()

        {
            StringWriter sw = new StringWriter();
            DataTable dt = new DataTable("resultSet");
            dt.Columns.Add("Result", typeof(string));

            ConnectionOptions connectionOptions = new ConnectionOptions();
            connectionOptions.Username = UserName;
            connectionOptions.Password = Password;
            connectionOptions.Authentication = AuthenticationLevel.PacketPrivacy;
            connectionOptions.Impersonation = ImpersonationLevel.Impersonate;
            connectionOptions.EnablePrivileges = true;
            ManagementScope oms;

            if (ActivitiesUtils.IsLocalhost(HostName))
            {
                oms = new ManagementScope(@"\\.\root\cimv2");
            }
            else
            {
                oms = new ManagementScope(string.Format(@"\\{0}\root\cimv2", HostName), connectionOptions);
            }

            Path = Path.Trim();
            if (Path.EndsWith(@"\"))
            {
                Path = Path.Substring(0, Path.LastIndexOf(@"\"));
            }

            string PathRoot = System.IO.Path.GetDirectoryName(Path);
            if (string.IsNullOrEmpty(PathRoot))
            {
                // Don't delete Root ???????????????
                ContentsOnly = 1;
            }

            if (Path.StartsWith(@"\\"))
            {
                DirectoryInfo RemotePath = new DirectoryInfo(Path);
                ClearDirContent(RemotePath);
                if (ContentsOnly == 0)
                {
                    RemotePath.Delete(true);
                }
            }
            else
            {
                string PathBeforeConvertToQuery = Path;
                Path = Path.GetQueryPath();
                ObjectQuery oQueryDirCheck = new System.Management.ObjectQuery("SELECT Name FROM Win32_Directory WHERE Name = '" + Path + "'");
                ManagementObjectSearcher oSearcherDirCheck = new ManagementObjectSearcher(oms, oQueryDirCheck);

                var allObjDirCheck = oSearcherDirCheck.Get();
                bool isFound = false;
                foreach (ManagementObject ob in allObjDirCheck)
                {
                    isFound = true;
                }

                if (!isFound)
                {
                    throw new Exception("Folder not found");
                }

                if (ContentsOnly == 0)
                {
                    // DELETE THE CURRENT FOLDER
                    ObjectQuery oQueryDir = new System.Management.ObjectQuery("SELECT Name FROM Win32_Directory WHERE Name = '" + Path + "'");
                    ManagementObjectSearcher oSearcherDir = new ManagementObjectSearcher(oms, oQueryDir);
                    var allObjDir = oSearcherDir.Get();
                    foreach (ManagementObject ob in allObjDir)
                    {
                        var result = ob.InvokeMethod("Delete", null);
                        uint actualResult = 0;
                        if (uint.TryParse(result.ToString(), out actualResult) == true)
                        {
                            if (actualResult != 0)
                            {
                                throw new ApplicationException("Could not delete folder: " + Path + ". Please check folder contents and try again.");
                            }
                            else
                            {
                                // deleted
                            }
                        }
                        else
                        {
                            // Could not parse to result ?
                            throw new ApplicationException("Could not parse function result");
                        }
                    }
                }
                else
                {
                    // DELETE ALL FILES

                    string Volume = System.IO.Path.GetPathRoot(PathBeforeConvertToQuery);
                    string PathOnlyNoVolume = Path.Replace(Volume, "");
                    PathOnlyNoVolume.GetQueryPath();

                    ObjectQuery oQuery1 = null;
                    if (string.IsNullOrEmpty(PathOnlyNoVolume))
                    {
                        oQuery1 = new System.Management.ObjectQuery("SELECT Name FROM CIM_DataFile WHERE Drive = '" + Volume.Replace(@"\", "") + @"' and path = '\\'");
                    }
                    else
                    {
                        oQuery1 = new System.Management.ObjectQuery("SELECT Name FROM CIM_DataFile WHERE Drive = '" + Volume.Replace(@"\", "") + @"' and path = '\\" + PathOnlyNoVolume + @"\\'");
                    }

                    //ObjectQuery oQuery1 = new System.Management.ObjectQuery("SELECT Name FROM CIM_DataFile WHERE Drive = '" + Volume.Replace(@"\", "") + @"' and path = '\\" + PathOnlyNoVolume + @"\\'");
                    ManagementObjectSearcher oSearcher1 = new ManagementObjectSearcher(oms, oQuery1);
                    var allOb1 = oSearcher1.Get();
                    foreach (ManagementObject ob in allOb1)
                    {
                        try
                        {
                            ob.Delete();
                        }
                        catch
                        { }
                    }

                    // DELETE ALL SUBFolders

                    //ObjectQuery oQuery = new System.Management.ObjectQuery("SELECT Name FROM Win32_Directory WHERE Name = '" + Path.Replace(@"\", @"\\") + "'");
                    ObjectQuery oQuery = new System.Management.ObjectQuery("Associators of {Win32_Directory.Name='" + Path + "'} Where AssocClass = Win32_Subdirectory ResultRole = PartComponent");
                    ManagementObjectSearcher oSearcher = new ManagementObjectSearcher(oms, oQuery);

                    var allObj = oSearcher.Get();
                    foreach (ManagementObject ob in allObj)
                    {
                        var result = ob.InvokeMethod("Delete", null);
                        uint actualResult = 0;
                        if (uint.TryParse(result.ToString(), out actualResult) == true)
                        {
                            if (actualResult != 0)
                            {
                                throw new ApplicationException("Could not delete folder: " + Path + ". Please check folder contents and try again.");
                            }
                            else
                            {
                                // deleted
                            }
                        }
                        else
                        {
                            // Could not parse to result ?
                            throw new ApplicationException("Could not parse function result");
                        }
                    }
                }

            }
            dt.Rows.Add("Success");
            return this.GenerateActivityResult(dt);
        }

        public void ClearDirContent(DirectoryInfo dir)
        {
            foreach (FileInfo fi in dir.GetFiles())
            {
                fi.IsReadOnly = false;
                try
                {
                    fi.Delete();
                }
                catch
                { }
            }
            foreach (DirectoryInfo di in dir.GetDirectories())
            {
                ClearDirContent(di);
                try
                {
                    di.Delete(true);
                }
                catch
                { }
            }
        }
    }
}
