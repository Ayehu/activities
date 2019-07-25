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


        Dim CacheList As New List(Of String)
        Dim Filters As New List(Of String)
        Private Const DefaultAdPort As String = "389"
        Public HostName As String
        Public Path As String
        Public UserName As String
        Public Password As String
        Public Recursive As String
        Public Filter As String
        Public SecurePort As String

        Public Function Execute() As ICustomActivityResult Implements IActivity.Execute


            Dim dt As DataTable = New DataTable("resultSet")
            dt.Columns.Add("Type", GetType(String))
            dt.Columns.Add("Name", GetType(String))
            dt.Columns.Add("OU", GetType(String))

            If String.IsNullOrEmpty(SecurePort) = True Then
                SecurePort = DefaultAdPort
            End If

            If IsNumeric(SecurePort) = False Then
                Dim msg As String = "Port parameter must be number"
                Throw New ApplicationException(msg)
            End If

            If String.IsNullOrEmpty(Filter) = False Then
                If Filter.Contains(",") Then
                    Dim MyArray As Array = Filter.Split(",")
                    For Each item As String In MyArray
                        If LCase(Trim(item)) = "ou" Then
                            Filters.Add("organizationalunit")
                        Else
                            Filters.Add(Trim(LCase(item)))
                        End If

                    Next
                Else
                    If LCase(Trim(Filter)) = "ou" Then
                        Filters.Add(Trim("organizationalunit"))
                    Else
                        Filters.Add(Trim(LCase(Filter)))
                    End If
                End If

            End If
            Dim OUList As New List(Of String)
            Dim resultTBL As DataTable

            If String.IsNullOrEmpty(Path) Then Throw New Exception("There is no such object on the server")

            If Path.StartsWith("<NewDataSet>") Then ' ResultSet
                Dim sr As StringReader = New StringReader(Path)
                resultTBL = New DataTable()
                resultTBL.ReadXml(sr)
                For Each row As DataRow In resultTBL.Rows
                    If String.IsNullOrEmpty(row(0)) = False Then OUList.Add(row(0))
                Next
            Else
                OUList.Add(Path)
            End If

            For Each item As String In OUList

                If LCase(item) <> "all" Then
                    If LCase(item).Contains("cn=") OrElse LCase(item).Contains("ou=") Then
                    Else
                        item = item.Replace("/", "\")
                        item = Trim(item)
                        If item.Contains("\") Then
                            If item.StartsWith("\") Then item = "ou=" & item.Substring(1)
                            item = item.Replace("\", ",ou=")
                        End If
                        If item.StartsWith("ou=") = False Then item = "ou=" & item


                        If LCase(item).StartsWith("ou=users") Or LCase(item).StartsWith("ou=builtin") Or LCase(item).StartsWith("ou=microsoft exchange system objects") Or LCase(item).StartsWith("ou=system") Or LCase(item).StartsWith("ou=program data") Or LCase(item).StartsWith("ou=managed service accounts") Or LCase(item).StartsWith("ou=lostandfound") Or LCase(item).StartsWith("ou=computers") Or LCase(item).StartsWith("ou=foreignsecurityprincipals") Or LCase(item).StartsWith("ou=ntds quotas") Then item = "cn=" & item.Substring(3)

                        Dim TheNewitemBackup = item
                        Try
                            Dim itemStart As String = item.Substring(0, LCase(item).IndexOf("ou="))
                            Dim itemends As String = item.Substring(LCase(item).IndexOf("ou="))
                            Dim Myarray As Array = Split(itemends, ",")
                            Array.Reverse(Myarray)
                            For Each pt As String In Myarray
                                itemStart = itemStart & "," & pt
                            Next
                            item = itemStart.Replace(",,", ",")
                            If item.StartsWith(",") Then item = item.Substring(1)
                        Catch
                            item = TheNewitemBackup
                        End Try
                    End If
                End If

                Dim de As DirectoryEntry = GetAdEntry(HostName, SecurePort, UserName, Password)
                Dim dq As DirectorySearcher = New DirectorySearcher(de)
                dq.Filter = "(&(objectCategory=person)(objectClass=user))"
                dq.SearchScope = SearchScope.Subtree
                Dim TempUser As SearchResult = dq.FindOne()

                Dim entry As DirectoryEntry
                If item = "" OrElse LCase(item) = "all" OrElse LCase(item) = "ou=" OrElse item = "\" Then
                    entry = GetAdEntryByFullPath(de.Path & "/" & TempUser.Path.Substring(TempUser.Path.IndexOf("DC=")), UserName, Password)
                Else
                    entry = GetAdEntryByFullPath(de.Path & "/" & item & "," & TempUser.Path.Substring(TempUser.Path.IndexOf("DC=")), UserName, Password)
                End If



                Dim Fnd As Boolean = False
                GetOURecursive(entry, dt, UserName, Password, Recursive, Filter, SecurePort)

                de.Close()
            Next

            Return Me.GenerateActivityResult(dt)

        End Function

        Function GetOURecursive(ByVal de As DirectoryEntry, ByRef dt As DataTable, ByVal UserName As String, ByVal Password As String, ByVal Recursive As String, Filter As String, SecurePort As String)

            For Each child As DirectoryEntry In de.Children
                If CacheList.Contains(child.Path) Then Continue For
                CacheList.Add(child.Path)
                Select Case LCase(child.SchemaClassName)
                    Case "organizationalunit"
                        If Filters.Count = 0 Then
                            dt.Rows.Add("OU", child.Name.Replace("OU=", ""), child.Path.Substring(child.Path.IndexOf("OU")))
                        ElseIf Filters.Contains(LCase(child.SchemaClassName)) Then
                            dt.Rows.Add("OU", child.Name.Replace("OU=", ""), child.Path.Substring(child.Path.IndexOf("OU")))
                        End If
                        If Recursive = "True" Then
                            Dim Tempde As DirectoryEntry = GetAdEntryByFullPath(child.Path, UserName, Password)
                            GetOURecursive(Tempde, dt, UserName, Password, Recursive, Filter, SecurePort)
                            Tempde.Close()
                        End If
                    Case Else
                        If Filters.Count = 0 Then
                            Try
                                If LCase(child.SchemaClassName) = "computer" Then
                                    dt.Rows.Add(UpperFirstLetter(child.SchemaClassName), child.Properties("sAMAccountName").Value.ToString.Replace("$", ""), child.Path.Substring(child.Path.IndexOf("CN")))
                                Else
                                    dt.Rows.Add(UpperFirstLetter(child.SchemaClassName), child.Properties("sAMAccountName").Value.ToString, child.Path.Substring(child.Path.IndexOf("CN")))
                                End If
                            Catch
                                dt.Rows.Add(UpperFirstLetter(child.SchemaClassName), child.Properties("name").Value.ToString, child.Path.Substring(child.Path.IndexOf("CN")))
                            End Try
                        Else
                            If Filters.Contains(LCase(child.SchemaClassName)) Then
                                Try
                                    If LCase(child.SchemaClassName) = "computer" Then
                                        dt.Rows.Add(UpperFirstLetter(child.SchemaClassName), child.Properties("sAMAccountName").Value.ToString.Replace("$", ""), child.Path.Substring(child.Path.IndexOf("CN")))
                                    Else
                                        dt.Rows.Add(UpperFirstLetter(child.SchemaClassName), child.Properties("sAMAccountName").Value.ToString, child.Path.Substring(child.Path.IndexOf("CN")))
                                    End If
                                Catch
                                    dt.Rows.Add(UpperFirstLetter(child.SchemaClassName), child.Properties("name").Value.ToString, child.Path.Substring(child.Path.IndexOf("CN")))
                                End Try
                            End If
                        End If
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

