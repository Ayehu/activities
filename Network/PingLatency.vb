Imports Ayehu.Sdk.ActivityCreation.Interfaces
Imports Ayehu.Sdk.ActivityCreation.Extension
Imports System.Text
Imports System
Imports System.Xml
Imports System.Data
Imports System.Net.NetworkInformation
Imports System.IO
Imports System.Collections.Generic
Imports Microsoft.VisualBasic

Namespace Ayehu.Sdk.ActivityCreation
    Public Class ActivityClass
        Implements IActivity


        Public HostName As String
        Public Timeout As Integer
        Public BufferSize As String
        Public ResultType As String

        Public Function Execute() As ICustomActivityResult Implements IActivity.Execute

            Dim dt As DataTable = New DataTable("resultSet")
            dt.Columns.Add("Result", GetType(String))

            Try

                Dim pingSender As Ping = New Ping()
                Dim options As PingOptions = New PingOptions()
                options.DontFragment = True
                Dim data As String = ""
                If Val(BufferSize) > 0 Then
                    For x As Integer = 1 To Val(BufferSize)
                        data = data + "a"
                    Next
                Else
                    Throw New Exception("Buffer is empty")
                End If
                Dim buffer As Byte() = Encoding.ASCII.GetBytes(data)
                Dim reply As PingReply = pingSender.Send(HostName, Timeout * 1000, buffer, options)
                If reply.Status = IPStatus.Success Then

                    Select Case ResultType
                        Case "RoundTrip time"
                            dt.Rows.Add(reply.RoundtripTime.ToString())
                        Case "Time to live"
                            dt.Rows.Add(reply.Options.Ttl.ToString())
                        Case Else
                            dt = New DataTable("resultSet")
                            dt.Columns.Add("RoundtripTime", GetType(String))
                            dt.Columns.Add("Time to live", GetType(String))
                            Dim NewRow As DataRow = dt.newrow
                            NewRow(0) = reply.RoundtripTime.ToString
                            NewRow(1) = reply.Options.Ttl.ToString
                            dt.Rows.Add(NewRow)

                    End Select

                Else
                    Throw New Exception("Failure")
                End If



            Catch
                dt.Rows.Add("Failure")
            End Try
            Return Me.GenerateActivityResult(dt)
        End Function


    End Class
End Namespace

