using Ayehu.Sdk.ActivityCreation.Interfaces;
using Ayehu.Sdk.ActivityCreation.Extension;
using System.Text;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Data;
using System.Diagnostics;

namespace Ayehu.Sdk.ActivityCreation
{
    public class ActivityClass : IActivity
    {
        public string HostName;
        public string UserName;
        public string Password;
        public string SpecName;
        public string PersonName;
        public string Organization;
        public string UseVMNameComputerName;
        public string ComputerName;
        public string ProductKey;
        public string IncludeLicenseInformation;
        public string ServerRegType;
        public string ServerMaxConnections;
        public string AdminPassword;
        public string AdminAutoLogin;
        public string AdminAutoLoginCount;
        public string RunOnceCommand;
        public string NetworkIPConfig;
        public string NetworkIPConfigIP;
        public string NetworkIPConfigSubnet;
        public string NetworkIPConfigDefaultGateway;
        public string NetworkIPConfigAlternateGateway;
        public string NetworkDNSConfig;
        public string NetworkDNSConfigDefault;
        public string NetworkDNSConfigSecondary;
        public string DomainConfiguration;
        public string DomainWorkgroup;
        public string DomainServerDomain;
        public string DomainServerUsername;
        public string DomainServerPassword;
        public string GenerateSID;

        public ICustomActivityResult Execute()

        {
            StringWriter sw = new StringWriter();
            DataTable dt = new DataTable("resultSet");
            dt.Columns.Add("Result", typeof(String));
            string sResult = "";

            try
            {
                string command_path = "VMWare.exe";
                DataTable dtParams = new DataTable("Params");
                dtParams.Columns.Add("Command");
                dtParams.Columns.Add("HostName");
                dtParams.Columns.Add("UserName");
                dtParams.Columns.Add("Password");
                dtParams.Columns.Add("SpecName");
                dtParams.Columns.Add("PersonName");
                dtParams.Columns.Add("Organization");
                dtParams.Columns.Add("UseVMNameComputerName");
                dtParams.Columns.Add("ComputerName");
                dtParams.Columns.Add("ProductKey");
                dtParams.Columns.Add("IncludeLicenseInformation");
                dtParams.Columns.Add("ServerRegType");
                dtParams.Columns.Add("ServerMaxConnections");
                dtParams.Columns.Add("AdminPassword");
                dtParams.Columns.Add("AdminAutoLogin");
                dtParams.Columns.Add("AdminAutoLoginCount");
                dtParams.Columns.Add("RunOnceCommand");
                dtParams.Columns.Add("NetworkIPConfig");
                dtParams.Columns.Add("NetworkIPConfigIP");
                dtParams.Columns.Add("NetworkIPConfigSubnet");
                dtParams.Columns.Add("NetworkIPConfigDefaultGateway");
                dtParams.Columns.Add("NetworkIPConfigAlternateGateway");
                dtParams.Columns.Add("NetworkDNSConfig");
                dtParams.Columns.Add("NetworkDNSConfigDefault");
                dtParams.Columns.Add("NetworkDNSConfigSecondary");
                dtParams.Columns.Add("DomainConfiguration");
                dtParams.Columns.Add("DomainWorkgroup");
                dtParams.Columns.Add("DomainServerDomain");
                dtParams.Columns.Add("DomainServerUsername");
                dtParams.Columns.Add("DomainServerPassword");
                dtParams.Columns.Add("GenerateSID");

                DataRow rParams = dtParams.NewRow();
                rParams["Command"] = "VMCreateConfigurationSpec";
                rParams["HostName"] = HostName;
                rParams["UserName"] = UserName;
                rParams["Password"] = Password;
                rParams["SpecName"] = SpecName;
                rParams["PersonName"] = PersonName;
                rParams["Organization"] = Organization;
                rParams["UseVMNameComputerName"] = UseVMNameComputerName;
                rParams["ComputerName"] = ComputerName;
                rParams["ProductKey"] = ProductKey;
                rParams["IncludeLicenseInformation"] = IncludeLicenseInformation;
                rParams["ServerRegType"] = ServerRegType;
                rParams["ServerMaxConnections"] = ServerMaxConnections;
                rParams["AdminPassword"] = AdminPassword;
                rParams["AdminAutoLogin"] = AdminAutoLogin;
                rParams["AdminAutoLoginCount"] = AdminAutoLoginCount;
                rParams["RunOnceCommand"] = RunOnceCommand;
                rParams["NetworkIPConfig"] = NetworkIPConfig;
                rParams["NetworkIPConfigIP"] = NetworkIPConfigIP;
                rParams["NetworkIPConfigSubnet"] = NetworkIPConfigSubnet;
                rParams["NetworkIPConfigDefaultGateway"] = NetworkIPConfigDefaultGateway;
                rParams["NetworkIPConfigAlternateGateway"] = NetworkIPConfigAlternateGateway;
                rParams["NetworkDNSConfig"] = NetworkDNSConfig;
                rParams["NetworkDNSConfigDefault"] = NetworkDNSConfigDefault;
                rParams["NetworkDNSConfigSecondary"] = NetworkDNSConfigSecondary;
                rParams["DomainConfiguration"] = DomainConfiguration;
                rParams["DomainWorkgroup"] = DomainWorkgroup;
                rParams["DomainServerDomain"] = DomainServerDomain;
                rParams["DomainServerUsername"] = DomainServerUsername;
                rParams["DomainServerPassword"] = DomainServerPassword;
                rParams["GenerateSID"] = GenerateSID;

                dtParams.Rows.Add(rParams);

                dtParams.WriteXml(sw, XmlWriteMode.WriteSchema, false);

                Process prVMWare = new Process();
                prVMWare.StartInfo.FileName = command_path;
                prVMWare.StartInfo.Arguments = "\"" + sw.ToString().Replace("\"", "\\\"") + "\"";
                prVMWare.StartInfo.UseShellExecute = false;
                prVMWare.StartInfo.CreateNoWindow = true;
                prVMWare.StartInfo.RedirectStandardError = true;
                prVMWare.StartInfo.RedirectStandardInput = true;
                prVMWare.StartInfo.RedirectStandardOutput = true;

                prVMWare.Start();
                StreamReader srResult = prVMWare.StandardOutput;
                sResult = srResult.ReadToEnd();

                srResult.Close();
                prVMWare.Close();

            }
            catch (Exception e)
            {
                //dt.Rows.Add(dt.NewRow()["Result"] = e.Message);
                throw;
            }
            if (sResult == "")
            {
                return this.GenerateActivityResult(dt);
            }
            else
            {
                return this.GenerateActivityResult(sResult);
            }
        }
    }
}
