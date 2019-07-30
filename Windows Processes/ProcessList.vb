Imports Ayehu.Sdk.ActivityCreation.Interfaces
Imports Ayehu.Sdk.ActivityCreation.Extension
Imports System.Text
Imports System.Runtime.InteropServices
Imports System.Management
Imports System.Collections.Generic
Imports System.IO
Imports System.Timers
Imports System
Imports System.Xml
Imports System.Data
Imports Microsoft.VisualBasic

Namespace Ayehu.Sdk.ActivityCreation
    Public Class ActivityClass
        Implements IActivity


        Public HostName As String
        Public UserName As String
        Public Password As String
        Public TimeInSeconds As Integer

        Public Function Execute() As ICustomActivityResult Implements IActivity.Execute


            Dim dt As DataTable = New DataTable("resultSet")
            Dim newDt As DataTable




            Dim wmiQuery As String = "Select * from Win32_PerfFormattedData_PerfProc_Process"
            Dim Counter As Integer = 0
            Dim connectionOptions As ConnectionOptions = New ConnectionOptions
            connectionOptions.Username = UserName
            connectionOptions.Password = Password
            connectionOptions.Authentication = 6
            Dim PropertyName As String = ""
            Dim Classname As String = ""
            Dim oMs As ManagementScope

            oMs = New ManagementScope("\\" & HostName & "\root\CIMV2", connectionOptions)
            Dim wmiClass As ManagementClass = New ManagementClass("Win32_PerfFormattedData_PerfProc_Process")
            Dim allParams As New List(Of String)
            For Each prop As PropertyData In wmiClass.Properties
                If Trim(prop.Name) <> "WorkingSetPrivate" Then
                    dt.Columns.Add(Trim(prop.Name), GetType(String))
                    allParams.Add(Trim(prop.Name))
                End If
            Next
            Dim exittime As Date = DateAdd(DateInterval.Second, TimeInSeconds, Date.UtcNow)
            Dim exitc As Boolean = False

            If LCase(HostName) = "localhost" OrElse HostName = "127.0.0.1" Then
                oMs = New ManagementScope("\\.\root\cimv2")
            Else
                oMs = New ManagementScope("\\" & HostName & "\root\cimv2", connectionOptions)
            End If



            Dim oQuery As ObjectQuery = New System.Management.ObjectQuery(wmiQuery)
            Dim oSearcher As ManagementObjectSearcher = New ManagementObjectSearcher(oMs, oQuery)
            Dim oReturnCollection As ManagementObjectCollection = oSearcher.Get()

            For Each oReturn As ManagementObject In oReturnCollection
                Dim dr As DataRow = dt.NewRow()
                For Each prop As String In allParams
                    dr(prop) = oReturn(prop)
                Next
                dt.Rows.Add(dr)
                If exittime < Date.UtcNow Then Exit For
            Next
            dt.Columns("Name").SetOrdinal(0)
            dt.Columns("IDProcess").SetOrdinal(1)
            dt.Columns("Description").SetOrdinal(2)
            dt.Columns("WorkingSet").SetOrdinal(3)
            dt.Columns("WorkingSet").ColumnName = "Memory"
            dt.Columns("PercentProcessorTime").SetOrdinal(4)
            dt.Columns("PercentProcessorTime").ColumnName = "CPU"
            dt.Columns.Remove("Caption")
            dt.Columns.Remove("Description")




            newDt = dt.Clone()
            newDt.Columns("Memory").DataType = GetType(ULong)
            newDt.Columns("CPU").DataType = GetType(ULong)
            newDt.Columns("IDProcess").DataType = GetType(Integer)
            newDt.Columns("CreatingProcessID").DataType = GetType(Integer)
            newDt.Columns("ElapsedTime").DataType = GetType(ULong)
            newDt.Columns("HandleCount").DataType = GetType(Integer)
            newDt.Columns("IODataBytesPerSec").DataType = GetType(ULong)
            newDt.Columns("IODataOperationsPerSec").DataType = GetType(ULong)
            newDt.Columns("IOOtherBytesPerSec").DataType = GetType(ULong)
            newDt.Columns("IOOtherOperationsPerSec").DataType = GetType(ULong)
            newDt.Columns("IOReadBytesPerSec").DataType = GetType(ULong)
            newDt.Columns("IOReadOperationsPerSec").DataType = GetType(ULong)
            newDt.Columns("IOWriteBytesPerSec").DataType = GetType(ULong)
            newDt.Columns("IOWriteOperationsPerSec").DataType = GetType(ULong)

            For Each row As DataRow In dt.Rows
                newDt.ImportRow(row)
            Next

            newDt.columns.add("Memory%", GetType(Double))
            Dim TotalPhisicalMemory As Double
            oQuery = New System.Management.ObjectQuery("Select TotalVisibleMemorySize from Win32_OperatingSystem")
            oSearcher = New ManagementObjectSearcher(oMs, oQuery)
            For Each oReturn As ManagementObject In oSearcher.Get()
                TotalPhisicalMemory = oReturn("TotalVisibleMemorySize")
            Next

            For Each Row As datarow In newDt.rows
                Row("Memory%") = Math.Round(((Row("Memory") / 1024) / TotalPhisicalMemory) * 100, 2)
            Next



            newDt.Rows.RemoveAt(newDt.Rows.Count - 1)
            Return Me.GenerateActivityResult(dt)
        End Function


    End Class
End Namespace

