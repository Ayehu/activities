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


        Public HostName As String
        Public UserName As String
        Public Password As String
        Public Path As String
        Public NewValue As String
        Public IsAppend As String

        Public Function Execute() As ICustomActivityResult Implements IActivity.Execute


            Dim dt As DataTable = New DataTable("resultSet")
            dt.Columns.Add("Result", GetType(String))


            Try
                dt = WriteFileRemote(HostName, UserName, Password, Path, NewValue, IsAppend)
            Catch
                Throw New Exception("Unable to save file")
            End Try


            Return Me.GenerateActivityResult(dt)

        End Function


        Function WriteFileRemote(ByVal HostName As String, ByVal UserName As String, ByVal Password As String, ByVal Path As String, ByVal Value As String, IsAppend As String) As DataTable

            Dim dtResult As New DataTable("resultSet")
            dtResult.Columns.Add("Result", GetType(String))



            If IsAppend = "False" Then
                File.WriteAllText(Path, Value)
            Else
                If File.Exists(Path) = False Then
                    Value = Value
                Else
                    Value = vbcrlf & Value
                End If

                File.AppendAllText(Path, Value)
            End If
            dtResult.Rows.Add("Success")


            Return dtResult

        End Function


    End Class
End Namespace

