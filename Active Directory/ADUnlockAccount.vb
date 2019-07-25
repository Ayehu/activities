Imports Ayehu.Sdk.ActivityCreation.Interfaces
Imports Ayehu.Sdk.ActivityCreation.Extension
Imports System.Text
Imports System.DirectoryServices
Imports System.IO
Imports System.Net
Imports System
Imports System.Data
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
                ds.Filter = "(&(objectClass=user) (sAMAccountName=" + ADUserName + "))"
                ds.SearchScope = SearchScope.Subtree
                Dim results As SearchResult = ds.FindOne()
                If results IsNot Nothing Then
                    Dim dey As DirectoryEntry = GetAdEntryByFullPath(results.Path, UserName, Password)
                    UnlockAccount(dey)
                    dt.Rows.Add("Success")
                    dey.Close()
                Else
                    Throw New Exception("User does not exist")
                End If
            Else
                UnlockAccount(de)
                dt.Rows.Add("Success")
            End If

            de.Close()

            Return Me.GenerateActivityResult(dt)
        End Function

        Sub UnlockAccount(ByVal de As DirectoryEntry)
            de.Properties("LockOutTime").Value = 0
            de.CommitChanges()
        End Sub

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

