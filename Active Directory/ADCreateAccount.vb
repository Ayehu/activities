Imports Ayehu.Sdk.ActivityCreation.Interfaces
Imports Ayehu.Sdk.ActivityCreation.Extension
Imports System.Text
Imports System
Imports System.Xml
Imports System.Data
Imports System.IO
Imports System.Management
Imports System.Collections.Generic
Imports Microsoft.VisualBasic
Imports System.DirectoryServices
Imports System.Text.RegularExpressions
Imports System.Net

Namespace Ayehu.Sdk.ActivityCreation
  Public Class ActivityClass
      Implements IActivity


      Private Const DefaultAdPort As String = "389"
      Public HostName As String
      Public Path As String
      Public UserName As String
      Public Password As String
      Public FirstName As String
      Public Initials As String
      Public LastName As String
      Public LoginName As String
      Public LoginPassword As String
      Public Email As String
      Public FullName As String
      Public Chkpasswordnextlogin As String
      Public Chkpasswordexpires As String
      Public SecurePort As String

      Public Function Execute() As ICustomActivityResult Implements IActivity.Execute

          
          Dim dt As DataTable = New DataTable("resultSet")
          dt.Columns.Add("Result", GetType(String))
          
          If String.IsNullOrEmpty(SecurePort) = True Then
          SecurePort = DefaultAdPort
          End If
          
          If IsNumeric(SecurePort) = False Then
          Dim msg As String = "Port parameter must be number"
          Throw New ApplicationException(msg)
          End If
          
          
          If (Not String.IsNullOrEmpty(Email)) Then
          Dim myMatch As Match = System.Text.RegularExpressions.Regex.Match(Email, "^(([^<>()[\]\\.,;:\s@\""]+(\.[^<>()[\]\\.,;:\s@\""]+)*)|(\"".+\""))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3} \.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z-]{2,}))$")
          If Not myMatch.Success Then
          Throw New Exception("Invalid email address")
          End If
          End If
          If LCase(Path).Contains("cn=") OrElse LCase(Path).Contains("ou=") Then
          Else
          Path = Path.Replace("/", "\")
          Path = Trim(Path)
          If Path.Contains("\") Then
          If Path.StartsWith("\") Then Path = "ou=" & Path.Substring(1)
          Path = Path.Replace("\", ",ou=")
          End If
          If Path.StartsWith("ou=") = False Then Path = "ou=" & Path
          
          
          If LCase(Path).StartsWith("ou=users") Or LCase(Path).StartsWith("ou=builtin") Or LCase(Path).StartsWith("ou=microsoft exchange system objects") Or LCase(Path).StartsWith("ou=system") Or LCase(Path).StartsWith("ou=program data") Or LCase(Path).StartsWith("ou=managed service accounts") Or LCase(Path).StartsWith("ou=lostandfound") Or LCase(Path).StartsWith("ou=computers") Or LCase(Path).StartsWith("ou=foreignsecurityprincipals") Or LCase(Path).StartsWith("ou=ntds quotas") Then Path = "cn=" & Path.Substring(3)
          
          Dim TheNewPathBackup = Path
          Try
          Dim PathStart As String = Path.Substring(0, LCase(Path).IndexOf("ou="))
          Dim Pathends As String = Path.Substring(LCase(Path).IndexOf("ou="))
          Dim Myarray As Array = Split(Pathends, ",")
          Array.Reverse(Myarray)
          For Each pt As String In Myarray
          PathStart = PathStart & "," & pt
          Next
          Path = PathStart.Replace(",,", ",")
          If Path.StartsWith(",") Then Path = Path.Substring(1)
          Catch
          Path = TheNewPathBackup
          End Try
          End If
          
          Dim de As DirectoryEntry = GetAdEntry(HostName, SecurePort, UserName, Password)
          
          Dim ds As DirectorySearcher = New DirectorySearcher(de)
          ds.Filter = "(&(objectClass=user) (sAMAccountName=" + LoginName + "))"
          ds.SearchScope = SearchScope.Subtree
          Dim results As SearchResult = ds.FindOne()
          If results Is Nothing Then
          
          'Get Some default user in order to get complete path sample
          Dim dq As DirectorySearcher = New DirectorySearcher(de)
          dq.Filter = "(&(objectCategory=person)(objectClass=user))"
          dq.SearchScope = SearchScope.Subtree
          Dim TempUser As SearchResult = dq.FindOne()
          
          Dim entry As DirectoryEntry
          
          Dim dcData As String = TempUser.Path.Substring(TempUser.Path.IndexOf("DC="))
          
          If LCase(Path) = "ou=" Then
          entry = GetAdEntryByFullPath(de.Path & "/" & dcData, UserName, Password)
          Else
          entry = GetAdEntryByFullPath(de.Path & "/" & Path & "," & dcData, UserName, Password)
          End If
          
          Dim newuser As DirectoryEntry = Nothing
          Try
          newuser = entry.Children.Add("CN=" + FullName, "user")
          Catch
          Throw New Exception("The login name is not valid")
          End Try
          
          newuser.Properties("samAccountName").Value = LoginName
          newuser.Properties("givenname").Value = FirstName
          
          If dcData IsNot Nothing Then
          Dim difaultDomainName As String = GetDomainName(dcData)
          newuser.Properties("userPrincipalName").Value = LoginName & "@" & difaultDomainName
          Else
          newuser.Properties("userPrincipalName").Value = LoginName
          End If
          
          If Not String.IsNullOrEmpty(LastName) Then newuser.Properties("SN").Value = LastName
          If Not String.IsNullOrEmpty(Initials) Then newuser.Properties("Initials").Value = Initials
          If Not String.IsNullOrEmpty(FullName) Then newuser.Properties("displayName").Value = FullName
          If Not String.IsNullOrEmpty(Email) Then newuser.Properties("mail").Value = Email
          newuser.CommitChanges()
          newuser.RefreshCache()
          
          Try
          newuser.Invoke("SetPassword", New Object() {LoginPassword})
          newuser.CommitChanges()
          Catch Ex As Exception
          entry.Children.Remove(newuser)
          Throw Ex.InnerException
          End Try
          
          Dim value As Integer = Val(de.Properties("userAccountControl").Value)
          newuser.Properties("userAccountControl").Value = value And Not &H2
          newuser.CommitChanges()
          newuser.RefreshCache()
          
          If Chkpasswordnextlogin = "True" Then
          newuser.Properties("pwdLastSet").Value = 0
          Else
          newuser.Properties("pwdLastSet").Value = -1
          End If
          newuser.CommitChanges()
          newuser.RefreshCache()
          
          If Chkpasswordexpires = "True" Then 'Password expired
          Dim value3 As Integer = Val(de.Properties("userAccountControl").Value)
          newuser.Properties("userAccountControl").Value = value3 Or &H10000
          newuser.CommitChanges()
          newuser.RefreshCache()
          Else
          Dim value3 As Integer = Val(de.Properties("userAccountControl").Value)
          newuser.Properties("userAccountControl").Value = value3 And Not &H10000
          newuser.CommitChanges()
          newuser.RefreshCache()
          End If
          
          newuser.Close()
          dt.Rows.Add("Success")
          Else
          Throw New Exception("User already exists")
          End If
          
          de.Close()
          
 Return Me.GenerateActivityResult(dt)
          End Function
          
          Function GetDomainName(ByVal DistinguishedName As String) As String
          Dim sb As StringBuilder = New StringBuilder()
          Dim dcs As String() = DistinguishedName.Split(",")
          For Each dc As String In dcs
          
          Dim eqSignIndex As Integer = dc.IndexOf("=")
          sb.Append(dc.Substring(eqSignIndex + 1)).Append(".")
          Next
          Dim domainStr As String = sb.ToString()
          domainStr = domainStr.Remove(domainStr.LastIndexOf("."))
          Return domainStr
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

