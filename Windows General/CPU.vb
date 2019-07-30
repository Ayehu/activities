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

            Dim oQuery As ObjectQuery = New System.Management.ObjectQuery("Select LoadPercentage from Win32_Processor")
            Dim oSearcher As ManagementObjectSearcher = New ManagementObjectSearcher(oms, oQuery)

            Dim Resultint As Integer = 0
            Dim Counter As Integer = 0
            For Each oReturn As ManagementObject In oSearcher.Get()
                Counter = Counter + 1
                Select Case SelectedDeviceStatus
                    Case "Available"
                        dt.Rows.Add(100 - oReturn("LoadPercentage"))
                    Case "Average Available"
                        Resultint = Resultint + (100 - oReturn("LoadPercentage"))
                    Case "Usage"
                        dt.Rows.Add(oReturn("LoadPercentage"))
                    Case "Average Usage"
                        Resultint = Resultint + oReturn("LoadPercentage")
                End Select
            Next

            If SelectedDeviceStatus = "Average Available" OrElse SelectedDeviceStatus = "Average Usage" Then
                dt.Rows.Add(Math.Round(Resultint / Counter, 2))
            End If
            Return Me.GenerateActivityResult(dt)
        End Function


    End Class
End Namespace

