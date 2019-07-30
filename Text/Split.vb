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
        Public SelectionType As Integer
        Public SelectedKey As String

        Public Function Execute() As ICustomActivityResult Implements IActivity.Execute


            Dim dt As DataTable = New DataTable("resultSet")
            dt.Columns.Add("Result", GetType(String))

            If String.IsNullOrEmpty(TheValue) Then Throw New Exception("String is empty")
            If SelectionType = 1 Or SelectionType = 0 Then
                Dim Myarray As Array = split(TheValue, TheValue2)
                For Each Item As String In Myarray
                    dt.Rows.Add(Item)
                Next
            Else
                Dim Myarray As Array
                Select Case SelectedKey
                    Case "Enter"
                        Myarray = split(TheValue, VBCRLF)
                    Case "Tab"
                        Myarray = split(TheValue, chr(9))
                    Case "Space"
                        Myarray = split(TheValue, " ")
                    Case "Carriage Return"
                        Myarray = split(TheValue, chr(13))
                    Case "Line Feed"
                        Myarray = split(TheValue, chr(10))
                End Select

                For Each Item As String In Myarray
                    dt.Rows.Add(Item)
                Next

            End If

            Return Me.GenerateActivityResult(dt)
        End Function


    End Class
End Namespace

