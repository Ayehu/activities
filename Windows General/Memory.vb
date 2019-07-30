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
        Public SelectedDeviceStatus As String
        Public UserName As String
        Public Password As String

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
                oms = New ManagementScope("\\.\root\cimv2")
            Else
                oms = New ManagementScope("\\" & HostName & "\root\cimv2", connectionOptions)
            End If



            If SelectedDeviceStatus = "Available" Then
                Dim oQuery As ObjectQuery = New System.Management.ObjectQuery("Select AvailableMBytes from Win32_PerfRawData_PerfOS_Memory")
                Dim oSearcher As ManagementObjectSearcher = New ManagementObjectSearcher(oms, oQuery)
                For Each oReturn As ManagementObject In oSearcher.Get()
                    dt.Rows.Add(Math.Round(oReturn("AvailableMBytes"), 2))
                Next
            ElseIf SelectedDeviceStatus = "Total" Then
                Dim TotalPhisicalMemory As Double
                Dim oQuery As ObjectQuery = New System.Management.ObjectQuery("Select TotalVisibleMemorySize from Win32_OperatingSystem")
                Dim oSearcher As ManagementObjectSearcher = New ManagementObjectSearcher(oms, oQuery)
                For Each oReturn As ManagementObject In oSearcher.Get()
                    TotalPhisicalMemory = oReturn("TotalVisibleMemorySize")
                Next
                dt.Rows.Add(Math.Round(TotalPhisicalMemory / 1024, 2))

            Else
                Dim Available As Double
                Dim oQuery As ObjectQuery = New System.Management.ObjectQuery("Select AvailableMBytes from Win32_PerfRawData_PerfOS_Memory")
                Dim oSearcher As ManagementObjectSearcher = New ManagementObjectSearcher(oms, oQuery)
                For Each oReturn As ManagementObject In oSearcher.Get()
                    Available = oReturn("AvailableMBytes")
                Next

                Dim TotalPhisicalMemory As Double
                oQuery = New System.Management.ObjectQuery("Select TotalVisibleMemorySize from Win32_OperatingSystem")
                oSearcher = New ManagementObjectSearcher(oms, oQuery)
                For Each oReturn As ManagementObject In oSearcher.Get()
                    TotalPhisicalMemory = oReturn("TotalVisibleMemorySize")
                Next
                dt.Rows.Add(Math.Round(TotalPhisicalMemory / 1024 - Available, 2))

            End If


            Return Me.GenerateActivityResult(dt)

        End Function


    End Class
End Namespace

