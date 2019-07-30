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
        Public StartValue As String
        Public EndValue As String

        Public Function Execute() As ICustomActivityResult Implements IActivity.Execute


            Dim dt As DataTable = New DataTable("resultSet")
            dt.Columns.Add("Result", GetType(String))

            If String.IsNullOrEmpty(TheValue) Then Throw New Exception("String is empty")

            If String.IsNullOrEmpty(StartValue) = False Then
                If LCase(TheValue).StartsWith(LCase(StartValue)) Then
                    TheValue = TheValue.Substring(Len(StartValue))
                End If
            End If
            If String.IsNullOrEmpty(EndValue) = False Then
                If LCase(TheValue).EndsWith(LCase(EndValue)) Then
                    Dim x As Integer = LCase(TheValue).LastIndexOf(LCase(EndValue))
                    TheValue = TheValue.Substring(0, x)
                End If
            End If
            dt.Rows.Add(TheValue)

            Return Me.GenerateActivityResult(dt)
        End Function


    End Class
End Namespace

