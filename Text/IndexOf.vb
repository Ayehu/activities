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


        Public TheValue As String
        Public SearchString As String
        Public StartIndex As Integer

        Public Function Execute() As ICustomActivityResult Implements IActivity.Execute


            Dim dt As DataTable = New DataTable("resultSet")
            Dim Rs As Integer = 0
            dt.Columns.Add("Result", GetType(Integer))


            If String.IsNullOrEmpty(TheValue) Then Throw New Exception("String is empty")
            If String.IsNullOrEmpty(SearchString) Then Throw New Exception("Search string is empty")
            If StartIndex > len(TheValue) Then Throw New Exception("Start index is greater than the string length")

            Rs = TheValue.IndexOf(SearchString, StartIndex)
            If Rs = -1 Then
                Throw New Exception("String was not found")
            Else
                Rs = Rs + 1
                dt.Rows.Add(Rs.ToString)
            End If


            Return Me.GenerateActivityResult(dt)
        End Function


    End Class
End Namespace

