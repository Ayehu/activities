using Ayehu.Sdk.ActivityCreation.Interfaces;
using Ayehu.Sdk.ActivityCreation.Extension;
using System.Text;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Data;
using System.Diagnostics;
using System.Collections;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Collections.ObjectModel;

namespace Ayehu.Sdk.ActivityCreation
{
    public class ActivityClass : IActivity
    {
        public string HostName;
        public string UserName;
        public string Password;
        public string PropertyName;
        public string DbName;

        public ICustomActivityResult Execute()

        {
            string command = "Get-MailboxDatabase";

            DataTable dt = new DataTable("resultSet");

            string shellUri = "http://schemas.microsoft.com/powershell/Microsoft.Exchange";
            System.Uri serverUri = new Uri(String.Format("HTTPS://{0}/powershell?serializationLevel=Full", HostName));

            System.Security.SecureString securePassword = new System.Security.SecureString();
            foreach (char c in Password.ToCharArray())
            {
                securePassword.AppendChar(c);
            }
            PSCredential creds = new PSCredential(UserName, securePassword);

            RunspaceConfiguration rc = RunspaceConfiguration.Create();
            WSManConnectionInfo wsManInfo = new WSManConnectionInfo(serverUri, shellUri, creds);
            wsManInfo.SkipCNCheck = true;
            wsManInfo.SkipCACheck = true;

            using (var runspace = RunspaceFactory.CreateRunspace(wsManInfo))
            {
                runspace.Open();

                using (Pipeline pipeline = runspace.CreatePipeline())
                {
                    string COMMAND = "Get-MailboxDatabase -Server " + HostName + " -Status -ErrorAction stop | Select-Object " + PropertyName + ", Name";
                    string cmdLine = string.Format("&{0}", COMMAND);

                    pipeline.Commands.AddScript(cmdLine);

                    Collection<PSObject> results = pipeline.Invoke();

                    dt.Columns.Add(PropertyName, typeof(String));

                    if (string.IsNullOrEmpty(DbName))
                    {
                        foreach (PSObject obj in results)
                        {
                            PSPropertyInfo psInfo = obj.Properties[PropertyName];
                            if (psInfo != null)
                            {
                                if (psInfo.Value != null)
                                {
                                    DataRow dr = dt.NewRow();
                                    dr[PropertyName] = psInfo.Value.ToString();
                                    dt.Rows.Add(dr);
                                }
                            }
                            else
                            {
                                throw new Exception(string.Format("The property \"{0}\" does not exist.", PropertyName));
                            }
                        }
                    }
                    else
                    {
                        if (results != null)
                        {
                            var filteredResult = results.Select(p => p).Where(p => p.Properties["Name"].Value.ToString() == DbName).ToList();
                            if (filteredResult.Count > 0)
                            {
                                PSObject obj = filteredResult[0];
                                PSPropertyInfo psInfo = obj.Properties[PropertyName];
                                if (psInfo != null)
                                {
                                    if (psInfo.Value != null)
                                    {
                                        DataRow dr = dt.NewRow();
                                        dr[PropertyName] = psInfo.Value.ToString();
                                        dt.Rows.Add(dr);
                                    }
                                }
                                else
                                {
                                    throw new Exception(string.Format("The property \"{0}\" does not exist.", PropertyName));
                                }
                            }
                            else
                            {
                                throw new Exception("The specified BD Name wasn't found.");
                            }
                        }
                    }

                }




                // ==================================

                //using (PowerShell powershell = PowerShell.Create())
                //{

                //    powershell.Runspace = runspace;
                //    powershell.AddCommand(command);

                //	if(string.Equals(PropertyName, "DatabaseSize"))
                //	{
                //		//powershell.AddCommand(command + " -Status");
                //		powershell.Commands.AddParameter("Status", null);
                //	}


                //    powershell.Commands.AddParameter("Server", HostName); //ServerName);
                //    powershell.Commands.AddParameter("ErrorAction", "stop");

                //	//Command cmd2 = new Command("Select-Object");
                //	//cmd2.AddParameter("PropertyName");
                //	//powershell.AddCommand(cmd2);

                //    Collection<PSObject> results = powershell.Invoke();

                //    dt.Columns.Add(PropertyName, typeof(String));

                //    if (string.IsNullOrEmpty(DbName))
                //    {
                //        foreach (PSObject obj in results)
                //        {
                //            PSPropertyInfo psInfo = obj.Properties[PropertyName];
                //            if (psInfo != null)
                //            {
                //                if (psInfo.Value != null)
                //                {
                //                    DataRow dr = dt.NewRow();
                //                    dr[PropertyName] = psInfo.Value.ToString();
                //                    dt.Rows.Add(dr);
                //                }
                //            }
                //            else
                //            {
                //                throw new Exception(string.Format("The property \"{0}\" does not exist.", PropertyName));
                //           }
                //        }
                //    }
                //    else
                //    {
                //        var filteredResult = results.Where(p => p.ToString() == DbName).ToList();
                //        if (filteredResult.Count > 0)
                //        {
                //            PSObject obj = filteredResult[0];
                //            PSPropertyInfo psInfo = obj.Properties[PropertyName];
                //            if (psInfo != null)
                //            {
                //                if (psInfo.Value != null)
                //                {
                //                    DataRow dr = dt.NewRow();
                //                    dr[PropertyName] = psInfo.Value.ToString();
                //                    dt.Rows.Add(dr);
                //                }
                //            }
                //            else
                //            {
                //                throw new Exception(string.Format("The property \"{0}\" does not exist.", PropertyName));
                //            }
                //        }
                //        else
                //        {
                //            throw new Exception("The specified database does not exist");
                //        }
                //    }
                //}
            }
            return this.GenerateActivityResult(dt);
        }
    }
}
