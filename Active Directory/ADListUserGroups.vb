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
            dt.Columns.Add("Name", GetType(String))
            dt.Columns.Add("OU", GetType(String))

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

                    Dim groups As Object = dey.Properties("MemberOf").Value


                    Dim primaryGroupName As String = GetPrimaryGroup(dey, de)
                    If (String.IsNullOrEmpty(primaryGroupName) = False) Then
                        dt.Rows.Add(primaryGroupName, "")
                    Else
                        Throw New Exception("Failed to retrieve primary group name.")
                    End If


                    If groups IsNot Nothing Then
                        Dim TempName As String = String.Empty
                        If groups.GetType Is GetType(String) Then
                            TempName = groups.Substring(LCase(groups).IndexOf("cn=") + 3)
                            TempName = TempName.Substring(0, TempName.IndexOf(","))
                            dt.Rows.Add(TempName, CleanEscapeCharacters(groups))
                        Else

                            For Each group As String In groups
                                TempName = group.Substring(LCase(group).IndexOf("cn=") + 3)
                                TempName = TempName.Substring(0, TempName.IndexOf(","))
                                dt.Rows.Add(TempName, CleanEscapeCharacters(group))
                            Next
                        End If
                        dey.Close()
                    End If
                Else
                    Throw New Exception("User does not exist")
                End If
            End If

            de.Close()

            Return Me.GenerateActivityResult(dt)

        End Function

        Function CleanEscapeCharacters(ByVal groupName As String) As String
            groupName = Replace(groupName, (Chr(92) + Chr(34)), Chr(34))    ' '\"' change to '"'
            'groupName = Replace(groupName, "\/", Chr(47))                   ' '\/' change to '/'
            Return groupName
        End Function

        Private Function GetPrimaryGroup(aEntry As DirectoryEntry, aDomainEntry As DirectoryEntry) As String

            Dim primaryGroupID As Integer = CType(aEntry.Properties("primaryGroupID").Value, Integer)
            Dim objectSid() As Byte = CType(aEntry.Properties("objectSid").Value, Byte())

            Dim escapedGroupSid As StringBuilder = New StringBuilder()

            For i As UInt32 = 0 To objectSid.Length - 5 Step 1
                escapedGroupSid.AppendFormat("\{0:x2}", objectSid(i))
            Next

            For i As UInt32 = 0 To 3 Step 1
                escapedGroupSid.AppendFormat("\{0:x2}", (primaryGroupID And 255))
                primaryGroupID >>= 8
            Next

            Dim primGroupName As String = String.Empty

            Using searcher As DirectorySearcher = New DirectorySearcher()
                If (searcher IsNot Nothing) Then
                    searcher.SearchRoot = aDomainEntry
                End If
                searcher.Filter = "(&(objectCategory=Group)(objectSID=" + escapedGroupSid.ToString() + "))"
                searcher.PropertiesToLoad.Add("samaccountname")
                Dim srchResult As SearchResult = searcher.FindOne()
                If (srchResult.Properties("samaccountname") IsNot Nothing) Then
                    If (srchResult.Properties("samaccountname")(0) IsNot Nothing) Then
                        primGroupName = srchResult.Properties("samaccountname")(0).ToString()
                    End If
                End If
            End Using

            Return primGroupName
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

