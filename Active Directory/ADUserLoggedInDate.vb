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

Namespace Ayehu.Sdk.ActivityCreation
  Public Class ActivityClass
      Implements IActivity


      Private Const DEFAULT_AD_PORT As String = "389"
      Public HostName As String
      Public UserName As String
      Public Password As String
      Public ADUserName As String
      Public SecurePort As String

      Public Function Execute() As ICustomActivityResult Implements IActivity.Execute

          
          Dim dt As DataTable = New DataTable("resultSet")
          dt.Columns.Add("Result", GetType(String))
          
          If String.IsNullOrEmpty(SecurePort) = True Then
          SecurePort = DEFAULT_AD_PORT
          End If
          
          If IsNumeric(SecurePort) = False Then
          Dim msg As String = "Port parameter must be number"
          Throw New ApplicationException(msg)
          End If
          
          Dim de As DirectoryEntry = GetConnection(HostName, SecurePort, UserName, Password)
          
          If Trim(ADUserName) <> "" Then
          
          Dim ds As DirectorySearcher = New DirectorySearcher(de)
          ds.Filter = "(&(objectClass=user) (sAMAccountName=" + ADUserName + "))"
          ds.SearchScope = SearchScope.Subtree
          Dim results As SearchResult = ds.FindOne()
          If results IsNot Nothing Then
          Dim dey As DirectoryEntry = GetConnection2(results.Path, UserName, Password)
          Dim ticks As Long = GetInt64(dey, "lastlogon")
          Dim LogonDate As Date
          Try
          LogonDate = DateTime.FromFileTime(ticks)
          Catch ex As Exception
          LogonDate = DateTime.FromFileTime(0)
          End Try
          
          Dim TimeStampDate As Date
          ticks = GetInt64(dey, "lastLogonTimeStamp")
          Try
          TimeStampDate = DateTime.FromFileTime(ticks)
          Catch
          TimeStampDate = DateTime.FromFileTime(0)
          End Try
          If TimeStampDate > LogonDate Then
          LogonDate = TimeStampDate
          End If
          
          If LogonDate < DateAdd(DateInterval.Year, -10, DateTime.Now) Then
          dt.Rows.Add("User never logged in")
          Else
          dt.Rows.Add(LogonDate)
          End If
          dey.Close()
          Else
          Throw New Exception("User does not exist")
          End If
          Else
          Dim ticks As Long = GetInt64(de, "lastlogon")
          Dim LogonDate As Date
          Try
          LogonDate = DateTime.FromFileTime(ticks)
          Catch ex As Exception
          LogonDate = DateTime.FromFileTime(0)
          End Try
          
          Dim TimeStampDate As Date
          ticks = GetInt64(de, "lastLogonTimeStamp")
          Try
          TimeStampDate = DateTime.FromFileTime(ticks)
          Catch
          TimeStampDate = DateTime.FromFileTime(0)
          End Try
          If TimeStampDate > LogonDate Then
          LogonDate = TimeStampDate
          End If
          
          If LogonDate <= DateAdd(DateInterval.Year, -10, DateTime.Now) Then
          dt.Rows.Add("User never logged in")
          Else
          dt.Rows.Add(DateTime.FromFileTime(ticks))
          End If
          End If
          de.Close()
          
 Return Me.GenerateActivityResult(dt)
          End Function
          
          Function GetConnection(ByVal HostName As String, ByVal Port As String, ByVal UserName As String, ByVal Password As String) As DirectoryEntry
          Dim entry As DirectoryEntry = New DirectoryEntry("LDAP://" & HostName & ":" & Port, UserName, Password, AuthenticationTypes.Secure)
          Return entry
          End Function
          
          Function GetConnection2(ByVal Path As String, ByVal UserName As String, ByVal Password As String) As DirectoryEntry
          Dim entry As DirectoryEntry = New DirectoryEntry(Path)
          entry.Username = UserName
          entry.Password = Password
          entry.AuthenticationType = AuthenticationTypes.Secure
          Return entry
          End Function
          
          Function GetInt64(ByVal entry As DirectoryEntry, ByVal attr As String) As Int64
          Dim ds As DirectorySearcher = New DirectorySearcher(entry, String.Format("({0}=*)", attr), New String() {attr}, SearchScope.Base)
          Dim sr As SearchResult = ds.FindOne()
          If sr IsNot Nothing Then
          If sr.Properties.Contains(attr) Then Return Convert.ToInt64(sr.Properties(attr)(0))
          End If
          Return -1
          End Function

  End Class
End Namespace

