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


        Public SrcHostName As String
        Public SrcUserName As String
        Public SrcPassword As String
        Public SrcPath As String
        Public DstHostName As String
        Public DstUserName As String
        Public DstPassword As String
        Public DstPath As String

        Public Function Execute() As ICustomActivityResult Implements IActivity.Execute


            Dim dt As DataTable = New DataTable("resultSet")
            dt.Columns.Add("Result", GetType(String))

            If File.Exists(SrcPath) = False Then Throw New Exception("File not found")

            File.Copy(SrcPath, DstPath, True)
            Dim hidden As Boolean = False
            Dim tempf As New FileInfo(SrcPath)
            If tempf.Attributes = FileAttributes.Hidden Then hidden = True


            If File.Exists(DstPath) Then
                If GetFileSize(SrcPath) = GetFileSize(DstPath) Then
                    Dim tempf2 As New FileInfo(SrcPath)
                    If hidden = True Then tempf2.Attributes = FileAttributes.Hidden
                    dt.Rows.Add("Success")
                Else
                    Throw New Exception("Failure")
                End If
            Else
                Throw New Exception("Failure")
            End If


            Return Me.GenerateActivityResult(dt)

        End Function

        Private Function GetFileSize(ByVal MyFilePath As String) As Long
            Dim MyFile As New FileInfo(MyFilePath)
            Dim FileSize As Long = MyFile.Length
            Return FileSize
        End Function




    End Class
End Namespace

