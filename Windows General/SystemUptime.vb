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
Imports System.Net.Security
Imports ActivitiesUtilsLib

Namespace Ayehu.Sdk.ActivityCreation
    Public Class ActivityClass
        Implements IActivity


        Public HostName As String
        Public UserName As String
        Public Password As String
        Public TimeInterval As String

        Public Function Execute() As ICustomActivityResult Implements IActivity.Execute

            Dim dt As DataTable = New DataTable("resultSet")

            Dim oms As ManagementScope = ActivitiesUtils.GetWinManagementScope(UserName, Password, HostName, "\root\CIMV2")

            dt.Columns.Add("Result", GetType(String))

            Dim oQuery As ObjectQuery = New System.Management.ObjectQuery("Select SystemUpTime from Win32_PerfFormattedData_PerfOS_System")
            Dim oSearcher As ManagementObjectSearcher = New ManagementObjectSearcher(oms, oQuery)

            For Each oReturn As ManagementObject In oSearcher.Get()
                Select Case LCase(TimeInterval)
                    Case "seconds"
                        dt.Rows.Add(oReturn("SystemUpTime"))
                    Case "minutes"
                        dt.Rows.Add((Val(oReturn("SystemUpTime")) / 60).ToString("0.00"))
                    Case "hours"
                        dt.Rows.Add((Val(oReturn("SystemUpTime")) / 60 / 60).ToString("0.00"))
                    Case "days"
                        dt.Rows.Add((Val(oReturn("SystemUpTime")) / 60 / 60 / 24).ToString("0.00"))
                    Case Else
                        Throw New Exception("Time interval is invalid")
                End Select
            Next
            Return Me.GenerateActivityResult(dt)
        End Function


    End Class
End Namespace

