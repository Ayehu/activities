using Ayehu.Sdk.ActivityCreation.Interfaces;
using Ayehu.Sdk.ActivityCreation.Extension;
using System.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Data;
using System.Management;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Collections;
using System.Net;
using System.DirectoryServices.AccountManagement;

namespace Ayehu.Sdk.ActivityCreation
{
    public class ActivityClass : IActivity
    {
        public string HostName;
        public string UserName;
        public string Password;
        public string Path;
        public string PermissionsData;
        public string InheritablePermissions;

        public ICustomActivityResult Execute()

        {

            StringWriter sw = new StringWriter();
            DataTable dt = new DataTable("resultSet");
            dt.Columns.Add("Result", typeof(string));

            if (string.IsNullOrEmpty(Path))
            {
                throw new Exception("File not found");
            }
            if (string.IsNullOrEmpty(PermissionsData))
            {
                throw new Exception("Permissions data wasn't provided.");
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

            string[] singleUserData = PermissionsData.Split(new String[] { "<value>" }, StringSplitOptions.None);
            string fullUserName = GetUserName(HostName, singleUserData[0], Path);

            if (Path.StartsWith(@"\\")) // || HostName.ToLower() == "localhost" || HostName.ToLower() == "127.0.0.1")
            {
                // ---------- SET PERMISSIONS ------------------

                if (!File.Exists(Path))
                {
                    throw new Exception("File not found");
                }

                //get file info
                FileInfo fi = new FileInfo(Path);

                //get security access
                FileSecurity fs = fi.GetAccessControl();

                SecurityIdentifier si = null;
                // Check is User/Group exist
                try
                {
                    si = GetSecurityIdentifier(HostName, fullUserName, Path);
                    AddAccessRule(fs, si, singleUserData);
                }
                catch
                {
                    throw new Exception(@"User / Group does not exist or you are not authorized to change the security settings.");
                }

                if (bool.Parse(InheritablePermissions))
                {
                    //remove any inherited access
                    fs.SetAccessRuleProtection(true, false);
                }


                //get any special user access
                AuthorizationRuleCollection rules = fs.GetAccessRules(true, true, typeof(System.Security.Principal.NTAccount));

                //remove any special access
                foreach (FileSystemAccessRule rule in rules)
                {

                    if (bool.Parse(InheritablePermissions))
                    {
                        fs.RemoveAccessRule(rule);
                    }
                    else
                    {
                        string ruleUserName = rule.IdentityReference.Value;
                        if (string.Equals(ruleUserName.ToLower(), fullUserName))
                        {
                            fs.RemoveAccessRule(rule);
                        }
                    }
                }

                AddAccessRule(fs, si, singleUserData);

                //add all other users delete only permissions.
                //fs.AddAccessRule(new FileSystemAccessRule("Authenticated Users", FileSystemRights.Delete, AccessControlType.Allow));

                //flush security access.
                fi.SetAccessControl(fs);

                dt.Rows.Add("Success");
            }
            else
            {
                // ---------- SET PERMISSIONS WMI ------------------

                int lastInd = Path.LastIndexOf(@"\");
                //string folderPath = Path.Substring(0, Path.Length - lastInd);
                string folderPath = Path.Substring(0, lastInd);

                string Volume = System.IO.Path.GetPathRoot(folderPath);
                string PathOnlyNoVolume = string.Empty;
                if (!string.IsNullOrEmpty(Volume))
                {
                    PathOnlyNoVolume = folderPath.Replace(Volume, "");
                }
                PathOnlyNoVolume = PathOnlyNoVolume.Replace(@"\", @"\\");

                // ----------------- Check File existance -----------------------------------
                ObjectQuery oQuery = null;
                if (string.IsNullOrEmpty(PathOnlyNoVolume))
                {
                    oQuery = new System.Management.ObjectQuery("SELECT Name FROM CIM_DataFile WHERE Drive = '" + Volume.Replace(@"\", "") + @"' and path = '\\'");
                }
                else
                {
                    oQuery = new System.Management.ObjectQuery("SELECT Name FROM CIM_DataFile WHERE Drive = '" + Volume.Replace(@"\", "") + @"' and path = '\\" + PathOnlyNoVolume + @"\\'");
                }

                ManagementObjectSearcher oSearcher1 = new ManagementObjectSearcher(oms, oQuery);
                var allOb1 = oSearcher1.Get();


                bool isFound = false;
                foreach (ManagementObject fobject in allOb1)
                {
                    if (string.Equals(fobject["Name"].ToString().ToLower(), Path.ToLower()))
                    {
                        isFound = true;
                    }
                }
                if (!isFound)
                {
                    throw new Exception("File not found.");
                }
                // --------------------- End Check File Existance ------------------------------------------------------------


                //string[] singleUserData = PermissionsData.Split(new String[] { "<value>" }, StringSplitOptions.None);

                //string UsrName = singleUserData[0];

                // Works when fileName is local directory, but not UNC path.
                ManagementPath mngPath = new ManagementPath();
                mngPath.RelativePath = @"Win32_LogicalFileSecuritySetting.Path=" + "'" + Path + "'";
                //+ "'" + @"C:\Test1\tf1.txt" + "'";

                ManagementObject fileSecurity = new ManagementObject(
                oms, mngPath, null);


                // When used with UNC path, exception with "Not Found" is thrown.
                ManagementBaseObject outParams = null;
                try
                {
                    outParams = (ManagementBaseObject)fileSecurity.InvokeMethod(
                    "GetSecurityDescriptor", null, null);
                }
                catch
                {
                    throw new Exception("Failed to get Security Descriptor.");
                }

                // Get security descriptor and DACL for specified file.
                ManagementBaseObject descriptor =
                (ManagementBaseObject)outParams.Properties["Descriptor"].Value;
                ManagementBaseObject[] dacl =
                (ManagementBaseObject[])descriptor.Properties["Dacl"].Value;

                //string ppp = fileSecurity.Properties["ControlFlags"].Value.ToString();
                //fileSecurity.Properties["ControlFlags"].Value = 4 | 4096 | 8192;//ControlFlags.DiscretionaryAclProtected;


                //string propName = string.Empty;
                //foreach (var prop in fileSecurity.Properties)
                //{
                //    propName += prop.Name + ";";
                //}


                //ControlFlags.SystemAclProtected property 

                //string name = string.Empty;


                //string[] singleUserData = PermissionsData.Split(new String[] { "<value>" }, StringSplitOptions.None);


                //string fullUsrName = GetUserName(singleUserData[0]);
                string machineName = GetMachineName(HostName);
                string[] dividedUserName = fullUserName.Split(new string[] { @"\" }, StringSplitOptions.None);
                string UserDomain = string.Empty;
                string UsrName = string.Empty;
                if (dividedUserName.Length < 2)
                {
                    UsrName = dividedUserName[0].ToLower();
                }
                else
                {
                    UserDomain = dividedUserName[0].ToLower();
                    UsrName = dividedUserName[1].ToLower();
                }

                // Get the user account to be trustee.
                ManagementObject userAccount = new ManagementClass(oms,
                new ManagementPath("Win32_Trustee"), null);
                userAccount.Properties["Name"].Value = UsrName;
                userAccount.Properties["Domain"].Value = UserDomain;

                ManagementObject newAce = CreateNewACE(oms, userAccount, singleUserData);

                // Check is User/Group exist.
                try
                {
                    // Add ACE to DACL and set to descriptor.
                    ArrayList daclArray = new ArrayList(dacl);
                    daclArray.Add(newAce);

                    descriptor.Properties["Dacl"].Value = daclArray.ToArray();

                    // User SetSecurityDescriptor to apply the descriptor.
                    ManagementBaseObject inParamsCheck = fileSecurity.GetMethodParameters("SetSecurityDescriptor");
                    inParamsCheck["Descriptor"] = descriptor;
                    outParams = fileSecurity.InvokeMethod("SetSecurityDescriptor", inParamsCheck, null);

                    uint errorcodeCheck = (uint)outParams["returnValue"];

                    ErrorCheck(errorcodeCheck);
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("Invalid parameter"))
                    {
                        throw new Exception(@"User / Group does not exist or you are not authorized to change the security settings.");
                    }
                    else
                    {
                        throw new Exception(ex.Message);
                    }
                }

                ArrayList NewdaclArray = new ArrayList();

                if (bool.Parse(InheritablePermissions))
                {
                    // Remove inheritable permissions.
                    int descriptorMask = 0x0004 | 0x1000;
                    descriptor.Properties["ControlFlags"].Value = descriptorMask;
                }
                else
                {
                    // leave all inheritable permissions
                    foreach (var ace in dacl)
                    {
                        if (ace.Properties["AccessMask"] != null)
                        {
                            // ACE children inheritance
                            //ace.Properties["AceFlags"].Value = 16 | 1;

                            ManagementBaseObject mob = (ManagementBaseObject)ace.Properties["Trustee"].Value;
                            //name += mob.Properties["Name"].Value.ToString() + ";";
                            string TrusteeName = string.Empty;
                            string TrusteeDomain = string.Empty;

                            if (mob.Properties["Domain"] != null)
                            {
                                if (mob.Properties["Domain"].Value != null)
                                    TrusteeDomain = mob.Properties["Domain"].Value.ToString().ToLower();
                            }

                            if (mob.Properties["Name"] != null)
                            {
                                if (mob.Properties["Name"].Value != null)
                                    TrusteeName = mob.Properties["Name"].Value.ToString().ToLower();
                            }

                            if (!string.Equals(UsrName, TrusteeName))
                            {
                                NewdaclArray.Add(ace);
                            }
                            else
                            {
                                if (string.IsNullOrEmpty(UserDomain))
                                {
                                    if (!string.Equals(machineName, TrusteeDomain))
                                    {
                                        NewdaclArray.Add(ace);
                                    }
                                }
                                else
                                {
                                    if (!string.Equals(UserDomain, TrusteeDomain))
                                    {
                                        NewdaclArray.Add(ace);
                                    }
                                }
                            }
                        }
                    }
                }

                // Add ACE to DACL and set to descriptor.
                NewdaclArray.Add(newAce);
                descriptor.Properties["Dacl"].Value = NewdaclArray.ToArray();

                // User SetSecurityDescriptor to apply the descriptor.
                ManagementBaseObject inParams =
                fileSecurity.GetMethodParameters("SetSecurityDescriptor");
                inParams["Descriptor"] = descriptor;
                outParams = fileSecurity.InvokeMethod("SetSecurityDescriptor", inParams, null);

                uint errorcode = (uint)outParams["returnValue"];

                ErrorCheck(errorcode);

                dt.Rows.Add("Success");
            }
            return this.GenerateActivityResult(dt);
        }
        private void ErrorCheck(uint ReturnValue)
        {
            string ErrorMessage = string.Empty;
            switch (ReturnValue)
            {
                case 0: // Success 
                    break;
                case 2: // Access denied 
                    ErrorMessage = " Do not have access.";
                    break;
                case 8: // Unknown failure 
                    ErrorMessage = " Unknown failed.";
                    break;
                case 9: // Invalid name 
                    ErrorMessage = " Illegal share name.";
                    break;
                case 10: // Invalid level 
                    ErrorMessage = " Illegal level.";
                    break;
                case 21: // Invalid parameter 
                    ErrorMessage = " Illegal parameter.";
                    break;
                case 22: // Duplicate share 
                    ErrorMessage = " Repeated Share.";
                    break;
                case 23: // Redirected path 
                    ErrorMessage = " Redirect path.";
                    break;
                case 24: // Unknown device or directory 
                    ErrorMessage = " Unknown directory.";
                    break;
                case 25: // Net name not found 
                    ErrorMessage = " The network name does not exist.";
                    break;
                default:
                    ErrorMessage = " Unknown error.";
                    break;
            }
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                throw new Exception("Failed to set permissions." + ErrorMessage);
            }
        }
        public string GetUserName(string HostName, string UserName, string Path)
        {
            string fixedUserName = UserName;
            string fixedHostName = string.Empty;
            bool isUNC = false;

            if (Path.StartsWith(@"\\") && Path.Length > 2)
            {
                isUNC = true;
                int slashIndex = Path.Substring(2).IndexOf(@"\");
                fixedHostName = Path.Substring(2, slashIndex);
            }
            else
            {
                fixedHostName = HostName;
            }

            string MachineName = GetMachineName(fixedHostName);
            if (!string.IsNullOrEmpty(fixedUserName))
            {
                if (UserName.Contains(@"\"))
                {
                    int del = UserName.IndexOf(@"\");
                    string prefixName = UserName.Substring(0, del);
                    if (string.Equals(prefixName.ToLower(), "."))
                    {
                        //string MachineName = GetMachineName(HostName);
                        //fixedUserName = MachineName + UserName.Substring(del);
                        if (isUNC)
                        {
                            fixedUserName = MachineName + UserName.Substring(del);
                        }
                        else
                        {
                            fixedUserName = UserName.Substring(del + 1); // check it !!!!!!!!!!!!!!!!!!!!!!!!!!!
                        }
                    }
                    else
                    {
                        if (prefixName.Contains("."))
                        {
                            int dotIndex = prefixName.IndexOf(".");
                            string DomainName = prefixName.Substring(0, dotIndex).ToLower();
                            if (string.Equals(MachineName, DomainName))
                            {
                                fixedUserName = UserName.Substring(del);
                            }
                            else
                            {
                                fixedUserName = DomainName + UserName.Substring(del);
                            }
                        }
                        else
                        {
                            if (string.Equals(MachineName, prefixName))
                            {
                                if (isUNC)
                                {
                                    fixedUserName = prefixName + UserName.Substring(del);
                                }
                                else
                                {
                                    fixedUserName = UserName.Substring(del + 1);
                                }
                            }
                            else
                            {
                                fixedUserName = prefixName + UserName.Substring(del);
                            }
                        }
                    }
                }
                else
                {
                    if (UserName.Contains("@"))
                    {
                        int atIndex = UserName.IndexOf("@");
                        string fullDomainName = UserName.Substring(atIndex + 1);
                        if (fullDomainName.Contains("."))
                        {
                            int dotIndex = fullDomainName.IndexOf(".");
                            string DomainName = fullDomainName.Substring(0, dotIndex).ToLower();
                            if (string.Equals(MachineName, DomainName))
                            {
                                fixedUserName = UserName.Substring(0, atIndex);
                            }
                            else
                            {
                                fixedUserName = DomainName + @"\" + UserName.Substring(0, atIndex);
                            }
                        }
                        else
                        {
                            if (string.Equals(MachineName, fullDomainName))
                            {
                                fixedUserName = UserName.Substring(0, atIndex);
                            }
                            else
                            {
                                fixedUserName = fullDomainName + @"\" + UserName.Substring(0, atIndex);
                            }
                        }
                    }
                    else
                    {
                        //string MachineName = GetMachineName(HostName);
                        //fixedUserName = MachineName + @"\" + UserName;
                        if (isUNC)
                        {
                            fixedUserName = MachineName + @"\" + UserName;
                        }
                        else
                        {
                            fixedUserName = UserName;
                        }
                    }
                }

                fixedUserName = fixedUserName.ToLower();
            }
            return fixedUserName;
        }
        public string GetMachineName(string host)
        {
            string mName = string.Empty;
            try
            {
                IPHostEntry hostEntry = Dns.GetHostEntry(host);
                string machineName = hostEntry.HostName;
                int dotIndexMachineName = machineName.IndexOf(".");
                mName = machineName.Substring(0, dotIndexMachineName);
                return mName.ToLower();
            }
            catch
            {
                throw new Exception(string.Format("Machine {0} not found.", host));
            }
        }

        public void AddAccessRule(FileSecurity fs, SecurityIdentifier si, string[] singleUserData)
        {
            if (bool.Parse(singleUserData[1]))
            {
                // FullControl
                //add current user with full control.
                fs.AddAccessRule(new FileSystemAccessRule(si, FileSystemRights.FullControl, AccessControlType.Allow));
            }
            else
            {
                if (bool.Parse(singleUserData[2]))
                {
                    // Modify
                    //add current user with Modify.
                    fs.AddAccessRule(new FileSystemAccessRule(si, FileSystemRights.Modify, AccessControlType.Allow));
                }
                else
                {
                    if (bool.Parse(singleUserData[3]))
                    {
                        // Read & Execute
                        //add current user with Read.
                        fs.AddAccessRule(new FileSystemAccessRule(si, FileSystemRights.ReadAndExecute, AccessControlType.Allow));
                        //fs.AddAccessRule(new FileSystemAccessRule(fullUserName, FileSystemRights.ReadAndExecute, AccessControlType.Allow));
                    }
                    else
                    {
                        if (bool.Parse(singleUserData[4]))
                        {
                            // Read
                            //add current user with Read.
                            fs.AddAccessRule(new FileSystemAccessRule(si, FileSystemRights.Read, AccessControlType.Allow));
                        }
                    }
                    if (bool.Parse(singleUserData[5]))
                    {
                        // Write
                        //add current user with Write.
                        fs.AddAccessRule(new FileSystemAccessRule(si, FileSystemRights.Write, AccessControlType.Allow));
                    }
                    if (bool.Parse(singleUserData[6]))
                    {
                        // Delete
                        //add current user with Write.
                        fs.AddAccessRule(new FileSystemAccessRule(si, FileSystemRights.Delete, AccessControlType.Allow));
                    }
                }
            }
        }

        public SecurityIdentifier GetSecurityIdentifier(string HostName, string fullUserName, string Path)
        {
            string[] divNames = fullUserName.Split(new string[] { @"\" }, StringSplitOptions.None);
            string siDomain = divNames[0];
            string siUserName = divNames[1];

            string fixedHostName = string.Empty;

            if (Path.StartsWith(@"\\") && Path.Length > 2)
            {
                int slashIndex = Path.Substring(2).IndexOf(@"\");
                fixedHostName = Path.Substring(2, slashIndex);
            }
            else
            {
                fixedHostName = HostName;
            }

            string machineName = GetMachineName(fixedHostName);

            PrincipalContext pc = null;
            if (string.Equals(siDomain, machineName))
            {
                pc = new PrincipalContext(ContextType.Machine, siDomain);
            }
            else
            {
                pc = new PrincipalContext(ContextType.Domain, siDomain);
            }

            Principal prnc = Principal.FindByIdentity(pc, IdentityType.Name, siUserName);
            if (prnc == null)
            {
                prnc = Principal.FindByIdentity(pc, IdentityType.SamAccountName, siUserName);
            }
            return prnc.Sid;
        }

        public ManagementObject CreateNewACE(ManagementScope oms, ManagementObject userAccount, string[] singleUserData)
        {
            // Create a new ACE for the descriptor.
            ManagementObject newAce = new ManagementClass(oms, new ManagementPath("Win32_ACE"), null);
            newAce.Properties["Trustee"].Value = userAccount;

            // Low level ace flags.
            int FILE_READ_DATA = 0x1; // FILE_LIST_DIRECTORY (directory) // 1
            int FILE_READ_ATTRIBUTES = 0x80; // 128
            int FILE_READ_EA = 0x8;          // 8
            int READ_CONTROL = 0x20000;      //131072

            int FILE_WRITE_DATA = 0x2;
            int FILE_APPEND_DATA = 0x4;
            int FILE_DELETE = 0x10000;
            int FILE_EXECUTE = 0x20;

            int FILE_WRITE_EA = 0x10;
            int FILE_WRITE_ATTRIBUTES = 0x100;

            int FILE_READ = FILE_READ_DATA | FILE_READ_ATTRIBUTES | FILE_READ_EA | READ_CONTROL; // 0x1200A9;
            int FILE_MODIFY = 0x1301BF;
            int FULL_CONTROL = 0x1F01FF;


            // Translate FileSystemRights to flags.
            int ACCESS_MASK = 0;

            if (bool.Parse(singleUserData[1]))
            {
                // FullControl
                //newAce.Properties["AccessMask"].Value = FULL_CONTROL; // "2032127";
                ACCESS_MASK = FULL_CONTROL;
            }
            else
            {
                if (bool.Parse(singleUserData[2]))
                {
                    // Modify
                    //newAce.Properties["AccessMask"].Value = FILE_MODIFY; // "1245631"
                    ACCESS_MASK = ACCESS_MASK | FILE_MODIFY;
                }
                else
                {
                    if (bool.Parse(singleUserData[3]))
                    {
                        // Read & Execute
                        ACCESS_MASK = ACCESS_MASK | FILE_READ | FILE_EXECUTE;
                    }
                    else
                    {
                        if (bool.Parse(singleUserData[4]))
                        {
                            // Read
                            ACCESS_MASK = ACCESS_MASK | FILE_READ; // "1179817" 131208
                        }
                    }
                    if (bool.Parse(singleUserData[5]))
                    {
                        // Write
                        ACCESS_MASK = ACCESS_MASK | FILE_WRITE_DATA | FILE_APPEND_DATA | FILE_WRITE_EA | FILE_WRITE_ATTRIBUTES;

                        //newAce.Properties["AccessMask"].Value = ACCESS_MASK; // FILE_WRITE_DATA | FILE_APPEND_DATA | FILE_WRITE_EA | FILE_WRITE_ATTRIBUTES;                      
                    }
                    if (bool.Parse(singleUserData[6]))
                    {
                        // Delete
                        //add current user with Delete.
                        ACCESS_MASK = ACCESS_MASK | FILE_DELETE;
                    }
                }
            }

            newAce.Properties["AccessMask"].Value = ACCESS_MASK;

            // ACL will be inherited.
            newAce.Properties["AceFlags"].Value = 0x10;
            //newAce.Properties["AceFlags"].Value = AceFlags.NoPropagateInherit;


            // Allow access to resource.
            //AceType : Allowed = 0, Denied = 1, Audit = 2
            newAce.Properties["AceType"].Value = 0;

            return newAce;
        }
    }

}
