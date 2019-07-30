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

Namespace Ayehu.Sdk.ActivityCreation
    Public Class ActivityClass
        Implements IActivity


        Public HostName As String
        Public UserName As String
        Public Password As String
        Public VDName As String
        Public WebsiteName As String
        Public VDPath As String
        Public IsScripts As String
        Public IsExecutables As String
        Public IsRead As String
        Public IsWrite As String
        Public IsBrowseDir As String
        Public IsCreateApplication As String
        Public ApplicationName As String
        Public ContentSource As String
        Public RedirectURLWithParams As String
        Public AppPoolName As String

        Public Function Execute() As ICustomActivityResult Implements IActivity.Execute


            Dim sw As New StringWriter
            Dim dt As DataTable = New DataTable("resultSet")
            dt.Columns.Add("Result", GetType(String))

            If String.IsNullOrEmpty(VDName) Then Throw New Exception("Illegal directory name")
            Dim IISSchema As New System.DirectoryServices.DirectoryEntry("IIS://" & HostName & "/Schema/AppIsolated")
            If String.IsNullOrEmpty(UserName) = False Then
                IISSchema.Username = UserName
                IISSchema.Password = Password
            End If
            Dim CanCreate As Boolean = Not IISSchema.Properties("Syntax").Value.ToString.ToUpper() = "BOOLEAN"
            IISSchema.Dispose()

            If CanCreate Then

                Dim connectionOptions As ConnectionOptions = New ConnectionOptions
                connectionOptions.Username = UserName
                connectionOptions.Password = Password
                connectionOptions.Authentication = AuthenticationLevel.PacketPrivacy
                connectionOptions.Impersonation = ImpersonationLevel.Impersonate
                connectionOptions.EnablePrivileges = True
                Dim oms As Management.ManagementScope
                Dim oms2 As Management.ManagementScope
                If LCase(HostName) = "localhost" OrElse HostName = "127.0.0.1" Then
                    oms = New ManagementScope("\\.\root\microsoftiisv2")
                    oms2 = New ManagementScope("\\.\root\cimv2")
                Else
                    oms = New ManagementScope("\\" & HostName & "\root\microsoftiisv2", connectionOptions)
                    oms2 = New ManagementScope("\\" & HostName & "\root\cimv2", connectionOptions)
                End If

                'Get the common name of the website
                Dim oQuery As ObjectQuery = New System.Management.ObjectQuery("SELECT * FROM IISWebServerSetting WHERE ServerComment='" & WebsiteName & "'")
                Dim oSearcher As ManagementObjectSearcher = New ManagementObjectSearcher(oms, oQuery)
                Dim oReturnCollection As ManagementObjectCollection = oSearcher.Get()
                Dim CommonwebSiteName As String = ""
                For Each oReturn As ManagementObject In oReturnCollection
                    CommonwebSiteName = oReturn("Name")
                Next
                If String.IsNullOrEmpty(CommonwebSiteName) Then Throw New Exception("Website doesn't exist")

                Dim PathCreated As Boolean
                Dim IISAdmin As New System.DirectoryServices.DirectoryEntry("IIS://" & HostName & "/" & CommonwebSiteName & "/Root")
                If String.IsNullOrEmpty(UserName) = False Then
                    IISAdmin.Username = UserName
                    IISAdmin.Password = Password
                End If


                Dim sAppPoolFullName As String = ""
                If Len(AppPoolName) > 0 Then ' Don't use the default application Pool (use custom one)
                    sAppPoolFullName = "W3SVC/AppPools/" & AppPoolName
                    oQuery = New System.Management.ObjectQuery("SELECT * FROM IIsApplicationPool WHERE Name = '" & sAppPoolFullName & "'")
                    oSearcher = New ManagementObjectSearcher(oms, oQuery)
                    oReturnCollection = oSearcher.Get()
                    If oReturnCollection.Count = 0 Then Throw New Exception("No Application Pool found (" & AppPoolName & ")")
                End If



                Dim VDir As System.DirectoryServices.DirectoryEntry = IISAdmin.Children.Add(VDName, "IIsWebVirtualDir")
                If String.IsNullOrEmpty(AppPoolName) = False Then VDir.Properties("AppPoolId").Item(0) = AppPoolName
                VDir.Properties("EnableDirBrowsing").Item(0) = bool(IsBrowseDir)
                VDir.Properties("AccessRead").Item(0) = bool(IsRead)
                VDir.Properties("AccessExecute").Item(0) = bool(IsExecutables)
                VDir.Properties("AccessWrite").Item(0) = bool(IsWrite)
                VDir.Properties("AccessScript").Item(0) = bool(IsScripts)
                Select Case CInt(ContentSource)
                    Case 1 ' directory option, set path and access flags
                        VDir.Properties("Path").Item(0) = VDPath
                    Case 2 ' redirect to a nother url
                        VDir.Properties("HttpRedirect").Item(0) = RedirectURLWithParams
                        VDir.Properties("Path").Item(0) = "C:\"
                End Select

                ' VDir.Properties("AuthNTLM").Item(0) = True
                VDir.Properties("EnableDefaultDoc").Item(0) = True
                VDir.Properties("DefaultDoc").Item(0) = "default.htm,default.aspx,default.asp"
                VDir.Properties("AspEnableParentPaths").Item(0) = True
                VDir.CommitChanges()

                If bool(IsCreateApplication) = True Then
                    If Len(ApplicationName) > 0 Then VDir.Properties("AppFriendlyName").Item(0) = ApplicationName
                    Try
                        VDir.Invoke("AppCreate", True)
                    Catch
                    End Try
                    Try
                        VDir.Invoke("AppCreate2", New Object() {2})
                    Catch
                    End Try
                End If
                dt.Rows.Add("Success")
            Else
                Throw New Exception("Error creating virtual directory")
            End If

            Return Me.GenerateActivityResult(dt)


        End Function

        Function bool(Name As String) As Boolean

            If lcase(Name) = "true" Then
                Return True
            Else
                Return False
            End If

        End Function

    End Class
End Namespace

