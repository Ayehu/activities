using Ayehu.Sdk.ActivityCreation.Interfaces;
using Ayehu.Sdk.ActivityCreation.Extension;
using System.Text;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Data;
using System.Diagnostics;
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
        public string UserLogonName;
        public string DatabaseName;

        public ICustomActivityResult Execute()

        {
            string command = "Enable-Mailbox";

            DataTable dt = new DataTable("resultSet");
            dt.Columns.Add("Result", typeof(String));


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
            //wsManInfo.AuthenticationMechanism = AuthenticationMechanism.Default;
            wsManInfo.SkipCNCheck = true;
            wsManInfo.SkipCACheck = true;

            using (var runspace = RunspaceFactory.CreateRunspace(wsManInfo))
            {
                runspace.Open();
                using (PowerShell powershell = PowerShell.Create())
                {
                    powershell.Runspace = runspace;
                    powershell.AddCommand(command);
                    powershell.Commands.AddParameter("Identity", UserLogonName);
                    powershell.Commands.AddParameter("Database", DatabaseName);
                    powershell.Commands.AddParameter("ErrorAction", "stop");

                    string errorDescription = string.Format("{0} command execution has failed.", command);
                    Collection<PSObject> results = powershell.Invoke();
                    if (results != null)
                    {
                        if (results.Count > 0)
                        {
                            object resValue = results[0].Properties["IsMailboxEnabled"].Value;
                            if (resValue != null)
                            {
                                if (Convert.ToBoolean(resValue))
                                {
                                    dt.Rows.Add("Success");
                                }
                                else
                                {
                                    throw new Exception(errorDescription);
                                }
                            }
                            else
                            {
                                throw new Exception(errorDescription);
                            }
                        }
                        else
                        {
                            throw new Exception(errorDescription);
                        }
                    }
                    else
                    {
                        throw new Exception(errorDescription);
                    }
                }
            }
            return this.GenerateActivityResult(dt);
        }
    }
}
