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
        Public ServiceName As String
        Public ServiceType As String

        Public Function Execute() As ICustomActivityResult Implements IActivity.Execute


            Dim dt As DataTable = New DataTable("resultSet")
            dt.Columns.Add("Result")



            If lcase(ServiceType) <> "manual" AndAlso lcase(ServiceType) <> "automatic" AndAlso lcase(ServiceType) <> "disabled" Then Throw New Exception("Failure - unknown startup type")
            Dim wmiQuery As String = "Select Name, DisplayName, State, StartMode from Win32_Service"
            Dim connectionOptions As ConnectionOptions = New ConnectionOptions
            connectionOptions.Username = UserName
            connectionOptions.Password = Password
            connectionOptions.Authentication = 6

            Dim PropertyName As String = ""
            Dim Classname As String = ""
            Dim oMs As ManagementScope
            oMs = New ManagementScope("\\" & HostName & "\root\CIMV2", connectionOptions)
            Dim wmiClass As ManagementClass = New ManagementClass("Win32_Service")
            Dim exittime As Date = DateAdd(DateInterval.Second, TimeInSeconds, Date.UtcNow)
            Dim exitc As Boolean = False

            If LCase(HostName) = "localhost" OrElse HostName = "127.0.0.1" Then
                oMs = New ManagementScope("\\.\root/cimv2")
            Else
                oMs = New ManagementScope("\\" & HostName & "\root/cimv2", connectionOptions)
            End If

            Dim oQuery As ObjectQuery = New System.Management.ObjectQuery(wmiQuery)
            Dim oSearcher As ManagementObjectSearcher = New ManagementObjectSearcher(oMs, oQuery)
            Dim oReturnCollection As ManagementObjectCollection = oSearcher.Get()
            Dim fnd As Boolean = False
            For Each oReturn As ManagementObject In oReturnCollection
                If lcase(oReturn("Name")) = lcase(ServiceName) OrElse lcase(oReturn("DisplayName")) = lcase(ServiceName) Then
                    oReturn.InvokeMethod("ChangeStartMode", New Object() {ServiceType})

                    fnd = True
                End If


            Next
            If fnd = False Then Throw New Exception("Service not found")
            dt.rows.add("Success")

            Return Me.GenerateActivityResult(dt)

        End Function


    End Class
End Namespace

