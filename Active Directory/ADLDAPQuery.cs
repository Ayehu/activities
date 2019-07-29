using Ayehu.Sdk.ActivityCreation.Interfaces;
using Ayehu.Sdk.ActivityCreation.Extension;
using System.Text;
using System;
using System.Collections;
using System.Data;
using System.DirectoryServices;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;

namespace Ayehu.Sdk.ActivityCreation
{
    public class ActivityClass : IActivity
    {
        internal const string DefaultAdPort = "389";
        internal const string DefaultAdSecurePort = "636";
        public string HostName;
        public string UserName;
        public string Password;
        public string Domain;
        public string Ou;
        public string Query;
        public string Properties;
        public string Scope;
        public string SecurePort;

        public ICustomActivityResult Execute()

        {
            ArrayList arr = new ArrayList { "accountexpires"
                                                      , "badpasswordtime"
                                                      , "lastlogoff"
                                                      , "lastlogon"
                                                      , "lastlogontimestamp"
                                                      , "lockoutduration"
                                                      , "lockoutobservationwindow"
                                                      , "maxpwdage"
                                                      , "lockouttime"
                                                      , "minpwdage"
                                                      , "msds-lastfailedinteractivelogontime"
                                                      , "msds-lastsuccessfulinteractivelogontime"
                                                      , "msds-userpasswordexpirytimecomputed"
                                                      , "pwdlastset"};

            string path = string.Empty;
            if (!string.IsNullOrEmpty(Domain) && Domain.Contains('.'))
            {
                path = GetDomainPath(Domain);
            }


            if (string.IsNullOrEmpty(SecurePort))
            {
                SecurePort = DefaultAdPort;
            }

            if (SecurePort.All(c => c >= '0' && c <= '9') == false)
            {
                var msg = "Port parameter must be a number";
                throw new ApplicationException(msg);
            }

            SearchResultCollection allOUS;
            DataTable dt = new DataTable();
            dt.TableName = "resultSet";
            try
            {
                if (!string.IsNullOrEmpty(Ou) && string.IsNullOrEmpty(path))
                    throw new Exception("Domain must be specified when ou is searched");

                if (!string.IsNullOrEmpty(Ou) && Ou.ToLower() != "ou=")
                {
                    if (!(Ou.ToLower().Contains("cn=") || Ou.ToLower().Contains("ou=")))
                        Ou = Ou.Replace("/", @"\");

                    Ou = Ou.Trim();
                    if (Ou.Contains(@"\"))
                    {
                        if (Ou.StartsWith(@"\"))
                            Ou = "ou=" + Ou.Substring(1);
                        Ou = Ou.Replace(@"\", ",ou=");
                    }

                    if (!Ou.StartsWith("ou="))
                        Ou = "ou=" + Ou;

                    if (Ou.ToLower().StartsWith("ou=users") || Ou.ToLower().StartsWith("ou=builtin") || Ou.ToLower().StartsWith("ou=microsoft exchange system objects") || Ou.ToLower().StartsWith("ou=system") || Ou.ToLower().StartsWith("ou=program data") || Ou.ToLower().StartsWith("ou=managed service accounts") || Ou.ToLower().StartsWith("ou=lostandfound") || Ou.ToLower().StartsWith("ou=computers") || Ou.ToLower().StartsWith("ou=foreignsecurityprincipals") || Ou.ToLower().StartsWith("ou=ntds quotas"))
                        Ou = "cn=" + Ou.Substring(3);

                    string TheNewPathBackup = Ou;
                    try
                    {
                        string PathStart = Ou.Substring(0, Ou.ToLower().IndexOf("ou="));
                        string Pathends = Ou.Substring(Ou.ToLower().IndexOf("ou="));

                        Array Myarray = Pathends.Split(',');

                        Array.Reverse(Myarray);
                        foreach (string pt in Myarray)
                        {
                            PathStart = PathStart + "," + pt;
                        }

                        Ou = PathStart.Replace(",,", ",");
                        if (Ou.StartsWith(","))
                            Ou = Ou.Substring(1);
                    }
                    catch
                    {
                        Ou = TheNewPathBackup;
                    }


                    if (!string.IsNullOrEmpty(Ou) && !string.IsNullOrEmpty(path))
                        path = string.Format("{0},{1}", Ou, path);
                }

                DirectoryEntry rootDSE = GetAdEntry(HostName, SecurePort, UserName, Password, Domain, path);

                DirectorySearcher dirSearch = new DirectorySearcher(rootDSE);

                dirSearch.PageSize = 1001;
                dirSearch.Filter = Query;
                switch (Scope)
                {
                    case "Base":
                        {
                            dirSearch.SearchScope = SearchScope.Base;
                            break;
                        }
                    case "OneLevel":
                        {
                            dirSearch.SearchScope = SearchScope.OneLevel;
                            break;
                        }
                    case "Subtree":
                        {
                            dirSearch.SearchScope = SearchScope.Subtree;
                            break;
                        }
                    default:
                        dirSearch.SearchScope = SearchScope.Subtree;
                        break;
                }

                if (Properties != null && Properties.Trim().Length > 0)
                    dirSearch.PropertiesToLoad.AddRange(Properties.Split(','));

                try
                {
                    allOUS = dirSearch.FindAll();
                }
                catch (DirectoryServicesCOMException exCom)
                {
                    throw exCom;
                }
                catch (COMException exConnect)
                {
                    throw exConnect;
                }


                foreach (SearchResult oneResult in allOUS)
                {
                    DataRow dr = dt.NewRow();

                    foreach (string sname in oneResult.Properties.PropertyNames)
                    {
                        if (!dt.Columns.Contains(sname))
                        {
                            dt.Columns.Add(sname);
                        }
                        if (arr.Contains(sname))
                        {
                            if ((Int64)oneResult.Properties[sname][0] > 922337203685477580)
                                dr[sname] = "";
                            else
                                dr[sname] = DateTime.FromFileTime((Int64)oneResult.Properties[sname][0]).ToString();
                        }
                        else
                        {
                            dr[sname] = oneResult.Properties[sname][0].ToString();
                        }
                    }
                    dt.Rows.Add(dr);
                }

                rootDSE.Dispose();


                return this.GenerateActivityResult(dt);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public static DirectoryEntry GetAdEntry(string domainServer, string domainPort, string username, string password, string domainName, string domainPath)
        {

            if (domainPort.Equals(DefaultAdSecurePort) && IsIpAddress(domainServer)) throw new Exception("When using a secure port, a server domain name must be defined for the device.");



            string domainUrl = "LDAP://" + domainServer;
            if (!domainPort.Equals(DefaultAdPort))
            {
                domainUrl = domainUrl + ":" + domainPort;
            }

            if (!string.IsNullOrEmpty(domainPath))
            {
                domainUrl = domainUrl + "/" + domainPath;
            }

            var adEntry = new DirectoryEntry(domainUrl, username, password, AuthenticationTypes.Secure);
            return adEntry;
        }

        private static bool IsIpAddress(string domainServer)
        {
            IPAddress address;
            return IPAddress.TryParse(domainServer, out address);
        }

        private string GetDomainPath(string domain)
        {
            string domainPath = string.Empty;

            try
            {
                string[] myarray = domain.Split('.');
                foreach (string mystring in myarray)
                {
                    domainPath = domainPath + "DC=" + mystring + ",";
                }


                domainPath = domainPath.Substring(0, domainPath.Length - 1);
            }
            catch (Exception e)
            {
                domainPath = string.Empty;
            }

            return domainPath;
        }
    }
}
