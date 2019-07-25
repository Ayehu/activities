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
        Public Days As String
        Public SecurePort As String

        Public Function Execute() As ICustomActivityResult Implements IActivity.Execute

          ByVal ADUserName As String, ByVal Days As String, ByVal SecurePort As String) As String
          
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

            Dim maxage As Integer = GetMaxAge(de)

            If Days = "-1" Then Days = maxage

            Days = Math.Round(Val(Days)).ToString
            If Val(Days) > 10000 Then
                dt.Rows.Add("No password policy")

            ElseIf Trim(ADUserName) <> "" Then

                Dim ds As DirectorySearcher = New DirectorySearcher(de)
                ds.Filter = "(&(objectClass=user) (sAMAccountName=" + ADUserName + "))"
                ds.PropertiesToLoad().Add("msDS-UserPasswordExpiryTimeComputed")
                ds.SearchScope = SearchScope.Subtree
                Dim results As SearchResult = ds.FindOne()
                If results IsNot Nothing Then
                    Dim dey As DirectoryEntry = GetAdEntryByFullPath(results.Path, UserName, Password)

                    Dim allProperties = ds.FindOne().Properties
                    Dim maxPasswordAge As System.Collections.DictionaryEntry = Nothing

                    For Each entry As System.Collections.DictionaryEntry In allProperties
                        If entry.Key = "msds-userpasswordexpirytimecomputed" Then
                            maxPasswordAge = entry
                        End If
                    Next

                    If maxPasswordAge.Key Is Nothing Then
                        dt.Rows.Add("Password policy is not set")
                    Else

                        Try
                            Dim ChangeDate As DateTime = GetExpiration(dey, maxPasswordAge)
                            Dim time As TimeSpan
                            If Days > 0 Then
                                time = ChangeDate - DateTime.Now
                                Dim prv As Integer = maxage - time.Days  ' this returns the distance of days from AD
                                Dim n As Integer = Days - prv
                                If n < 0 Then
                                    dt.Rows.Add("Password expired")
                                Else
                                    dt.Rows.Add(n.ToString)
                                End If


                            Else
                                time = ChangeDate - DateTime.Now
                                If time.Days < 0 Then
                                    dt.Rows.Add("Password expired")
                                Else
                                    dt.Rows.Add(time.Days)
                                End If
                            End If

                            dey.Close()
                        Catch ex As Exception
                            dt.Rows.Add(Err.Description)
                        End Try
                    End If
                Else
                    Throw New Exception("User does not exist")
                End If
            Else
                Throw New Exception("Account name must exist")

            End If

            de.Close()

            Return Me.GenerateActivityResult(dt)
        End Function

        Function GetMaxAge(ByVal de As DirectoryEntry)
            Dim ticks As Long = GetInt64(de, "MaxPwdAge")
            Dim maxPwdAge As TimeSpan = TimeSpan.FromTicks(ticks)
            Return (maxPwdAge.TotalDays * -1)
        End Function

        Public Function GetExpiration(ByVal user As DirectoryEntry,
        Optional ByVal properties As System.Collections.DictionaryEntry = Nothing) _
        As DateTime
            'If password never expired set
            If Convert.ToBoolean(user.Properties("userAccountControl").Value And &H10000) Then _
            Throw New Exception("Password never expires")

            Dim ticks As Long = GetInt64(user, "pwdLastSet")

            'user must change password at next login
            If ticks = 0 Then Throw New Exception("Password expired")

            'password has never been set
            If (ticks = -1) Then Throw New Exception("User does not have a password")

            If properties.Key Is Nothing Then
                Throw New ArgumentNullException("properties")
            End If

            Dim someValue = CType(properties.Value, System.DirectoryServices.ResultPropertyValueCollection)

            Dim pwdLastSet As DateTime = DateTime.FromFileTime(someValue.Item(0))

            Return pwdLastSet
        End Function

        Function GetInt64(ByVal entry As DirectoryEntry, ByVal attr As String) As Int64
            Dim ds As DirectorySearcher = New DirectorySearcher(entry, String.Format("({0}=*)", attr),
            New String() {attr}, SearchScope.Base)
            Dim sr As SearchResult = ds.FindOne()
            If sr IsNot Nothing Then
                If sr.Properties.Contains(attr) Then Return Convert.ToInt64(sr.Properties(attr)(0))
            End If
            Return -1
        End Function

        Public Function GetAdEntryByFullPath(ByVal path As String, ByVal userName As String, ByVal password As String) _
        As DirectoryEntry
            Dim adEntry = New DirectoryEntry(path, userName, password, AuthenticationTypes.Secure)
            Return adEntry
        End Function

        Public Function GetAdEntry(ByVal domainServer As String, ByVal domainPort As String, ByVal username As String,
        ByVal password As String) As DirectoryEntry
            Dim defaultAdSecurePort As String = "636"
            If domainPort.Equals(defaultAdSecurePort) AndAlso IsIpAddress(domainServer) Then _
            Throw New Exception("When using a secure port, a server domain name must be defined for the device.")
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

