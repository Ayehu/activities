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
        Dim CacheList As New List(Of String)
        Public HostName As String
        Public UserName As String
        Public Password As String
        Public GroupName As String
        Public Recursive As String
        Public SecurePort As String

        Public Function Execute() As ICustomActivityResult Implements IActivity.Execute


            Dim dt As DataTable = New DataTable("resultSet")
            dt.Columns.Add("Type", GetType(String))
            dt.Columns.Add("Name", GetType(String))
            dt.Columns.Add("OU", GetType(String))
            dt.Columns.Add("Logon Name", GetType(String))

            If String.IsNullOrEmpty(SecurePort) = True Then
                SecurePort = DefaultAdPort
            End If

            If IsNumeric(SecurePort) = False Then
                Dim msg As String = "Port parameter must be number"
                Throw New ApplicationException(msg)
            End If

            Dim de As DirectoryEntry = GetAdEntry(HostName, SecurePort, UserName, Password)

            Dim Grouplist As New List(Of String)

            Dim resultTBL As DataTable
            If String.IsNullOrEmpty(GroupName) Then Throw New Exception("There is no such object on the server")
            If GroupName.StartsWith("<NewDataSet>") Then ' ResultSet
                Dim sr As StringReader = New StringReader(GroupName)
                resultTBL = New DataTable()
                resultTBL.ReadXml(sr)
                For Each row As DataRow In resultTBL.Rows
                    If String.IsNullOrEmpty(row(0)) = False Then Grouplist.Add(row(0))
                Next
            Else
                Grouplist.Add(GroupName)
            End If


            Dim ds As DirectorySearcher = New DirectorySearcher(de)
            For Each item As String In Grouplist
                ds.Filter = "(&(objectClass=group) (sAMAccountName=" + item + "))"
                ds.SearchScope = SearchScope.Subtree
                Dim results As SearchResult = ds.FindOne()
                If results IsNot Nothing Then
                    GetGroupRecursive(de, ds, dt, UserName, Password, Recursive, SecurePort)

                Else
                    Throw New Exception("Group does not exist - " & item)
                End If
                de.Close()
            Next
            Return Me.GenerateActivityResult(dt)
        End Function

        Function GetGroupRecursive(ByVal de As DirectoryEntry, ByVal Ds As DirectorySearcher, ByRef dt As DataTable, ByVal Username As String, ByVal Password As String, ByVal Recursive As String, SecurePort As String)
            Dim Members As Object = Ds.FindOne.GetDirectoryEntry.Invoke("Members", Nothing)

            For Each Member As Object In Members
                Dim CurrentMember As New DirectoryEntry(Member)

                If CacheList.Contains(CurrentMember.Path) Then Continue For
                CacheList.Add(CurrentMember.Path)

                Select Case LCase(CurrentMember.SchemaClassName)
                    Case "organizationalunit"
                        dt.Rows.Add("OU", CurrentMember.Name.Replace("OU=", ""), CurrentMember.Path.Substring(CurrentMember.Path.IndexOf("OU")))
                    Case "group"
                        dt.Rows.Add(UpperFirstLetter(CurrentMember.SchemaClassName), CurrentMember.Name.Replace("CN=", ""), CurrentMember.Path.Substring(CurrentMember.Path.IndexOf("CN")))
                        If Recursive = "True" Then
                            Dim Tempde As DirectoryEntry = GetAdEntryByFullPath(CurrentMember.Path, Username, Password)
                            Dim dsTemp As DirectorySearcher = New DirectorySearcher(Tempde)
                            Ds.Filter = "(&(objectClass=group) (" + CurrentMember.Name + "))"
                            dsTemp.SearchScope = SearchScope.Subtree
                            Dim results As SearchResult = dsTemp.FindOne()
                            If results IsNot Nothing Then
                                GetGroupRecursive(Tempde, dsTemp, dt, Username, Password, Recursive, SecurePort)
                                Tempde.Close()
                            End If
                        End If
                    Case Else
                        Dim LogonName As String
                        Try
                            LogonName = CurrentMember.Properties("userPrincipalName").Value.ToString
                        Catch ex As Exception
                        End Try
                        Try
                            dt.Rows.Add(UpperFirstLetter(CurrentMember.SchemaClassName), CurrentMember.Name.Replace("CN=", ""), CurrentMember.Path.Substring(CurrentMember.Path.IndexOf("CN")), LogonName)
                        Catch
                            dt.Rows.Add(UpperFirstLetter(CurrentMember.SchemaClassName), CurrentMember.Name.Replace("CN=", ""), CurrentMember.Path, LogonName)
                        End Try
                End Select
            Next
        End Function

        Function UpperFirstLetter(ByVal Name As String) As String
            Return Name.Substring(0, 1).ToUpper & Name.Substring(1, Len(Name) - 1).ToLower
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

