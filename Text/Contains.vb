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

        Public Function Execute() As ICustomActivityResult Implements IActivity.Execute


            Dim dt As DataTable = New DataTable("resultSet")
            dt.Columns.Add("Result", GetType(String))


            If TheValue2 IsNot DBNull.Value AndAlso TheValue IsNot DBNull.Value AndAlso String.IsNullOrEmpty(TheValue2) = False AndAlso String.IsNullOrEmpty(TheValue) = False Then

                Try
                    Dim sr As StringReader = New StringReader(TheValue)
                    Dim resultTBL As DataTable = New DataTable()
                    resultTBL.ReadXml(sr)
                    Dim fnd As Boolean = False
                    If resultTBL.Rows.Count > 0 Then
                        Dim CL As Integer = resultTBL.Columns.Count - 1
                        For Each Row As DataRow In resultTBL.Rows
                            If dt.rows.count > 0 Then Exit For
                            For x As Integer = 0 To CL
                                If lcase(Row(x).ToString).Contains(lcase(TheValue2)) Then
                                    dt.Rows.Add("True")
                                    fnd = True
                                    Exit For
                                End If
                            Next
                        Next
                        If fnd = False Then dt.Rows.Add("False")
                    Else
                        Throw New Exception("not resultset")
                    End If
                Catch
                    If lcase(TheValue).contains(lcase(TheValue2)) Then
                        dt.Rows.Add("True")
                    Else
                        dt.Rows.Add("False")
                    End If
                End Try




            Else
                dt.Rows.Add("False")
            End If
            Return Me.GenerateActivityResult(dt)
        End Function



    End Class
End Namespace

