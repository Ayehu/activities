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
        public Snmpmgr SNMPMgr;
        public DataTable dt;
        public string objID;
        public string OId;
        public string HostName;
        public int SNMPVersion;
        public string UName;
        public string AuthPassword;
        public string AuthMethod;
        public string EncPassword;
        public string EncMethod;
        public string Community;
        public string EngineID;

        public ICustomActivityResult Execute()

        {
            SNMPMgr = new Snmpmgr();
            dt = new DataTable("resultSet");
            objID = string.Empty;
            dt.Columns.Add("Result", typeof(string));
            StringWriter sw = new StringWriter();

            SNMPMgr.RuntimeLicense = "";
            SNMPMgr.OnResponse += SNMPmgr_OnResponse;
            //SNMPMgr.OnDiscover
            SNMPMgr.RemoteHost = HostName;
            SNMPMgr.Objects.Add(new SNMPObject(OId, null, SNMPObjectTypes.otOctetString));
            objID = OId;
            SNMPMgr.Community = Community;

            switch (SNMPVersion)
            {
                case 1:
                    SNMPMgr.SNMPVersion = SnmpmgrSNMPVersions.snmpverV1;
                    break;
                case 2:
                    SNMPMgr.SNMPVersion = SnmpmgrSNMPVersions.snmpverV2c;
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
                    SNMPMgr.User = UName;
                    SNMPMgr.SNMPVersion = SnmpmgrSNMPVersions.snmpverV3;
                    SNMPMgr.EncryptionPassword = EncPassword;
                    SNMPMgr.EncryptionAlgorithm = encAlgo;
                    SNMPMgr.AuthenticationPassword = AuthPassword;
                    SNMPMgr.AuthenticationProtocol = authAlgo;
                    if (!string.IsNullOrEmpty(EngineID))
                        SNMPMgr.LocalEngineId = EngineID;
                    SNMPMgr.Discover();
                    break;
            }// end switch
            SNMPMgr.SendGetRequest();
            SNMPMgr.Timeout = 1000;



            SNMPMgr.Dispose();
            return this.GenerateActivityResult(dt);
        }//end Execute             

        private void SNMPmgr_OnResponse(object sender, SnmpmgrResponseEventArgs e)
        {
            if (SNMPMgr.Objects.Count >= 1 && SNMPMgr.Objects[0].Oid == objID)
            {
                dt.Rows.Add(SNMPMgr.Objects[0].Value);
            }
        }


    }// end class
}// end namespace
