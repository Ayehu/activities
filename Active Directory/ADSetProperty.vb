Imports Ayehu.Sdk.ActivityCreation.Interfaces
Imports Ayehu.Sdk.ActivityCreation.Extension
Imports System.Text
Imports System.DirectoryServices
Imports System.IO
Imports System.Net
Imports Newtonsoft.Json.Linq
Imports System
Imports System.Data
Imports Microsoft.VisualBasic
Imports Newtonsoft.Json

Namespace Ayehu.Sdk.ActivityCreation
    Public Class ActivityClass
        Implements IActivity


        Private Const DefaultAdPort As String = "389"
        Public HostName As String
        Public UserName As String
        Public Password As String
        Public ADUserName As String
        Public FieldsList As String
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

            ' Escaping of potential backslashes in JSon
            Dim pattern = "(?<!\\)\\(?!\\)" ' Escape only single (but not double) backslashes
            Dim regex = New Regex(pattern)
            Dim escapedFieldsListJson = regex.Replace(FieldsList, "\\")

            If Trim(ADUserName) <> "" Then
                Dim ds As DirectorySearcher = New DirectorySearcher(de)


                Select Case LCase(AccountType)
                    Case "user"
                        ds.Filter = "(&(objectClass=user) (sAMAccountname=" + ADUserName + "))"
                    Case "computer"
                        ds.Filter = "(&(objectClass=computer) (sAMAccountname=" + ADUserName + "$))"
                End Select

                ds.SearchScope = SearchScope.Subtree
                Dim results As SearchResult = ds.FindOne()
                If results IsNot Nothing Then
                    Dim dey As DirectoryEntry = GetAdEntryByFullPath(results.Path, UserName, Password)
                    Dim json As JArray = JArray.Parse(escapedFieldsListJson)
                    For Each item As JToken In json.Children()
                        Dim key As String = item.Item("id")
                        Dim value As String = item.Item("value")

                        If (LCase(key) = "cn") Then
                            RenameProperty(dey, key, value)
                        Else
                            SetProperty(dey, key, value)
                        End If
                    Next
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
                Dim json As JArray = JArray.Parse(escapedFieldsListJson)
                For Each item As JToken In json.Children()
                    Dim key As String = item.Item("id")
                    Dim value As String = item.Item("value")

                    If (LCase(key) = "cn") Then
                        RenameProperty(de, key, value)
                    Else
                        SetProperty(de, key, value)
                    End If
                Next
                de.CommitChanges()
                dt.Rows.Add("Success")
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

        Sub RenameProperty(ByVal de As DirectoryEntry, ByVal PropertyName As String, ByVal PropertyValue As String)
            If (PropertyValue Is Nothing) Then
                Dim Value As String = GetPropertyValue(de, PropertyName)
                If Value IsNot Nothing Then de.Properties(PropertyName).Remove(GetPropertyValue(de, PropertyName))
            Else
                de.Rename(PropertyName & "=" & PropertyValue)
            End If
        End Sub

        Function GetPropertyValue(ByVal de As DirectoryEntry, ByVal PropertyName As String) As String
            Return de.Properties.Item(PropertyName).Value
        End Function

        Public Function GetAdEntryByFullPath(ByVal path As String, ByVal userName As String, ByVal password As String) As DirectoryEntry
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

