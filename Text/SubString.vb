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
            If IsNumeric(TheValue3) = False Then Throw New Exception("Start position must be numeric")
            If String.IsNullOrEmpty(TheValue2) Or IsNumeric(TheValue2) = False Then Throw New Exception("Length must be numeric")

            If val(TheValue3) = 0 Then Throw New Exception("Start position cannot be zero")
            If String.IsNullOrEmpty(TheValue2) Or val(TheValue2) = 0 Then Throw New Exception("Length cannot be zero")
            If val(TheValue3) > len(TheValue) Then Throw New Exception("Start position does not exist in the source string")
            If val(TheValue3) + val(TheValue2) - 1 > len(TheValue) Then TheValue2 = Nothing ' throw new exception("Length value exceeds the source string's length")
            If String.IsNullOrEmpty(TheValue2) Then
                dt.Rows.Add(TheValue.Substring(val(TheValue3) - 1))
            Else
                dt.Rows.Add(TheValue.Substring(val(TheValue3) - 1, val(TheValue2)))
            End If
            Return Me.GenerateActivityResult(dt)
        End Function


    End Class
End Namespace

