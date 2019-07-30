Imports Ayehu.Sdk.ActivityCreation.Interfaces
Imports Ayehu.Sdk.ActivityCreation.Extension
Imports System.Text
Imports System
Imports System.Xml
Imports System.Data
Imports System.IO
Imports System.Collections.Generic
Imports System.Management
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic
Imports ActivitiesUtilsLib

Namespace Ayehu.Sdk.ActivityCreation
    Public Class ActivityClass
        Implements IActivity


        Public HostName As String
        Public UserName As String
        Public Password As String
        Public SSH As Object
        Public TimeInSeconds As String
        Public Port As Integer
        Public SSHCertificate As String

        Public Function Execute() As ICustomActivityResult Implements IActivity.Execute


            Dim dt As DataTable = New DataTable("resultSet")
            dt.Columns.Add("Result", GetType(String))

            Dim MyCommand = "cat /etc/*-release | uniq"
            Dim Result As String = ActivitiesUtils.GetUnixManagementScope(UserName, Password, HostName, TimeInSeconds, Port, SSHCertificate, MyCommand)
            Dim MyArray As Array = Split(Result, vbCrLf)

            For Each line As String In MyArray
                If Trim(line) <> "" Then dt.Rows.Add(Result)
            Next

            Return Me.GenerateActivityResult(dt)

        End Function

    End Class
End Namespace

