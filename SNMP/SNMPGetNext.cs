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
        public string UserName;
        public string AuthPassword;
        public string AuthMethod;
        public string EncPassword;
        public string EncMethod;
        public string Community;
        public string EngineID;

        public ICustomActivityResult Execute()

        {

            dt.Columns.Add("Result", typeof(string));
            StringWriter sw = new StringWriter();

            snmpMgr.RuntimeLicense = "";
            snmpMgr.OnResponse += new Snmpmgr.OnResponseHandler(snmpmgr_OnResponse);
            snmpMgr.RemoteHost = HostName;
            snmpMgr.Community = Community;
            snmpMgr.Objects.Add(new SNMPObject(OId, null, SNMPObjectTypes.otOctetString));
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
                    if (!String.IsNullOrEmpty(EngineID))
                        snmpMgr.LocalEngineId = EngineID;
                    break;
            }// end switch
            snmpMgr.SendGetNextRequest();
            snmpMgr.Timeout = 10;



            return this.GenerateActivityResult(dt);
        }//end Execute		

        private void snmpmgr_OnResponse(object sender, SnmpmgrResponseEventArgs e)
        {
            if (snmpMgr.Objects.Count >= 1)
            {
                dt.Rows.Add(snmpMgr.Objects[0].Value);
            }
        }
    }// end class
}// end namespace
