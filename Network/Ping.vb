Imports Ayehu.Sdk.ActivityCreation.Interfaces
Imports Ayehu.Sdk.ActivityCreation.Extension
Imports System.Text
Imports System
Imports System.Xml
Imports System.Data
Imports System.IO
Imports System.Collections.Generic
Imports Microsoft.VisualBasic

Namespace Ayehu.Sdk.ActivityCreation
    Public Class ActivityClass
        Implements IActivity


        Public HostName As String
        Public Timeout As Integer

        Public Function Execute() As ICustomActivityResult Implements IActivity.Execute

            Dim dt As DataTable = New DataTable("resultSet")
            dt.Columns.Add("Result", GetType(String))

            Try
                If My.Computer.Network.Ping(HostName, Timeout) Then
                    dt.Rows.Add("Success")
                Else
                    If My.Computer.Network.Ping(HostName, Timeout) Then
                        dt.Rows.Add("Success")
                    Else
                        Throw New Exception("Failure")
                    End If
                End If
            Catch
                dt.Rows.Add("Failure")
            End Try
            Return Me.GenerateActivityResult(dt)
        End Function


    End Class
End Namespace

