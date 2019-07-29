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
        public string GroupName;
        public string UserLogonName;

        public ICustomActivityResult Execute()

        {
            string command = "Add-DistributionGroupMember";

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
                using (PowerShell powershell = PowerShell.Create())
                {
                    powershell.Runspace = runspace;
                    powershell.AddCommand(command);
                    powershell.Commands.AddParameter("Identity", GroupName);
                    powershell.Commands.AddParameter("Member", UserLogonName);
                    powershell.Commands.AddParameter("ErrorAction", "stop");

                    string errorDescription = string.Format("{0} command execution has failed.", command);
                    Collection<PSObject> results = powershell.Invoke();

                    if (results != null)
                    {
                        dt.Columns.Add("Result", typeof(String));
                        dt.Rows.Add("Success");
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
