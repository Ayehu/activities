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
        Public LinesToRead As String
        Public LineFilter As String
        Public ReadingDirection As String

        Public Function Execute() As ICustomActivityResult Implements IActivity.Execute


            Dim dt As DataTable = New DataTable("resultSet")
            dt.Columns.Add("Result", GetType(String))

            If File.Exists(Path) = False Then Throw New Exception("File not found")
            dt = ReadFile(HostName, UserName, Password, Path, LinesToRead, LineFilter, ReadingDirection)

            Return Me.GenerateActivityResult(dt)

        End Function

        Public Function GetFileContents(ByVal FullPath As String, Optional ByRef ErrInfo As String = "") As String

            ' This specific used for thr remote execution procedure
            Dim strContents As String
            Dim objReader As StreamReader

            objReader = New StreamReader(FullPath)
            strContents = objReader.ReadToEnd()
            objReader.Close()
            Return strContents


        End Function

        Public Function ParseFileContent(ByVal Content As String, ByVal LinesToRead As Integer, ByVal SearchPhrase As String, ByVal ReadFrom As String) As String

            ' This specific is used for the read file procedures
            Dim strContents As String = ""
            Dim min As Integer = 0
            SearchPhrase = LCase(SearchPhrase)
            Dim FileRead As Array

            If ((LinesToRead = Nothing) OrElse (LinesToRead = 0)) And SearchPhrase = "" Then strContents = Content
            If ((LinesToRead = Nothing) OrElse (LinesToRead = 0)) And SearchPhrase <> "" Then
                Select Case ReadFrom
                    Case "Top"
                        FileRead = Split(Content, vbCrLf)
                        For Each line As String In FileRead
                            If LCase(line).Contains(LCase(SearchPhrase)) = True Then
                                strContents = strContents & line & vbCrLf
                            End If
                        Next

                    Case "Bottom"
                        FileRead = Split(Content, vbCrLf)
                        For Each line As String In FileRead
                            If LCase(line).Contains(LCase(SearchPhrase)) = True Then
                                strContents = strContents & line & vbCrLf
                            End If
                        Next
                End Select
            End If

            If LinesToRead > 0 And SearchPhrase = "" Then
                Select Case ReadFrom
                    Case "Top"
                        FileRead = Split(Content, vbCrLf)
                        If FileRead.Length > LinesToRead Then
                            min = LinesToRead
                        Else
                            min = FileRead.Length
                        End If
                        Dim destArr(min - 1) As String
                        Array.Copy(FileRead, destArr, Math.Min(LinesToRead, FileRead.Length)) 'LinesToRead)
                        strContents = Join(destArr, vbCrLf)
                    Case "Bottom"
                        FileRead = Split(Content, vbCrLf)
                        If FileRead.Length > LinesToRead Then
                            min = LinesToRead
                        Else
                            min = FileRead.Length
                        End If
                        Dim destArr(min - 1) As String
                        Array.Copy(FileRead, FileRead.Length - Math.Min(LinesToRead, FileRead.Length), destArr, 0, Math.Min(LinesToRead, FileRead.Length))
                        strContents = Join(destArr, vbCrLf)
                End Select
            End If

            If LinesToRead > 0 And SearchPhrase <> "" Then
                Select Case ReadFrom
                    Case "Top"
                        FileRead = Split(Content, vbCrLf)
                        If FileRead.Length > LinesToRead Then
                            min = LinesToRead
                        Else
                            min = FileRead.Length
                        End If
                        Dim destArr(min - 1) As String
                        Array.Copy(FileRead, destArr, Math.Min(LinesToRead, FileRead.Length))
                        For Each line As String In destArr

                            If LCase(line).Contains(LCase(SearchPhrase)) = True Then
                                strContents = strContents & line & vbCrLf
                            End If
                        Next
                    Case "Bottom"
                        FileRead = Split(Content, vbCrLf)
                        If FileRead.Length > LinesToRead Then
                            min = LinesToRead
                        Else
                            min = FileRead.Length
                        End If
                        Dim destArr(min - 1) As String
                        Array.Copy(FileRead, FileRead.Length - Math.Min(LinesToRead, FileRead.Length), destArr, 0, Math.Min(LinesToRead, FileRead.Length))
                        For Each line As String In destArr

                            If LCase(line).Contains(LCase(SearchPhrase)) = True Then
                                strContents = strContents & line & vbCrLf
                            End If
                        Next
                End Select
            End If
            Return strContents


        End Function

        Function ReadFile(ByVal HostName As String, ByVal UserName As String, ByVal Password As String, ByVal Path As String, ByVal LinesToRead As String, ByVal LineFilter As String, ByVal ReadingDirection As String) As DataTable

            Dim dtResult As New DataTable("resultSet")
            dtResult.Columns.Add("Result", GetType(String))

            If LinesToRead = "0" OrElse Trim(LinesToRead) = "" Then LinesToRead = Nothing
            If ReadingDirection = "" Then ReadingDirection = "Top"
            Dim FileContent As String = GetFileContents(Path)
            FileContent = ParseFileContent(FileContent, LinesToRead, LineFilter, ReadingDirection)
            Dim MyArray As Array
            Dim Index As Integer = 0
            MyArray = Split(FileContent, vbCrLf)
            For Each line As String In MyArray
                Index = Index + 1
                If Index = MyArray.Length And Trim(line) = "" Then
                Else
                    dtResult.Rows.Add(line)
                End If
            Next

            Return dtResult
        End Function

    End Class
End Namespace

