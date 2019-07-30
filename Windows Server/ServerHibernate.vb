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
        Public UserName As String
        Public Password As String

        Public Function Execute() As ICustomActivityResult Implements IActivity.Execute


            Dim RetriveEnable As Boolean = False
            Dim dt As DataTable = New DataTable("resultSet")
            dt.Columns.Add("Result", GetType(String))

            Dim Filename As String = Guid.NewGuid.ToString + ".TXT"
            Dim strWorkingDirectory = "c:\"
            Dim Locator = CreateObject("WbemScripting.SWbemLocator")
            Dim objService = Locator.ConnectServer(HostName, "root\cimv2", UserName, Password)
            objService.Security_.AuthenticationLevel = 6
            Dim objStartup = objService.Get("Win32_ProcessStartup")
            Dim objConfig = objStartup.SpawnInstance_
            objConfig.ShowWindow = 1
            Dim strCmd As String = "rundll32 powrprof.dll,SetSuspendState Hibernate"
            Dim objProcess = objService.Get("Win32_Process")
            Dim intProcessID As Integer = 0
            Dim intReturn = objProcess.Create(strCmd, "c:\", objConfig, intProcessID)
            If intProcessID > 0 Then
                dt.Rows.Add("Success")
            Else
                Throw New Exception("Failure")
            End If




            Return Me.GenerateActivityResult(dt)
        End Function


    End Class
End Namespace

