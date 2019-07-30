using Ayehu.Sdk.ActivityCreation.Interfaces;
using Ayehu.Sdk.ActivityCreation.Extension;
using System.Text;
using System.Data;
using System.IO;
using nsoftware.IPWorksSNMP;

namespace Ayehu.Sdk.ActivityCreation
{
    public class ActivityClass : IActivity
    {
        public Snmpagent agent;
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

        public ICustomActivityResult Execute()

        {
            agent = new Snmpagent();
            dt = new DataTable("resultSet");
            dt.Columns.Add("Result", typeof(string));
            StringWriter sw = new StringWriter();


            agent.RuntimeLicense = "";
            agent.Reset();
            agent.Objects.Clear();


            switch (SNMPVersion)
            {
                case 1:
                    agent.SNMPVersion = SnmpagentSNMPVersions.snmpverV1;
                    if (!string.IsNullOrEmpty(TrapValue))
                    {
                        SNMPObject snmpObj = new SNMPObject(OId)
                        {
                            ObjectType = SNMPObjectTypes.otOctetString,
                            Value = TrapValue
                        };
                        agent.Objects.Add(snmpObj);

                        agent.SendTrap(HostName, "ignoredbecausecustomobjectsspecified");
                    }
                    else
                    {
                        agent.SendTrap(HostName, OId);
                    }
                    dt.Rows.Add("Success");
                    break;
                case 2:
                    agent.SNMPVersion = SnmpagentSNMPVersions.snmpverV2c;
                    if (!string.IsNullOrEmpty(TrapValue))
                    {
                        SNMPObject snmpObj = new SNMPObject(OId)
                        {
                            ObjectType = SNMPObjectTypes.otOctetString,
                            Value = TrapValue
                        };
                        agent.Objects.Add(snmpObj);
                        agent.SendTrap(HostName, "ignoredbecausecustomobjectsspecified");
                    }
                    else
                    {
                        agent.SendTrap(HostName, OId);
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
                    agent.SNMPVersion = SnmpagentSNMPVersions.snmpverV3;
                    if (!string.IsNullOrEmpty(EngineID))
                        agent.LocalEngineId = EngineID;

                    if (!string.IsNullOrEmpty(TrapValue))
                    {
                        SNMPObject snmpObj = new SNMPObject(OId)
                        {
                            ObjectType = SNMPObjectTypes.otOctetString,
                            Value = TrapValue
                        };
                        agent.Objects.Add(snmpObj);
                        agent.SendSecureTrap(HostName, "ignoredbecausecustomobjectsspecified", UName, authAlgo, AuthPassword, encAlgo, EncPassword);
                    }
                    else
                    {
                        agent.SendSecureTrap(HostName, OId, UName, authAlgo, AuthPassword, encAlgo, EncPassword);
                    }

                    dt.Rows.Add("Success");
                    break;
            }// end switch



            agent.Dispose();
            return this.GenerateActivityResult(dt);
        }//end Execute	

    }// end class
}// end namespace
