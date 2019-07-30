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

        Public Function Execute() As ICustomActivityResult Implements IActivity.Execute


            Dim dt As DataTable = New DataTable("resultSet")
            dt.Columns.Add("Name", GetType(String))
            dt.Columns.Add("Size", GetType(String))
            dt.Columns.Add("Free Space", GetType(String))


            Dim connectionOptions As ConnectionOptions = New ConnectionOptions
            connectionOptions.Username = UserName
            connectionOptions.Password = Password
            connectionOptions.Authentication = AuthenticationLevel.PacketPrivacy
            connectionOptions.Impersonation = ImpersonationLevel.Impersonate
            connectionOptions.EnablePrivileges = True
            Dim oms As Management.ManagementScope

            If LCase(HostName) = "localhost" OrElse HostName = "127.0.0.1" Then
                oms = New ManagementScope("\\.\root\cimv2")
            Else
                oms = New ManagementScope("\\" & HostName & "\root\cimv2", connectionOptions)
            End If
            Dim Fnd As Boolean = False



            Dim oQuery As ObjectQuery = New System.Management.ObjectQuery("select FreeSpace,Size,Name from Win32_LogicalDisk where DriveType=3")
            Dim oSearcher As ManagementObjectSearcher = New ManagementObjectSearcher(oms, oQuery)

            For Each oReturn As ManagementObject In oSearcher.Get()
                Fnd = True
                dt.Rows.Add(oReturn("Name"), Math.Round(oReturn("Size") / 1024 / 1024, 2), Math.Round(oReturn("FreeSpace") / 1024 / 1024, 2))
            Next

            If Fnd = False Then
                dt = New DataTable("resultSet")
                dt.Columns.Add("Result", GetType(String))
                Throw New Exception("Volume not found")
            End If



            Return Me.GenerateActivityResult(dt)

        End Function


    End Class
End Namespace

