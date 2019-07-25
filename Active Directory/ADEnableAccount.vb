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
      Public UserName As String
      Public Password As String
      Public ADUserName As String
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
          
          Dim de As DirectoryEntry = GetAdEntry(HostName, SecurePort, UserName, Password)
          
          
          If Trim(ADUserName) <> "" Then
          
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
          Dim dey As DirectoryEntry = GetAdEntryByFullPath(results.Path, UserName, Password)
          SetProperty(dey)
          dey.CommitChanges()
          dey.Close()
          dt.Rows.Add("Success")
          Else
          Select Case LCase(AccountType)
          Case "user"
          Throw New Exception("User does not exist")
          Case "computer"
          Throw New Exception("Computer does not exist")
          End Select
          End If
          Else
          SetProperty(de)
          dt.Rows.Add("Success")
          End If
          
          de.Close()
          
 Return Me.GenerateActivityResult(dt)
          End Function
          
          Sub SetProperty(ByVal de As DirectoryEntry)
          Dim value As Integer = Val(de.Properties("userAccountControl").Value)
          de.Properties("userAccountControl").Value = value And Not &H2
          de.CommitChanges()
          End Sub
          
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
          
          Public  Function GetAdEntryByFullPath(ByVal path As String, ByVal userName As String, ByVal password As String) As DirectoryEntry
          Dim adEntry = New DirectoryEntry(path, userName, password, AuthenticationTypes.Secure)
          Return adEntry
          End Function
          
          

  End Class
End Namespace

