using Ayehu.Sdk.ActivityCreation.Interfaces;
using Ayehu.Sdk.ActivityCreation.Extension;
using System;
using System.IO;
using System.Data;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Collections.ObjectModel;
using System.Text;

namespace Ayehu.Sdk.ActivityCreation
{
    public class ActivityClass : IActivity
    {
        public string HostName;
        public string UserName;
        public string Password;
        public string ScriptCode;
        public string ScriptPath;
        public string PropertyNames;


        public ICustomActivityResult Execute()
        {
            DataTable dt = new DataTable();

            if (!string.IsNullOrEmpty(ScriptPath))
            {
                if (File.Exists(ScriptPath))
                {
                    ScriptCode = File.ReadAllText(ScriptPath);
                }
                else
                {
                    throw new Exception("File not found");
                }
            }
            if (string.IsNullOrEmpty(ScriptCode))
            {
                throw new Exception("Script code value is empty.");
            }

            string _exchangeConnectionUri = string.Format("http://{0}/PowerShell/?SerializationLevel=Full", HostName);
            StringBuilder sbInit = new StringBuilder();
            sbInit.AppendLine("$ErrorActionPreference = \"Stop\"");
            sbInit.AppendLine("$PSDefaultParameterValues['*:ErrorAction']='Stop'");
            sbInit.AppendLine("$session = New-PSSession -ConfigurationName Microsoft.Exchange -ConnectionUri " + _exchangeConnectionUri + " -Authentication Kerberos");
            sbInit.AppendLine("Set-ExecutionPolicy -Scope Process -ExecutionPolicy Unrestricted");
            sbInit.AppendLine("Import-PSSession $session");

            try
            {
                using (PowerShellProcessInstance instance = new PowerShellProcessInstance(new Version(4, 0), null, ScriptBlock.Create(sbInit.ToString()), false))
                {

                    using (var runspace = RunspaceFactory.CreateOutOfProcessRunspace(new TypeTable(new string[0]), instance))
                    {
                        runspace.Open();

                        using (var powershell = PowerShell.Create())
                        {
                            powershell.Commands.AddScript(ScriptCode);
                            powershell.Runspace = runspace;
                            string errorDescription = "Command execution has failed.";
                            Collection<PSObject> results = powershell.Invoke();

                            if (results != null)
                            {
                                if (results.Count > 0)
                                {
                                    var selectedResults = results.Where(o => o != null);
                                    bool isStartLine = true;
                                    string cmdResult = string.Empty;
                                    try
                                    {
                                        if (string.IsNullOrEmpty(PropertyNames))
                                        {
                                            // The properties were not provided
                                            dt.Columns.Add("Result", typeof(string));
                                            foreach (var output in selectedResults)
                                            {

                                                if (isStartLine)
                                                {
                                                    cmdResult = ClearString(output.ToString());
                                                    isStartLine = false;
                                                }
                                                else
                                                {
                                                    cmdResult = output.ToString();
                                                }
                                                dt.Rows.Add(cmdResult);
                                            }

                                        }
                                        else
                                        {
                                            // The properties were provided
                                            string[] properties = PropertyNames.Split(';');
                                            foreach (string propertyName in properties)
                                            {
                                                dt.Columns.Add(propertyName, typeof(string));
                                            }

                                            foreach (var output in selectedResults)
                                            {
                                                DataRow dr = dt.NewRow();
                                                bool propertyFound = false;
                                                foreach (string propertyName in properties)
                                                {
                                                    PSPropertyInfo psInfo = output.Properties[propertyName];
                                                    if (psInfo != null)
                                                    {
                                                        if (psInfo.Value != null)
                                                        {

                                                            dr[propertyName] = psInfo.Value.ToString();
                                                            propertyFound = true;

                                                        }
                                                    }
                                                    else
                                                    {
                                                        throw new Exception(string.Format("The property \"{0}\" does not exist.", propertyName));
                                                    }
                                                }
                                                if (propertyFound)
                                                {
                                                    dt.Rows.Add(dr);
                                                    propertyFound = false;
                                                }
                                            }
                                        }
                                    }
                                    catch
                                    {
                                        throw new Exception("Unable to evaluate result.");
                                    }
                                }
                            }
                            else
                            {
                                throw new Exception(errorDescription);
                            }

                        }

                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;

            }

            return this.GenerateActivityResult(dt);

        }

        private string ClearString(string Result)
        {
            Result = Result.Trim();
            if (Result.StartsWith("\r\n"))
                Result = Result.Replace("\r\n", "");

            if (Result.StartsWith("\t"))
                Result = Result.Replace("\t", "");

            if (Result.StartsWith("\n"))
                Result = Result.Replace("\n", "");

            return Result.Trim();
        }

    }
}
