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
        Public TheValue2 As String
        Public TheValue3 As String

        Public Function Execute() As ICustomActivityResult Implements IActivity.Execute


            Dim dt As DataTable = New DataTable("resultSet")
            dt.Columns.Add("Result", GetType(String))

            If String.IsNullOrEmpty(TheValue) Then Throw New Exception("Source string is empty")
            If String.IsNullOrEmpty(TheValue3) Then Throw New Exception("Start text is empty")
            If String.IsNullOrEmpty(TheValue2) Then Throw New Exception("End text is empty")

            Dim Startstring As String = lcase(TheValue3)
            Dim Endstring As String = lcase(TheValue2)
            Dim a As String = lcase(TheValue)
            Dim Collect As Boolean = False
            Dim Data As New List(Of String)
            Dim Value As String



            For x As Integer = 1 To Len(a)
                If Mid(a, x, Len(Startstring)) = Startstring Then Collect = True
                If Mid(a, x, Len(Endstring)) = Endstring AndAlso String.IsNullOrEmpty(Value) = False Then
                    Collect = False
                    Data.Add(Value)
                    Value = ""
                End If

                If Collect = True Then Value = Value & Mid(TheValue, x, 1)
            Next

            If Data.Count = 0 Then Throw New Exception("No result")
            For x As Integer = 0 To Data.Count - 1
                If String.IsNullOrEmpty(Data(x)) Then Continue For
                If Data(x).StartsWith(Startstring) Then Data(x) = Mid(Data(x), Len(Startstring) + 1)
                If Data(x).EndsWith(Endstring) Then Data(x) = Mid(Data(x), 1, Len(Data(x) - Len(Endstring)))
                dt.Rows.Add(Data(x))
            Next


            Return Me.GenerateActivityResult(dt)
        End Function


    End Class
End Namespace

