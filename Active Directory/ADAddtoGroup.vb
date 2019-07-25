Imports Ayehu.Sdk.ActivityCreation.Interfaces
Imports Ayehu.Sdk.ActivityCreation.Extension
Imports System.Text
Imports System
Imports System.Data
Imports System.IO
Imports Microsoft.VisualBasic
Imports System.DirectoryServices
Imports System.Net

Namespace Ayehu.Sdk.ActivityCreation
  Public Class ActivityClass
      Implements IActivity


      Private Const DefaultAdPort As String = "389"
      Public HostName As String
      Public UserName As String
      Public Password As String
      Public ADUserName As String
      Public ADGroupName As String
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
          
          
          Dim accntType As String = "user"
          If String.IsNullOrEmpty(AccountType) = False Then
          accntType = AccountType
          End If
          
          Dim userDirectoryEntry As DirectoryEntry
          Dim adminDirectoryEntry As DirectoryEntry
          Dim userFQDN = ADUserName.Split("\")
          
          
          If userFQDN.Length = 2 Then
          userDirectoryEntry = GetAdEntry(userFQDN(0),SecurePort,UserName,Password)
          adminDirectoryEntry = GetAdEntry(HostName,SecurePort,UserName,Password)
          Else
          userDirectoryEntry = GetAdEntry(HostName,SecurePort,UserName,Password)
          adminDirectoryEntry = userDirectoryEntry
          End If
          
          
          Dim userDirectorySearcher As DirectorySearcher = New DirectorySearcher(userDirectoryEntry)
          
          
          
          Select Case LCase(accntType)
          Case "user"
          userDirectorySearcher.Filter = "(&(objectClass=user)(!(objectclass=computer))(SamAccountName=" + ADUserName + "))"
          If (userFQDN.Length = 2) Then
          userDirectorySearcher.Filter = "(&(objectClass=user)(!(objectclass=computer))(SamAccountName=" + userFQDN(1) + "))"
          End If
          Case "computer"
          userDirectorySearcher.Filter = "(&(objectClass=computer) (sAMAccountname=" + ADUserName + "$))"
          If (userFQDN.Length = 2) Then
          userDirectorySearcher.Filter = "(&(objectClass=computer) (sAMAccountname=" + userFQDN(1) + "$))"
          End If
          End Select
          
          
          
          
          
          
          userDirectorySearcher.SearchScope = SearchScope.Subtree
          Dim results As SearchResult = userDirectorySearcher.FindOne()
          If results IsNot Nothing Then
          Dim groupDirectorySearcher As DirectorySearcher = New DirectorySearcher(adminDirectoryEntry)
          groupDirectorySearcher.Filter = "(&(objectClass=group) (sAMAccountName=" + ADGroupName + "))"
          groupDirectorySearcher.SearchScope = SearchScope.Subtree
          Dim resultsGroup As SearchResult = groupDirectorySearcher.FindOne()
          If resultsGroup IsNot Nothing Then
          Dim degroup As DirectoryEntry = GetAdEntryByFullPath(resultsGroup.Path, UserName, Password)
          Dim TempPath As String = LCase(results.Path)
          degroup.Properties("member").Add(LCase(TempPath.Substring(TempPath.IndexOf("cn"))))
          degroup.CommitChanges()
          degroup.Close()
          Else
          Throw New Exception("Group does not exist")
          End If
          Else
          Select Case LCase(accntType)
          Case "user"
          Throw New Exception("User does not exist")
          Case "computer"
          Throw New Exception("Computer does not exist")
          End Select
          End If
          dt.Rows.Add("Success")
          userDirectoryEntry.Close()
          adminDirectoryEntry.Close()
          
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

