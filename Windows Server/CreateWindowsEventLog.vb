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
Imports System.Diagnostics

Namespace Ayehu.Sdk.ActivityCreation
    Public Class ActivityClass
        Implements IActivity


        Public HostName As String
        Public UserName As String
        Public Password As String
        Public LogName As String
        Public LogType As String
        Public LogSource As String
        Public LogEventID As String
        Public LogCategory As String
        Public LogEvent As String

        Public Function Execute() As ICustomActivityResult Implements IActivity.Execute


            Dim dt As New DataTable("resultSet")
            dt.Columns.Add("Results")

            If HostName = "127.0.0.1" Then HostName = "."
            If String.IsNullOrEmpty(LogSource) Then Throw New Exception("Failure - Source is missing")
            If String.IsNullOrEmpty(LogType) Then LogType = "Information"
            If String.IsNullOrEmpty(LogName) Then LogName = "Application"
            Dim SelecetedType As EventLogEntryType
            Select Case LogType
                Case "Error"
                    SelecetedType = EventLogEntryType.Error
                Case "Information"
                    SelecetedType = EventLogEntryType.Information
                Case "FailureAudit"
                    SelecetedType = EventLogEntryType.FailureAudit
                Case "SuccessAudit"
                    SelecetedType = EventLogEntryType.SuccessAudit
                Case "Warning"
                    SelecetedType = EventLogEntryType.Warning
            End Select

            Dim EventID As Integer = val(LogEventID)
            Dim ELog As New EventLog(LogName, HostName, LogSource)
            ELog.WriteEntry(LogEvent, SelecetedType, EventID, CType(val(LogCategory), Short))
            dt.Rows.Add("Success")

            Return Me.GenerateActivityResult(dt)

        End Function




    End Class
End Namespace

