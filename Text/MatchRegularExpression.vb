Imports Ayehu.Sdk.ActivityCreation.Interfaces
Imports Ayehu.Sdk.ActivityCreation.Extension
Imports System.Text
Imports System
Imports System.Xml
Imports System.Data
Imports System.IO
Imports System.Collections.Generic
Imports Microsoft.VisualBasic
Imports System.Text.RegularExpressions

Namespace Ayehu.Sdk.ActivityCreation
    Public Class ActivityClass
        Implements IActivity


        Public MatchText As String
        Public MatchFormula As String

        Public Function Execute() As ICustomActivityResult Implements IActivity.Execute


            Dim dt As DataTable = New DataTable("resultSet")
            dt.Columns.Add("Result", GetType(String))
            If String.IsNullOrEmpty(MatchText) Then Throw New Exception("Text to match is empty")
            If String.IsNullOrEmpty(MatchFormula) Then Throw New Exception("Regular expression is empty")
            Dim MyMatchCollection As MatchCollection = Regex.Matches(MatchText, MatchFormula)
            For Each Item As Match In MyMatchCollection
                dt.Rows.Add(Item.Value)
            Next
            If dt.Rows.Count = 0 Then dt.Rows.Add("No matching")


            Return Me.GenerateActivityResult(dt)

        End Function


    End Class
End Namespace

