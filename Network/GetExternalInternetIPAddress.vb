Imports Ayehu.Sdk.ActivityCreation.Interfaces
Imports Ayehu.Sdk.ActivityCreation.Extension
Imports System.Text
Imports System
Imports System.Xml
Imports System.Data
Imports System.IO
Imports System.Collections.Generic
Imports Microsoft.VisualBasic
Imports System.Net
Imports System.Text.RegularExpressions

Namespace Ayehu.Sdk.ActivityCreation
    Public Class ActivityClass
        Implements IActivity


        Public TheValue As String

        Public Function Execute() As ICustomActivityResult Implements IActivity.Execute


            Dim dt As DataTable = New DataTable("resultSet")
            dt.Columns.Add("Result", GetType(String))
            Dim req As HttpWebRequest = WebRequest.Create(TheValue)
            Dim res As HttpWebResponse = req.GetResponse()
            Dim Stream As Stream = res.GetResponseStream()
            Dim sr As StreamReader = New StreamReader(Stream)
            Dim Ips As String = sr.ReadToEnd()
            Dim MyMatchCollection As MatchCollection = Regex.Matches(Ips, "(?<First>2[0-4]\d|25[0-5]|[01]?\d\d?)\.(?<Second>2[0-4]\d|25[0-5]|[01]?\d\d?)\.(?<Third>2[0-4]\d|25[0-5]|[01]?\d\d?)\.(?<Fourth>2[0-4]\d|25[0-5]|[01]?\d\d?)")
            For Each Item As Match In MyMatchCollection
                If Item.Value.Contains("0.0") Then Continue For
                dt.Rows.Add(Item.Value)
                Exit For
            Next

            Return Me.GenerateActivityResult(dt)
        End Function


    End Class
End Namespace

