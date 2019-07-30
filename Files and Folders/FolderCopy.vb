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
        Public SubFolders As Integer

        Public Function Execute() As ICustomActivityResult Implements IActivity.Execute


            Dim dt As DataTable = New DataTable("resultSet")
            dt.Columns.Add("Result", GetType(String))
            Dim IncludeSubFolder As Boolean
            Dim Hidden As Boolean = False
            If SubFolders = 1 Then IncludeSubFolder = True

            If Directory.Exists(SrcPath) = False Then Throw New Exception("Folder not found")
            If LCase(SrcPath) = LCase(DstPath) Then Throw New Exception("Unable to copy folder to its subfolder")
            If LCase(DstPath).StartsWith(LCase(SrcPath) & "\") Then Throw New Exception("Unable to copy folder to its subfolder")
            Dim dir As DirectoryInfo = New DirectoryInfo(SrcPath)
            If dir.Attributes = (FileAttributes.Hidden Or FileAttributes.Directory) Then Hidden = True
            If DirectoryCopy(SrcPath, DstPath, IncludeSubFolder, Hidden) = True Then dt.Rows.Add("Success")
            Return Me.GenerateActivityResult(dt)

        End Function


        Private Function DirectoryCopy(
        ByVal sourceDirName As String,
        ByVal destDirName As String,
        ByVal copySubDirs As Boolean, ByVal Hidden As Boolean) As Boolean

            Dim dir As DirectoryInfo = New DirectoryInfo(sourceDirName)
            Dim dirs As DirectoryInfo() = dir.GetDirectories()

            If Not Directory.Exists(destDirName) Then
                Dim di As DirectoryInfo = Directory.CreateDirectory(destDirName)
                If Hidden = True Then di.Attributes = FileAttributes.Hidden
            End If

            Dim files As FileInfo() = dir.GetFiles()
            For Each file As FileInfo In files
                Dim temppath As String = Path.Combine(destDirName, file.Name)
                file.CopyTo(temppath, True)
                If file.Attributes = FileAttributes.Hidden Then
                    Dim tempf As New FileInfo(temppath)
                    tempf.Attributes = FileAttributes.Hidden
                End If

            Next file

            If copySubDirs Then
                For Each subdir As DirectoryInfo In dirs
                    Dim temppath As String = Path.Combine(destDirName, subdir.Name)
                    Dim dir2 As DirectoryInfo = New DirectoryInfo(subdir.FullName)
                    If dir2.Attributes = (FileAttributes.Hidden Or FileAttributes.Directory) Then
                        Hidden = True
                    Else
                        Hidden = False
                    End If

                    DirectoryCopy(subdir.FullName, temppath, copySubDirs, Hidden)
                Next subdir
            End If

            Return True
        End Function


    End Class
End Namespace

