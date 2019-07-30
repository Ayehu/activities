Imports Ayehu.Sdk.ActivityCreation.Interfaces
Imports Ayehu.Sdk.ActivityCreation.Extension
Imports System.Text
Imports System
Imports System.Xml
Imports System.Data
Imports System.IO
Imports System.Collections.Generic
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic

Namespace Ayehu.Sdk.ActivityCreation
    Public Class ActivityClass
        Implements IActivity


        Public TheValue As String
        Public ColumnsSelection As Integer
        Public ColumnsDelimiterText As String
        Public ColumnsDelimiter As String
        Public ColumnsDelimiterSelection As String
        Public ColumnsRowNumber As Integer
        Public RowsSelection As Integer
        Public RowsDelimiterText As String
        Public RowsDelimiterSelection As String
        Public RowsDelimiter As String

        Public Function Execute() As ICustomActivityResult Implements IActivity.Execute


            Dim dt As DataTable = New DataTable("resultSet")

            If String.IsNullOrEmpty(TheValue) Then Throw New Exception("Variable data is not of string type")

            If ColumnsSelection = 0 Then
                ColumnsDelimiter = ColumnsDelimiterText
            Else
                ColumnsDelimiter = ColumnsDelimiterSelection
            End If

            If RowsSelection = 0 Then
                RowsDelimiter = RowsDelimiterText
            Else
                RowsDelimiter = RowsDelimiterSelection
            End If

            ' Remove double spaces.
            If ColumnsDelimiter.Equals("TAB", StringComparison.InvariantCultureIgnoreCase) Then
                ColumnsDelimiter = "TAB"
                Do While TheValue.IndexOf("  ") <> -1
                    TheValue = TheValue.Replace("  ", " ")
                Loop
            End If

            Dim Myarray As String() = _splitTextByDelimiter(RowsSelection, RowsDelimiter, TheValue)

            If ColumnsRowNumber = 0 Then
                Dim Columns As Array = _splitTextByDelimiter(ColumnsSelection, ColumnsDelimiter, Myarray(0))
                If ColumnsDelimiter = "TAB" And Columns.Length = 1 Then Columns = Split(Myarray(0), "   ")
                Dim a As Integer = 1
                For Each ColumnName As String In Columns
                    dt.Columns.Add("Column" & a.ToString)
                    a = a + 1
                Next
            Else
                ColumnsRowNumber = Math.Min(Myarray.Length, ColumnsRowNumber)

                Dim Columns As Array = _splitTextByDelimiter(ColumnsSelection, ColumnsDelimiter, Myarray(ColumnsRowNumber - 1))
                If ColumnsDelimiter = "TAB" And Columns.Length = 1 Then Columns = Split(Myarray(ColumnsRowNumber - 1), "   ")
                Dim a As Integer = 2
                For Each ColumnName As String In Columns
                    If dt.Columns.Contains(ColumnName) = False Then
                        dt.Columns.Add(ColumnName)
                    Else
                        dt.Columns.Add(ColumnName & a.ToString)
                        a = a + 1
                    End If
                Next
            End If

            For x As Integer = ColumnsRowNumber To Myarray.Length - 1
                If Myarray(x) <> "" Then
                    Dim MyLine = _splitTextByDelimiter(ColumnsSelection, ColumnsDelimiter, Myarray(x))
                    If ColumnsDelimiter = "TAB" And MyLine.Length = 1 Then MyLine = Split(Myarray(x), "   ")
                    Dim NewRow As DataRow = dt.NewRow
                    For y As Integer = 0 To MyLine.Length - 1
                        If y >= dt.Columns.Count - 1 Then
                            NewRow(dt.Columns.Count - 1) = NewRow(dt.Columns.Count - 1) & " " & MyLine(y)
                        Else
                            NewRow(y) = MyLine(y)
                        End If

                    Next
                    dt.Rows.Add(NewRow)
                End If
            Next

            Return Me.GenerateActivityResult(dt)
        End Function

        Private Function _splitTextByDelimiter(selection As Integer, delimiter As String, source As String) As String()
            Dim returnValue = New String() {}

            If selection = 0 Then
                returnValue = Split(source, delimiter)
            Else
                Select Case LCase(delimiter)
                    Case "enter"
                        Dim pattern As String = Environment.NewLine & "|" & vbCrLf
                        returnValue = Regex.Split(source, pattern)
                    Case "space"
                        returnValue = source.Split(New Char() {" ", Chr(32)}, StringSplitOptions.RemoveEmptyEntries)
                    Case "carriage return"
                        returnValue = source.Split(New Char() {Chr(13)})
                    Case "line feed"
                        returnValue = source.Split(New Char() {Chr(10)})
                    Case "tab"
                        returnValue = source.Split(New Char() {"	", Chr(9)}, StringSplitOptions.RemoveEmptyEntries)
                End Select
            End If

            Return returnValue
        End Function


    End Class
End Namespace

