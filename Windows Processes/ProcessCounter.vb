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


        Public HostName As String
        Public ProcessName As String
        Public UserName As String
        Public Password As String

        Public Function Execute() As ICustomActivityResult Implements IActivity.Execute


            Dim dt As DataTable = New DataTable("resultSet")
            dt.Columns.Add("Result", GetType(String))


            Dim connectionOptions As ConnectionOptions = New ConnectionOptions
            connectionOptions.Username = UserName
            connectionOptions.Password = Password
            connectionOptions.Authentication = AuthenticationLevel.PacketPrivacy
            connectionOptions.Impersonation = ImpersonationLevel.Impersonate
            connectionOptions.EnablePrivileges = True
            Dim oms As Management.ManagementScope
            Dim HasWildCard As Boolean = False
            If LCase(HostName) = "localhost" OrElse HostName = "127.0.0.1" Then
                oms = New ManagementScope("\\.\root\cimv2")
            Else
                oms = New ManagementScope("\\" & HostName & "\root\cimv2", connectionOptions)
            End If
            Dim Counter As Integer = 0
            If LCase(ProcessName).Contains(".exe") AndAlso LCase(ProcessName).EndsWith(".exe") Then ProcessName = Mid(ProcessName, 1, Len(ProcessName) - 4)
            If ProcessName.StartsWith("*") OrElse ProcessName.EndsWith("*") Then
                ProcessName = ProcessName.Replace("*", "")
                HasWildCard = True
            End If

            Dim oQuery As ObjectQuery = New System.Management.ObjectQuery("Select name from Win32_Process where name LIKE '%" & ProcessName & "%' or name LIKE '%" & ProcessName & ".exe%'")
            Dim oSearcher As ManagementObjectSearcher = New ManagementObjectSearcher(oms, oQuery)
            Dim Fnd As Boolean = False
            For Each oReturn As ManagementObject In oSearcher.Get()
                If HasWildCard = True Then

                    Counter = Counter + 1
                Else
                    If LCase(oReturn("Name")) = LCase(ProcessName) OrElse LCase(oReturn("Name")) = LCase(ProcessName & ".exe") OrElse LCase(oReturn("Name")).Contains(LCase(ProcessName) & "#") OrElse LCase(oReturn("Name")).Contains(LCase(ProcessName) & ".exe#") Then

                        Counter = Counter + 1
                    End If
                End If


            Next
            dt.Rows.Add(Counter.ToString)

            Return Me.GenerateActivityResult(dt)

        End Function


    End Class
End Namespace

