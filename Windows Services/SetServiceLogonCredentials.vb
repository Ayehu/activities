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
        Public SRVDomain As String
        Public SRVUserName As String
        Public SRVPassword As String
        Public SRVType As Integer
        Public SRVInteract As Integer

        Public Function Execute() As ICustomActivityResult Implements IActivity.Execute


            Dim dt As DataTable = New DataTable("resultSet")
            dt.Columns.Add("Result")



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
            Dim result As Object
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
                If LCase(oReturn("Name")) = LCase(ServiceName) OrElse LCase(oReturn("DisplayName")) = LCase(ServiceName) Then
                    If SRVType = 0 Then
                        If SRVInteract = 1 Then
                            result = oReturn.InvokeMethod("Change", New Object() {Nothing, Nothing, Nothing, Nothing, Nothing, True, "LocalSystem", "", Nothing, Nothing, Nothing})
                        Else
                            result = oReturn.InvokeMethod("Change", New Object() {Nothing, Nothing, Nothing, Nothing, Nothing, False, "LocalSystem", "", Nothing, Nothing, Nothing})
                        End If

                    Else
                        If SRVUserName <> "" Then
                            If SRVDomain = "" Then SRVDomain = "."
                            SRVUserName = Path.Combine(SRVDomain, SRVUserName)
                            result = oReturn.InvokeMethod("Change", New Object() {Nothing, Nothing, Nothing, Nothing, Nothing, False, SRVUserName, SRVPassword, Nothing, Nothing, Nothing})
                        Else
                            result = oReturn.InvokeMethod("Change", New Object() {Nothing, Nothing, Nothing, Nothing, Nothing, False, Nothing, SRVPassword, Nothing, Nothing, Nothing})
                        End If
                    End If

                    fnd = True
                End If
            Next
            If fnd = False Then Throw New Exception("Service not found")
            If result Is Nothing Or CType(result, UInteger) <> 0 Then Throw New Exception("Service could not be altered " & CType(result, UInteger))
            dt.Rows.Add("Success")

            Return Me.GenerateActivityResult(dt)

        End Function


    End Class
End Namespace

