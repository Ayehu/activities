using Ayehu.Sdk.ActivityCreation.Interfaces;
using Ayehu.Sdk.ActivityCreation.Extension;
using System.Text;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Data;
using System.Diagnostics;
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
        public string ScriptCode;
        public string ScriptPath;
        public string PropertyNames;


        public ICustomActivityResult Execute()
        {
            StringWriter sw = new StringWriter();
            DataTable dt = new DataTable("resultSet");
            string strResult = string.Empty;

            string _exchangeConnectionUri = string.Format("http://{0}/PowerShell/?SerializationLevel=Full", HostName);

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


            // Prepare Runspace
            using (var runspace = RunspaceFactory.CreateRunspace())
            {
                runspace.Open();

                object psSessionConnection;

                // Create a powershell session for remote exchange server
                using (var powershell = PowerShell.Create())
                {
                    var command = new PSCommand();
                    command.AddCommand("New-PSSession");
                    command.AddParameter("ConfigurationName", "Microsoft.Exchange");
                    command.AddParameter("ConnectionUri", new Uri(_exchangeConnectionUri));
                    command.AddParameter("Authentication", "Kerberos");
                    powershell.Commands = command;
                    powershell.Runspace = runspace;

                    // TODO: Handle errors
                    var result = powershell.Invoke();
                    if (result != null)
                    {
                        if (result.Count > 0)
                        {
                            psSessionConnection = result[0];
                        }
                        else
                        {
                            throw new Exception("Failed to create a powershell session for remote exchange server");
                        }
                    }
                    else
                    {
                        throw new Exception("Failed to create a powershell session for remote exchange server");
                    }
                }

                // Set ExecutionPolicy on the process to unrestricted
                using (var powershell = PowerShell.Create())
                {
                    var command = new PSCommand();
                    command.AddCommand("Set-ExecutionPolicy");
                    command.AddParameter("Scope", "Process");
                    command.AddParameter("ExecutionPolicy", "Unrestricted");
                    powershell.Commands = command;
                    powershell.Runspace = runspace;

                    powershell.Invoke();
                }

                // Import remote exchange session into runspace
                using (var powershell = PowerShell.Create())
                {
                    var command = new PSCommand();
                    command.AddCommand("Import-PSSession");
                    command.AddParameter("Session", psSessionConnection);
                    powershell.Commands = command;
                    powershell.Runspace = runspace;

                    powershell.Invoke();
                }

                // Ready to run commands (could function as independent Activity)
                using (var powershell = PowerShell.Create())
                {

                    powershell.Commands.AddScript(ScriptCode);

                    powershell.Runspace = runspace;

                    //var result = powershell.Invoke();

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


                            dt.WriteXml(sw, XmlWriteMode.WriteSchema, false);
                            strResult = sw.ToString();
                        }
                        //else
                        //{
                        //    throw new Exception(errorDescription);
                        //}
                    }
                    else
                    {
                        throw new Exception(errorDescription);
                    }

                }

            }

            return this.GenerateActivityResult(strResult);

        }

        private string ClearString(string Result)
        {

            Result = Result.Trim();

            //Result = Result.Replace("\r\n", "");
            ////Result = Result.Replace("\t", "");
            //Result = Result.Replace("\r", "");
            //Result = Result.Replace("\n", "");

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