Imports Ayehu.Sdk.ActivityCreation.Interfaces
Imports Ayehu.Sdk.ActivityCreation.Extension
Imports System.Text
Imports System
Imports System.Xml
Imports System.Data
Imports System.IO
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
      Public PropertyName As String
      Public AccountType As String
      Public SecurePort As String

      Public Function Execute() As ICustomActivityResult Implements IActivity.Execute

          If String.IsNullOrEmpty(ADUserName) Then Throw New Exception("There is no such object on the server")
          If String.IsNullOrEmpty(PropertyName) Then Throw New Exception("There is no such object on the server")
          
          If String.IsNullOrEmpty(SecurePort) = True Then
          SecurePort = DefaultAdPort
          End If
          
          If IsNumeric(SecurePort) = False Then
          Dim msg As String = "Port parameter must be number"
          Throw New ApplicationException(msg)
          End If
          
          Dim dt As DataTable = New DataTable("resultSet")
          Dim de As DirectoryEntry = GetAdEntry(HostName, SecurePort, UserName, Password)
          Dim ProperyArray As Array = Nothing
          Dim ProperyArrayConList As New Dictionary(Of String, String)
          
          If PropertyName.Contains(",") Then
          ProperyArray = PropertyName.Split(",")
          For Each item As String In ProperyArray
          
          If String.Equals(AccountType, "User") Then
          
          Select Case Trim(LCase(item))
          Case "email"
          ProperyArrayConList("Email") = "mail"
          Case "city"
          ProperyArrayConList("City") = "l"
          Case "common name"
          ProperyArrayConList("Common Name") = "CN"
          Case "company"
          ProperyArrayConList("Company") = "company"
          Case "country"
          ProperyArrayConList("Country") = "c"
          Case "Department"
          ProperyArrayConList("Department") = "department"
          Case "description"
          ProperyArrayConList("Description") = "description"
          Case "display name"
          ProperyArrayConList("Display Name") = "displayName"
          Case "division"
          ProperyArrayConList("Division") = "division"
          Case "employee id"
          ProperyArrayConList("Employee ID") = "employeeID"
          Case "employee number"
          ProperyArrayConList("Employee Number") = "employeeNumber"
          Case "employee type"
          ProperyArrayConList("Employee Type") = "employeeType"
          Case "fax"
          ProperyArrayConList("Fax") = "facsimileTelephoneNumber"
          Case "first name"
          ProperyArrayConList("First Name") = "givenName"
          Case "home drive"
          ProperyArrayConList("Home Drive") = "HomeDrive"
          Case "home folder"
          ProperyArrayConList("Home Folder") = "HomeDirectory"
          Case "home phone"
          ProperyArrayConList("Home Phone") = "homePhone"
          Case "initials"
          ProperyArrayConList("Initials") = "initials"
          Case "ip phone"
          ProperyArrayConList("IP Phone") = "ipPhone"
          Case "last name"
          ProperyArrayConList("Last Name") = "sn"
          Case "login script"
          ProperyArrayConList("Login Script") = "scriptPath"
          Case "manager"
          ProperyArrayConList("Manager") = "manager"
          Case "middle name"
          ProperyArrayConList("Middle Name") = "middleName"
          Case "mobile"
          ProperyArrayConList("Mobile") = "mobile"
          Case "Notes"
          ProperyArrayConList("Notes") = "info"
          Case "office"
          ProperyArrayConList("Office") = "physicalDeliveryOfficeName"
          Case "pager"
          ProperyArrayConList("Pager") = "pager"
          Case "po box"
          ProperyArrayConList("PO Box") = "postOfficeBox"
          Case "profile path"
          ProperyArrayConList("Profile Path") = "profilePath"
          Case "room number"
          ProperyArrayConList("Room Number") = "roomNumber"
          Case "dtate/province"
          ProperyArrayConList("State/Province") = "st"
          Case "street"
          ProperyArrayConList("Street") = "streetAddress"
          Case "telephone number"
          ProperyArrayConList("Telephone Number") = "telephoneNumber"
          Case "title"
          ProperyArrayConList("Title") = "title"
          Case "web page"
          ProperyArrayConList("Web Page") = "wWWHomePage"
          Case "zip/postal code"
          ProperyArrayConList("Zip/Postal Code") = "postalCode"
          Case Else
          ProperyArrayConList(item) = item
          End Select
          Else
          ProperyArrayConList(Trim(LCase(item))) = Trim(LCase(item))
          End If
          Next
          End If
          
          
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
          If ProperyArrayConList.Count > 0 Then
          For Each item As String In ProperyArrayConList.Keys
          dt.Columns.Add(item, GetType(String))
          Next
          Dim NewRow As DataRow = dt.NewRow
          For Each item As KeyValuePair(Of String, String) In ProperyArrayConList
          Try
          NewRow(item.Key) = GetPropertyValue(dey, item.Value)
          Catch
          End Try
          Next
          dt.Rows.Add(NewRow)
          Else
          dt.Columns.Add("Result", GetType(String))
          Dim Result As String = GetPropertyValue(dey, PropertyName)
          If Result Is Nothing Then Result = ""
          dt.Rows.Add(Result)
          dey.Close()
          End If
          Else
          Select Case LCase(AccountType)
          Case "user"
          Throw New Exception("User does not exist")
          Case "computer"
          Throw New Exception("Computer does not exist")
          End Select
          End If
          Else
          Dim Result As String = GetPropertyValue(de, PropertyName)
          If Result Is Nothing Then Result = ""
          dt.Rows.Add(Result)
          End If
          de.Close()
          
 Return Me.GenerateActivityResult(dt)
          End Function
          Function GetPropertyValue(ByVal de As DirectoryEntry, ByVal PropertyName As String) As String
          Dim result11 As String = String.Empty
          If de.Properties.Item(PropertyName).Value IsNot Nothing Then
          Dim valueType As Type = de.Properties.Item(PropertyName).Value.GetType()
          Select Case LCase(valueType.Name)
          Case "object[]"
          Dim props As Object() = de.Properties.Item(PropertyName).Value
          For Each item As String In props
          result11 = result11 & item.ToString() & ","
          Next
          Dim lindex As Integer = result11.LastIndexOf(",")
          result11.Remove(lindex)
          Case Else
          result11 = de.Properties.Item(PropertyName).Value.ToString()
          End Select
          End If
          Return result11
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

