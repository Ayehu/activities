Imports Ayehu.Sdk.ActivityCreation.Interfaces
Imports Ayehu.Sdk.ActivityCreation.Extension
Imports System.Text
Imports System
Imports System.Xml
Imports System.Data
Imports System.IO
Imports System.Collections.Generic
Imports Microsoft.VisualBasic
Imports System.Diagnostics

Namespace Ayehu.Sdk.ActivityCreation
    Public Class ActivityClass
        Implements IActivity


        Public HostName As String
        Public UserName As String
        Public Password As String

        Public Function Execute() As ICustomActivityResult Implements IActivity.Execute


            Dim sw As New StringWriter
            Dim dt As DataTable = New DataTable("resultSet")
            dt.Columns.Add("Result", GetType(String))
            Dim Domain As String = ""


            Dim myProcess As Process = New Process()
            myProcess.StartInfo.FileName = "cmd.exe"
            myProcess.StartInfo.Arguments = "/c IISRESET " & HostName & " /Start"

            If InStr(UserName, "\") Then
                Domain = Mid(UserName, 1, UserName.IndexOf("\"))
                UserName = Mid(UserName, UserName.IndexOf("\") + 2, Len(UserName))
            End If

            If Domain <> "" Then myProcess.StartInfo.Domain = Domain
            myProcess.StartInfo.UserName = UserName
            Dim sstr As New System.Security.SecureString
            If Password <> "" AndAlso UserName <> "" Then
                Dim pwd As String = Password
                Dim chars() As Char = pwd.ToCharArray()
                Dim i As Integer
                For i = 0 To chars.Length - 1
                    sstr.AppendChar(chars(i))
                Next
                myProcess.StartInfo.Password = sstr
            End If
            myProcess.StartInfo.UseShellExecute = False
            myProcess.StartInfo.CreateNoWindow = True
            myProcess.StartInfo.RedirectStandardInput = True
            myProcess.StartInfo.RedirectStandardOutput = True
            myProcess.StartInfo.RedirectStandardError = True
            myProcess.Start()

            Dim sIn As IO.StreamWriter = myProcess.StandardInput
            Dim sOut As IO.StreamReader = myProcess.StandardOutput

            sIn.AutoFlush = True
            sIn.Write("exit" & System.Environment.NewLine)
            Dim Result As String = sOut.ReadToEnd
            myProcess.WaitForExit(60000)
            sIn.Close()
            sOut.Close()
            myProcess.Close()

            If InStr(Result, "successfully started") Then
                dt.Rows.Add("Success")
            Else
                Throw New Exception("Failure")
            End If


            Return Me.GenerateActivityResult(dt)


        End Function


    End Class
End Namespace

