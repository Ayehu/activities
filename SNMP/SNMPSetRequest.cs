using Ayehu.Sdk.ActivityCreation.Interfaces;
using Ayehu.Sdk.ActivityCreation.Extension;
using System.Text;
using System;
using System.Data;
using System.IO;
using nsoftware.IPWorksSNMP;

namespace Ayehu.Sdk.ActivityCreation
{
    public class ActivityClass : IActivity
    {
        public Snmpmgr agent = new Snmpmgr();
        public DataTable dt;
        public string OId;
        public string HostName;
        public int SNMPVersion;
        public string UName;
        public string AuthPassword;
        public string AuthMethod;
        public string EncPassword;
        public string EncMethod;
        public string EngineID;
        public string TrapValue;
        public string Community;
        public string ValueType;

        public ICustomActivityResult Execute()

        {

            dt = new DataTable("resultSet");
            dt.Columns.Add("Result", typeof(string));
            StringWriter sw = new StringWriter();

            if (string.IsNullOrEmpty(ValueType))
                throw new Exception("Value type is missing");
            agent.RuntimeLicense = "";
            agent.Reset();
            agent.Objects.Clear();


            switch (SNMPVersion)
            {
                case 1:
                    agent.SNMPVersion = SnmpmgrSNMPVersions.snmpverV1;
                    if (!string.IsNullOrEmpty(TrapValue))
                    {
                        SNMPObject snmpObj = new SNMPObject(OId);
                        if (ValueType.ToLower() == "string")
                            snmpObj.ObjectType = SNMPObjectTypes.otOctetString;

                        if (ValueType.ToLower() == "integer")
                            snmpObj.ObjectType = SNMPObjectTypes.otInteger;
                        snmpObj.Value = TrapValue;
                        agent.Objects.Add(snmpObj);
                        agent.RemoteHost = HostName;
                        agent.Community = Community;
                        agent.SendSetRequest();
                    }
                    else
                    {
                        throw new Exception("Value is missing");
                    }
                    dt.Rows.Add("Success");
                    break;
                case 2:
                    agent.SNMPVersion = SnmpmgrSNMPVersions.snmpverV2c;
                    if (!string.IsNullOrEmpty(TrapValue))
                    {
                        SNMPObject snmpObj = new SNMPObject(OId);
                        if (ValueType.ToLower() == "string")
                            snmpObj.ObjectType = SNMPObjectTypes.otOctetString;

                        if (ValueType.ToLower() == "integer")
                            snmpObj.ObjectType = SNMPObjectTypes.otInteger;
                        snmpObj.Value = TrapValue;
                        agent.Objects.Add(snmpObj);
                        agent.RemoteHost = HostName;
                        agent.Community = Community;
                        agent.SendSetRequest();

                    }
                    else
                    {
                        throw new Exception("Value is missing");
                    }

                    dt.Rows.Add("Success");
                    break;
                case 3:
                    int encAlgo;
                    int authAlgo;


                    switch (EncMethod)
                    {
                        case "DES":
                            encAlgo = 1;
                            break;
                        case "AES":
                            encAlgo = 2;
                            break;
                        case "3DES":
                            encAlgo = 3;
                            break;
                        default:
                            encAlgo = 1;
                            break;
                    }
                    switch (AuthMethod)
                    {
                        case "MD5":
                            authAlgo = 1;
                            break;
                        case "SHA":
                            authAlgo = 2;
                            break;
                        default:
                            authAlgo = 1;
                            break;
                    }
                    agent.SNMPVersion = SnmpmgrSNMPVersions.snmpverV3;
                    if (!String.IsNullOrEmpty(EngineID))
                        agent.LocalEngineId = EngineID;

                    if (!string.IsNullOrEmpty(TrapValue))
                    {
                        SNMPObject snmpObj = new SNMPObject(OId);
                        if (ValueType.ToLower() == "string")
                            snmpObj.ObjectType = SNMPObjectTypes.otOctetString;

                        if (ValueType.ToLower() == "integer")
                            snmpObj.ObjectType = SNMPObjectTypes.otInteger;
                        snmpObj.Value = TrapValue;
                        agent.Objects.Add(snmpObj);
                        agent.RemoteHost = HostName;
                        agent.Community = Community;
                        agent.SendSetRequest();
                        //agent.SendSecureTrap(HostName, "ignoredbecausecustomobjectsspecified", UName, authAlgo, AuthPassword, encAlgo, EncPassword);

                    }
                    else
                    {
                        throw new Exception("Value is missing");
                    }

                    dt.Rows.Add("Success");
                    break;
            }// end switch



            agent.Dispose();
            return this.GenerateActivityResult(dt);
        }//end Execute	

    }// end class
}// end namespace
