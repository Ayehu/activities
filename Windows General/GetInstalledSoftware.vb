Imports Ayehu.Sdk.ActivityCreation.Interfaces
Imports Ayehu.Sdk.ActivityCreation.Extension
Imports System.Text
Imports System.Runtime.InteropServices
Imports System.Management
Imports System.Collections.Generic
Imports System.IO
Imports System.Timers
Imports Microsoft.VisualBasic
Imports System
Imports System.Xml
Imports System.Data

Namespace Ayehu.Sdk.ActivityCreation
    Public Class ActivityClass
        Implements IActivity


        Public HostName As String
        Public UserName As String
        Public Password As String
        Public TimeInSeconds As Integer

        Public Function Execute() As ICustomActivityResult Implements IActivity.Execute


            Dim dt As DataTable = New DataTable("resultSet")
            dt.Columns.Add(("Name"), GetType(String))
            dt.Columns.Add(("Description"), GetType(String))
            dt.Columns.Add(("InstallState"), GetType(String))
            dt.Columns.Add(("Vendor"), GetType(String))
            dt.Columns.Add(("Version"), GetType(String))
            dt.Columns.Add(("IdentifyingNumber"), GetType(String))

            Dim wmiQuery As String = "Select Name, Description, InstallState, Vendor, Version, IdentifyingNumber, InstallLocation from Win32_Product"
            Dim connectionOptions As ConnectionOptions = New ConnectionOptions
            connectionOptions.Username = UserName
            connectionOptions.Password = Password
            connectionOptions.Authentication = 6

            Dim PropertyName As String = ""
            Dim Classname As String = ""
            Dim oMs As ManagementScope
            oMs = New ManagementScope("\\" & HostName & "\root\CIMV2", connectionOptions)
            Dim wmiClass As ManagementClass = New ManagementClass("Win32_Product")
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
                dr("Name") = oReturn("Name")
                dr("Description") = oReturn("Description")
                dr("InstallState") = getInstallState(oReturn("InstallState"))
                dr("Vendor") = oReturn("Vendor")
                dr("Version") = oReturn("Version")
                dr("IdentifyingNumber") = oReturn("IdentifyingNumber")
                dt.Rows.Add(dr)
                If exittime < Date.UtcNow Then Exit For
            Next

            Return Me.GenerateActivityResult(dt)
        End Function

        Private Function getInstallState(ByVal state As Object) As Object
            Select Case state
                Case -6
                    Return "Bad configuration"
                Case -2
                    Return "Invalid argument"
                Case -1
                    Return "Unknown package"
                Case 1
                    Return "Advertised"
                Case 2
                    Return "Absent"
                Case 5
                    Return "Installed"
            End Select
        End Function


    End Class
End Namespace

