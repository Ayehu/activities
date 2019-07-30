Imports Ayehu.Sdk.ActivityCreation.Interfaces
Imports Ayehu.Sdk.ActivityCreation.Extension
Imports System.Text
Imports System
Imports System.Xml
Imports System.Data
Imports System.IO
Imports System.Collections.Generic
Imports Microsoft.VisualBasic
Imports System.Management

Namespace Ayehu.Sdk.ActivityCreation
    Public Class ActivityClass
        Implements IActivity


        Public ThePath As String

        Public Function Execute() As ICustomActivityResult Implements IActivity.Execute



            If String.IsNullOrEmpty(ThePath) Then Throw New Exception("File name cannot be empty")
            Dim dt As DataTable = New DataTable("resultSet")
            dt.Columns.Add("Result", GetType(String))


            Dim Backslash As Boolean = False
            If ThePath.Contains("/") Then
                Backslash = True
                ThePath = ThePath.Replace("/", "\")
            End If
            Dim NewPath As String = System.IO.Path.GetExtension(ThePath)
            If NewPath.StartsWith(".") Then NewPath = mid(NewPath, 2, len(NewPath))
            If Backslash = True Then NewPath = NewPath.Replace("\", "/")
            dt.Rows.Add(NewPath)


            Return Me.GenerateActivityResult(dt)

        End Function


    End Class
End Namespace

