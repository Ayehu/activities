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
        public Snmpmgr snmpMgr = new Snmpmgr();
        public DataTable dt = new DataTable("resultSet");
        public string OId;
        public string HostName;
        public int SnmpVersion;
        public string UName;
        public string AuthPassword;
        public string AuthMethod;
        public string EncPassword;
        public string EncMethod;
        public int isWalkLimit;
        public string WalkLimit;
        public string Community;
        public string EngineID;

        public ICustomActivityResult Execute()

        {
            dt.Columns.Add("Oid", typeof(string));
            dt.Columns.Add("Type", typeof(string));
            dt.Columns.Add("Value", typeof(string));
            StringWriter sw = new StringWriter();

            snmpMgr.RuntimeLicense = "";
            snmpMgr.RemoteHost = HostName;
            snmpMgr.Community = Community;
            snmpMgr.StoreWalkObjects = true;
            switch (SnmpVersion)
            {
                case 1:
                    snmpMgr.SNMPVersion = SnmpmgrSNMPVersions.snmpverV1;
                    break;
                case 2:
                    snmpMgr.SNMPVersion = SnmpmgrSNMPVersions.snmpverV2c;
                    break;
                case 3:
                    SnmpmgrEncryptionAlgorithms encAlgo;
                    SnmpmgrAuthenticationProtocols authAlgo;
                    switch (EncMethod)
                    {
                        case "DES":
                            encAlgo = SnmpmgrEncryptionAlgorithms.encraDES;
                            break;
                        case "AES":
                            encAlgo = SnmpmgrEncryptionAlgorithms.encraAES;
                            break;
                        case "3DES":
                            encAlgo = SnmpmgrEncryptionAlgorithms.encra3DES;
                            break;
                        default:
                            encAlgo = SnmpmgrEncryptionAlgorithms.encraDES;
                            break;
                    }
                    switch (AuthMethod)
                    {
                        case "MD5":
                            authAlgo = SnmpmgrAuthenticationProtocols.authpHMACMD596;
                            break;
                        case "SHA":
                            authAlgo = SnmpmgrAuthenticationProtocols.authpHMACSHA96;
                            break;
                        default:
                            authAlgo = SnmpmgrAuthenticationProtocols.authpHMACMD596;
                            break;
                    }
                    snmpMgr.SNMPVersion = SnmpmgrSNMPVersions.snmpverV3;
                    snmpMgr.EncryptionPassword = EncPassword;
                    snmpMgr.EncryptionAlgorithm = encAlgo;
                    snmpMgr.AuthenticationPassword = AuthPassword;
                    snmpMgr.AuthenticationProtocol = authAlgo;
                    if (!string.IsNullOrEmpty(EngineID))
                        snmpMgr.LocalEngineId = EngineID;
                    break;
            }// end switch
            if (isWalkLimit == 1)
            {
                int result = 0;
                if (int.TryParse(WalkLimit, out result))
                {
                    snmpMgr.WalkLimit = result;
                    snmpMgr.Timeout = 100;
                    snmpMgr.Walk(OId);

                }
                else
                {
                    //dt = new DataTable("resultSet");
                    //dt.Columns.Add("Result", typeof(string));
                    //dt.Rows.Add("Failure - the walk limit provided was not an integer.");
                    throw new Exception("the walk limit provided was not an integer.");
                }
            }
            else
            {
                snmpMgr.Timeout = 100;
                snmpMgr.Walk(OId);
            }
            foreach (var obj in snmpMgr.Objects)
            {
                dt.Rows.Add(obj.Oid, obj.TypeString, obj.Value);
            }



            snmpMgr.Dispose();
            return this.GenerateActivityResult(dt);
        }//end Execute		

    }// end class
}// end namespace
