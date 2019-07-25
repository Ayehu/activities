Imports Ayehu.Sdk.ActivityCreation.Interfaces
Imports Ayehu.Sdk.ActivityCreation.Extension
Imports System.Text
Imports System.DirectoryServices
Imports System.IO
Imports System.Xml
Imports System.Net
Imports System
Imports System.Data
Imports Microsoft.VisualBasic
Imports System.Collections.Generic

Namespace Ayehu.Sdk.ActivityCreation
  Public Class ActivityClass
      Implements IActivity


      Private Const DefaultAdPort As String = "389"
      Public HostName As String
      Public UserName As String
      Public Password As String
      Public ADResult As String
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
          
          If String.IsNullOrEmpty(ADResult) Then Throw New Exception("No properties definition")
          Dim plist As Dictionary(Of String, String) = ConvertXMLStringToDic(ADResult)
          Dim ds As DirectorySearcher = New DirectorySearcher(de)
          
          Dim QString As String = "(&(objectClass=user)(!(objectclass=computer))"
          For Each item As KeyValuePair(Of String, String) In plist
          If String.IsNullOrEmpty(item.Value) = False Then QString = QString & "(" & item.Key & "=" & item.Value & ")"
          Next
          QString = QString & ")"
          
          ds.Filter = QString
          
          ds.SearchScope = SearchScope.Subtree
          Dim results As SearchResultCollection = ds.FindAll
          If results IsNot Nothing Then
          If results.Count = 0 Then Throw New Exception("User does not exist")
          For Each item As SearchResult In results
          Dim dey As DirectoryEntry = GetAdEntryByFullPath(item.Path, UserName, Password)
          dt.Rows.Add(GetPropertyValue(dey, "sAMAccountname"))
          Next
          Else
          Throw New Exception("User does not exist")
          End If
          
          de.Close()
          
          Dim sorttable As New DataView(dt)
          sorttable.Sort() = "Result"
          dt = sorttable.ToTable
          
 Return Me.GenerateActivityResult(dt)
          End Function
          
          Function GetPropertyValue(ByVal de As DirectoryEntry, ByVal PropertyName As String) As String
          
          Return de.Properties.Item(PropertyName).Value
          End Function
          
          Public Function ConvertXMLStringToDic(ByVal XMLString As String) As Dictionary(Of String, String)
          Dim TempDic As New Dictionary(Of String, String)
          Dim xmlDoc As New XmlDocument
          Try
          xmlDoc.LoadXml(XMLString)
          Catch
          Throw New Exception("No properties definition")
          End Try
          
          Dim DocRoot As XmlNode = xmlDoc.GetElementsByTagName("Settings")(0)
          
          For Each ChileNode As Object In DocRoot.ChildNodes
          For Each Element As XmlAttribute In ChileNode.Attributes
          TempDic(Element.Name) = Element.InnerText
          Next
          Next
          Return TempDic
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

