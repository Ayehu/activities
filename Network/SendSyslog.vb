Imports Ayehu.Sdk.ActivityCreation.Interfaces
Imports Ayehu.Sdk.ActivityCreation.Extension
Imports System.Text
Imports System
Imports System.Xml
Imports System.Data
Imports System.IO
Imports System.Collections.Generic
Imports Microsoft.VisualBasic
Imports System.Net.Sockets
Imports System.Net

Namespace Ayehu.Sdk.ActivityCreation
    Public Class ActivityClass
        Implements IActivity


        Public HostName As String
        Public Message As String
        Public Facility As String
        Public Level As String
        Public Port As String

        Public Function Execute() As ICustomActivityResult Implements IActivity.Execute

            Dim dt As DataTable = New DataTable("resultSet")
            dt.Columns.Add("Result", GetType(String))

            If String.IsNullOrEmpty(Port) OrElse Port = "0" Then Port = "514"
            Dim Priority As Integer = 0
            Dim TheLevel As Integer = 0

            Select Case Level
                Case "Emergency"
                    TheLevel = 0
                Case "Alert"
                    TheLevel = 1
                Case "Critical"
                    TheLevel = 2
                Case "Error"
                    TheLevel = 3
                Case "Warning"
                    TheLevel = 4
                Case "Notice"
                    TheLevel = 5
                Case "Information"
                    TheLevel = 6
                Case "Debug"
                    TheLevel = 7
            End Select

            Select Case Facility
                Case "Kernel"
                    Priority = 0
                Case "User"
                    Priority = 1
                Case "Mail"
                    Priority = 2
                Case "Daemon"
                    Priority = 3
                Case "Auth"
                    Priority = 4
                Case "Syslog"
                    Priority = 5
                Case "Lpr"
                    Priority = 6
                Case "News"
                    Priority = 7
                Case "UUCP"
                    Priority = 8
                Case "Cron"
                    Priority = 9
                Case "System0"
                    Priority = 10
                Case "System1"
                    Priority = 11
                Case "System2"
                    Priority = 12
                Case "System3"
                    Priority = 13
                Case "System4"
                    Priority = 15
                Case "System5"
                    Priority = 16
                Case "Local0"
                    Priority = 17
                Case "Local1"
                    Priority = 18
                Case "Local2"
                    Priority = 19
                Case "Local3"
                    Priority = 20
                Case "Local4"
                    Priority = 21
                Case "Local5"
                    Priority = 22
                Case "Local6"
                    Priority = 23
                Case "Local7"
                    Priority = 24
            End Select

            Priority = Priority * 8 + TheLevel

            Dim udp As UdpClient
            udp = New UdpClient(HostName, Integer.Parse(Port))
            Dim rawMsg() As Byte = ASCIIEncoding.ASCII.GetBytes("<" & Priority.ToString & "> " & Message)
            udp.Send(rawMsg, rawMsg.Length)
            udp.Close()
            dt.Rows.Add("Success")

            Return Me.GenerateActivityResult(dt)
        End Function


    End Class
End Namespace

