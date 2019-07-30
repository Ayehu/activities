Imports Ayehu.Sdk.ActivityCreation.Interfaces
Imports Ayehu.Sdk.ActivityCreation.Extension
Imports System.Text
Imports System
Imports System.Xml
Imports System.Data
Imports System.IO
Imports System.Collections.Generic
Imports Microsoft.VisualBasic
Imports System.Management

Namespace Ayehu.Sdk.ActivityCreation
    Public Class ActivityClass
        Implements IActivity


        Public HostName As String
        Public ProcessName As String
        Public UserName As String
        Public Password As String

        Public Function Execute() As ICustomActivityResult Implements IActivity.Execute


            Dim dt As DataTable = New DataTable("resultSet")
            dt.Columns.Add("PID", GetType(String))
            dt.Columns.Add("CPU", GetType(String))


            Dim connectionOptions As ConnectionOptions = New ConnectionOptions
            connectionOptions.Username = UserName
            connectionOptions.Password = Password
            connectionOptions.Authentication = AuthenticationLevel.PacketPrivacy
            connectionOptions.Impersonation = ImpersonationLevel.Impersonate
            connectionOptions.EnablePrivileges = True
            Dim oms As Management.ManagementScope
            Dim HasWildCard As Boolean = False

            If LCase(HostName) = "localhost" OrElse HostName = "127.0.0.1" Then
                oms = New ManagementScope("\\.\root\cimv2")
            Else
                oms = New ManagementScope("\\" & HostName & "\root\cimv2", connectionOptions)
            End If
            If LCase(ProcessName).Contains(".exe") AndAlso LCase(ProcessName).EndsWith(".exe") Then ProcessName = Mid(ProcessName, 1, Len(ProcessName) - 4)
            If ProcessName.StartsWith("*") OrElse ProcessName.EndsWith("*") Then
                ProcessName = ProcessName.Replace("*", "")
                HasWildCard = True
            End If
            Dim oQuery As ObjectQuery = New System.Management.ObjectQuery("Select Name, IDProcess , PercentProcessorTime from  Win32_PerfFormattedData_PerfProc_Process where name LIKE '%" & ProcessName & "%'")
            Dim oSearcher As ManagementObjectSearcher = New ManagementObjectSearcher(oms, oQuery)
            Dim Fnd As Boolean = False
            For Each oReturn As ManagementObject In oSearcher.Get()

                If HasWildCard = True Then
                    Fnd = True
                    dt.Rows.Add(oReturn("IDProcess"), oReturn("PercentProcessorTime"))
                Else
                    If LCase(oReturn("Name")) = LCase(ProcessName) OrElse LCase(oReturn("Name")) = LCase(ProcessName & ".exe") OrElse LCase(oReturn("Name")).Contains(LCase(ProcessName) & "#") OrElse LCase(oReturn("Name")).Contains(LCase(ProcessName) & ".exe#") Then
                        Fnd = True
                        dt.Rows.Add(oReturn("IDProcess"), oReturn("PercentProcessorTime"))
                    End If
                End If

            Next
            If Fnd = False Then
                dt = New DataTable("resultSet")
                dt.Columns.Add("Result", GetType(String))
                Throw New Exception("Process does not exist")
            End If


            Return Me.GenerateActivityResult(dt)

        End Function


    End Class
End Namespace

