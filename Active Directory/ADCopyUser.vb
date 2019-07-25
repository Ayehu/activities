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
      Public ADUserName As String
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
      Public Properties As String
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
          
          Dim dr As DirectorySearcher = New DirectorySearcher(de)
          dr.Filter = "(&(objectClass=user) (sAMAccountName=" + ADUserName + "))"
          dr.SearchScope = SearchScope.Subtree
          Dim resultsq As SearchResult = dr.FindOne()
          If resultsq Is Nothing Then Throw New Exception("User does not exist")
          
          Dim ds As DirectorySearcher = New DirectorySearcher(de)
          ds.Filter = "(&(objectClass=user) (sAMAccountName=" + LoginName + "))"
          ds.SearchScope = SearchScope.Subtree
          Dim results As SearchResult = ds.FindOne()
          If results Is Nothing Then
          
          Dim entry As DirectoryEntry = GetAdEntryByFullPath(resultsq.Path, UserName, Password)
          Dim newuser As DirectoryEntry = entry.Parent.Children.Add("CN=" + LoginName, "user")
          newuser.Properties("samAccountName").Value = LoginName
          newuser.Properties("givenname").Value = FirstName
          newuser.Properties("userPrincipalName").Value = LoginName
          If Not String.IsNullOrEmpty(LastName) Then newuser.Properties("SN").Value = LastName
          If Not String.IsNullOrEmpty(Initials) Then newuser.Properties("Initials").Value = Initials
          If Not String.IsNullOrEmpty(FullName) Then newuser.Properties("displayName").Value = FullName
          If Not String.IsNullOrEmpty(Email) Then newuser.Properties("mail").Value = Email
          newuser.CommitChanges()
          newuser.Invoke("SetPassword", New Object() {LoginPassword})
          newuser.CommitChanges()
          
          Dim value As Integer = Val(de.Properties("userAccountControl").Value)
          newuser.Properties("userAccountControl").Value = value And Not &H2
          newuser.CommitChanges()
          
          If Chkpasswordnextlogin = "True" Then
          newuser.Properties("pwdLastSet").Value = 0
          Else
          newuser.Properties("pwdLastSet").Value = -1
          End If
          newuser.CommitChanges()
          
          
          If Chkpasswordexpires = "True" Then 'Password expired
          Dim value3 As Integer = Val(de.Properties("userAccountControl").Value)
          newuser.Properties("userAccountControl").Value = value3 Or &H10000
          newuser.CommitChanges()
          Else
          Dim value3 As Integer = Val(de.Properties("userAccountControl").Value)
          newuser.Properties("userAccountControl").Value = value3 And Not &H10000
          newuser.CommitChanges()
          End If
          
          If String.IsNullOrEmpty(Properties) = False Then
          Dim MyProperties As Array = Split(Properties, ",")
          For Each item As String In MyProperties
          If item = "Groups" Then
          Dim groups As Object = entry.Properties("MemberOf").Value
          If groups.GetType Is GetType(String) Then
          Dim TempName As String = groups.Substring(LCase(groups).IndexOf("cn=") + 3)
          TempName = TempName.Substring(0, TempName.IndexOf(","))
          Dim dsgroup As DirectorySearcher = New DirectorySearcher(de)
          dsgroup.Filter = "(&(objectClass=group) (sAMAccountName=" + TempName + "))"
          dsgroup.SearchScope = SearchScope.Subtree
          Dim resultsGroup As SearchResult = dsgroup.FindOne()
          If resultsGroup IsNot Nothing Then
          Dim degroup As DirectoryEntry = GetAdEntryByFullPath(resultsGroup.Path, UserName, Password)
          Dim TempPath As String = LCase(newuser.Path)
          degroup.Properties("member").Add(LCase(TempPath.Substring(TempPath.IndexOf("cn"))))
          degroup.CommitChanges()
          degroup.Close()
          End If
          Else
          For Each group As String In groups
          Dim TempName As String = group.Substring(LCase(group).IndexOf("cn=") + 3)
          TempName = TempName.Substring(0, TempName.IndexOf(","))
          Dim dsgroup As DirectorySearcher = New DirectorySearcher(de)
          dsgroup.Filter = "(&(objectClass=group) (sAMAccountName=" + TempName + "))"
          dsgroup.SearchScope = SearchScope.Subtree
          Dim resultsGroup As SearchResult = dsgroup.FindOne()
          If resultsGroup IsNot Nothing Then
          Dim degroup As DirectoryEntry = GetAdEntryByFullPath(resultsGroup.Path, UserName, Password)
          Dim TempPath As String = LCase(newuser.Path)
          degroup.Properties("member").Add(LCase(TempPath.Substring(TempPath.IndexOf("cn"))))
          degroup.CommitChanges()
          degroup.Close()
          End If
          Next
          End If
          Else
          SetProperty(newuser, item, GetPropertyValue(entry, item))
          End If
          Next
          End If
          newuser.CommitChanges()
          newuser.Close()
          dt.Rows.Add("Success")
          Else
          Throw New Exception("User already exists")
          End If
          
          de.Close()
          
 Return Me.GenerateActivityResult(dt)
          End Function
          Sub SetProperty(ByVal de As DirectoryEntry, ByVal PropertyName As String, ByVal PropertyValue As String)
          If PropertyValue IsNot Nothing Then
          If Trim(PropertyValue) = "" Then
          Dim Value As String = GetPropertyValue(de, PropertyName)
          If Value IsNot Nothing Then de.Properties(PropertyName).Remove(GetPropertyValue(de, PropertyName))
          
          ElseIf de.Properties.Contains(PropertyName) Then
          de.Properties(PropertyName)(0) = PropertyValue
          Else
          de.Properties(PropertyName).Add(PropertyValue)
          End If
          Else
          Dim Value As String = GetPropertyValue(de, PropertyName)
          If Value IsNot Nothing Then de.Properties(PropertyName).Remove(GetPropertyValue(de, PropertyName))
          
          End If
          End Sub
          
          Function GetPropertyValue(ByVal de As DirectoryEntry, ByVal PropertyName As String) As String
          Return de.Properties.Item(PropertyName).Value
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

