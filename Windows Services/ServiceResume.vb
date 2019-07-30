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
        Public ServiceName As String

        Public Function Execute() As ICustomActivityResult Implements IActivity.Execute

            Dim dt As DataTable = New DataTable("resultSet")
            dt.Columns.Add("Result", GetType(String))


            Dim connectionOptions As ConnectionOptions = New ConnectionOptions
            connectionOptions.Username = UserName
            connectionOptions.Password = Password
            connectionOptions.Authentication = AuthenticationLevel.PacketPrivacy
            connectionOptions.Impersonation = ImpersonationLevel.Impersonate
            connectionOptions.EnablePrivileges = True

            Dim oms As Management.ManagementScope

            If LCase(HostName) = "localhost" OrElse HostName = "127.0.0.1" Then
                oms = New ManagementScope("\\.\root/cimv2")
            Else
                oms = New ManagementScope("\\" & HostName & "\root/cimv2", connectionOptions)
            End If

            Dim oQuery As ObjectQuery = New System.Management.ObjectQuery("Select * from  Win32_Service where name = '" & ServiceName & "'")
            Dim oSearcher As ManagementObjectSearcher = New ManagementObjectSearcher(oms, oQuery)
            Dim oReturnCollection As ManagementObjectCollection = oSearcher.Get()
            Dim Fnd As Boolean = False
            For Each oReturn As ManagementObject In oReturnCollection
                Fnd = True
                oReturn.InvokeMethod("ResumeService", Nothing)
            Next

            If Fnd = False Then
                Throw New Exception("Service not found")
            Else
                oQuery = New System.Management.ObjectQuery("Select state from  Win32_Service where name = '" & ServiceName & "'")
                oSearcher = New ManagementObjectSearcher(oms, oQuery)
                oReturnCollection = oSearcher.Get()

                For Each oReturn As ManagementObject In oReturnCollection
                    Fnd = True
                    dt.Rows.Add(oReturn("state"))
                Next
            End If


            Return Me.GenerateActivityResult(dt)

        End Function


    End Class
End Namespace

