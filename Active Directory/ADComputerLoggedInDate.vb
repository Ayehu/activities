Imports Ayehu.Sdk.ActivityCreation.Interfaces
Imports Ayehu.Sdk.ActivityCreation.Extension
Imports System.Text
Imports System.DirectoryServices
Imports System.IO
Imports System
Imports System.Data
Imports System.Net
Imports Microsoft.VisualBasic

Namespace Ayehu.Sdk.ActivityCreation
  Public Class ActivityClass
      Implements IActivity


      Private Const DefaultAdPort As String = "389"
      Public HostName As String
      Public UserName As String
      Public Password As String
      Public ADUserName As String
      Public SecurePort As String

      Public Function Execute() As ICustomActivityResult Implements IActivity.Execute

          
          Dim dt = New DataTable("resultSet")
          dt.Columns.Add("Result", GetType(String))
          
          If String.IsNullOrEmpty(SecurePort) = True Then
          SecurePort = DefaultAdPort
          End If
          
          If IsNumeric(SecurePort) = False Then
          Dim msg As String = "Port parameter must be number"
          Throw New ApplicationException(msg)
          End If
          
          Dim de As DirectoryEntry = GetAdEntry(HostName, SecurePort, UserName, Password)
          
          If Trim(ADUserName) <> "" Then
          
          Dim ds As DirectorySearcher = New DirectorySearcher(de)
          ds.Filter = "(&(objectClass=computer) (sAMAccountName=" + ADUserName + "$))"
          ds.SearchScope = SearchScope.Subtree
          ds.ReferralChasing = ReferralChasingOption.All      ' https://docs.microsoft.com/en-us/dotnet/api/system.directoryservices.referralchasingoption?view=netframework-4.5
          
          Dim results As SearchResult = ds.FindOne()
          If results IsNot Nothing Then
          Dim dey As DirectoryEntry = GetAdEntryByFullPath(results.Path, UserName, Password)
          Dim ticks As Long = GetInt64(dey, "lastlogon")
          Dim LogonDate As Date = DateTime.FromFileTime(ticks)
          If LogonDate < DateAdd(DateInterval.Year, -10, DateTime.Now) Then
          dt.Rows.Add("Computer never logged in")
          Else
          dt.Rows.Add(DateTime.FromFileTime(ticks))
          End If
          dey.Close()
          Else
          Throw New Exception("Computer does not exist")
          End If
          Else
          Dim ticks As Long = GetInt64(de, "lastlogon")
          Dim LogonDate As Date = DateTime.FromFileTime(ticks)
          If LogonDate <= DateAdd(DateInterval.Year, -10, DateTime.Now) Then
          dt.Rows.Add("Computer never logged in")
          Else
          
          dt.Rows.Add(DateTime.FromFileTime(ticks))
          End If
          End If
          de.Close()
          
 Return Me.GenerateActivityResult(dt)
          End Function
          Function GetInt64(ByVal entry As DirectoryEntry, ByVal attr As String) As Int64
          Dim ds As DirectorySearcher = New DirectorySearcher(entry, String.Format("({0}=*)", attr), New String() {attr}, SearchScope.Base)
          Dim sr As SearchResult = ds.FindOne()
          If sr IsNot Nothing Then
          If sr.Properties.Contains(attr) Then Return Convert.ToInt64(sr.Properties(attr)(0))
          End If
          Return -1
          End Function
          
          Public  Function GetAdEntryByFullPath(ByVal path As String, ByVal userName As String, ByVal password As String) As DirectoryEntry
          Dim adEntry = New DirectoryEntry(path, userName, password, AuthenticationTypes.Secure)
          Return adEntry
          End Function
          
          Public Function GetAdEntry(ByVal domainServer As String, ByVal domainPort As String, ByVal username As String, ByVal password As String) As DirectoryEntry
          Dim defaultAdSecurePort As String = "636"
          If domainPort.Equals(defaultAdSecurePort) AndAlso IsIpAddress(domainServer) Then Throw New Exception("When using a secure port, a server domain name must be defined for the device.")
          Dim domainUrl As String = "LDAP://" & domainServer
          
          If Not domainPort.Equals(DefaultAdPort) Then
          domainUrl = domainUrl & ":" & domainPort
          End If
          
          Dim adEntry = New DirectoryEntry(domainUrl, username, password, AuthenticationTypes.Secure)
          Return adEntry
          End Function
          
          Private Function IsIpAddress(ByVal domainServer As String) As Boolean
          Dim address As IPAddress
          Return IPAddress.TryParse(domainServer, address)
          End Function

  End Class
End Namespace

