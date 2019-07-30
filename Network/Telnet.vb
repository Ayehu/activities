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

Namespace Ayehu.Sdk.ActivityCreation
    Public Class ActivityClass
        Implements IActivity


        Public HostName As String
        Public Port As Integer

        Public Function Execute() As ICustomActivityResult Implements IActivity.Execute

            Dim dt As DataTable = New DataTable("resultSet")
            dt.Columns.Add("Result", GetType(String))
            Dim telnetServersend As TcpClient
            Dim telnetNetStrmsend As NetworkStream
            Dim telnetRdStrmsend As StreamReader
            Try



                telnetServersend = New TcpClient(HostName, val(Port))
                telnetNetStrmsend = telnetServersend.GetStream
                telnetRdStrmsend = New StreamReader(telnetServersend.GetStream)
                If (telnetServersend.Connected) = True Then
                    dt.Rows.Add("Success")
                Else
                    Throw New Exception("Failure")
                End If

            Catch
                dt.Rows.Add("Failure")
            End Try
            telnetServersend = Nothing
            telnetNetStrmsend = Nothing
            telnetRdStrmsend = Nothing
            Return Me.GenerateActivityResult(dt)
        End Function


    End Class
End Namespace

