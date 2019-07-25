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
Imports System.Net

Namespace Ayehu.Sdk.ActivityCreation
  Public Class ActivityClass
      Implements IActivity


      Private Const DefaultAdPort As String = "389"
      Public HostName As String
      Public ADUserName As String
      Public UserName As String
      Public Password As String
      Public Path As String
      Public AccountType As String
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
          Select Case LCase(AccountType)
          Case "user"
          ds.Filter = "(&(objectClass=user)(!(objectclass=computer))(sAMAccountname=" + ADUserName + "))"
          Case "computer"
          ds.Filter = "(&(objectClass=computer) (sAMAccountname=" + ADUserName + "$))"
          End Select
          ds.SearchScope = SearchScope.Subtree
          Dim results As SearchResult = ds.FindOne()
          If results IsNot Nothing Then
          Dim target As DirectoryEntry
          If Path = "" OrElse Path = "\" OrElse LCase(Path) = "ou=" Then
          target = GetAdEntryByFullPath(de.Path & "/" & results.Path.Substring(results.Path.IndexOf("DC=")), UserName, Password)
          Else
          target = GetAdEntryByFullPath(de.Path & "/" & Path & "," & results.Path.Substring(results.Path.IndexOf("DC=")), UserName, Password)
          End If
          Dim source As DirectoryEntry = GetAdEntryByFullPath(results.Path, UserName, Password)
          source.MoveTo(target)
          dt.Rows.Add("Success")
          Else
          Select Case LCase(AccountType)
          Case "user"
          Throw New Exception("User does not exist")
          Case "computer"
          Throw New Exception("Computer does not exist")
          End Select
          End If
          
          de.Close()
          
 Return Me.GenerateActivityResult(dt)
          
          
          
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

