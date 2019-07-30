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
            Dim user As String = String.Empty
            dt.Columns.Add("PID", GetType(String))
            dt.Columns.Add("Owner", GetType(String))


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
            If lcase(ProcessName).Contains(".exe") AndAlso LCase(ProcessName).EndsWith(".exe") Then ProcessName = mid(ProcessName, 1, len(ProcessName) - 4)
            If ProcessName.StartsWith("*") OrElse ProcessName.EndsWith("*") Then
                ProcessName = ProcessName.Replace("*", "")
                HasWildCard = True
            End If
            Dim oQuery As ObjectQuery = New System.Management.ObjectQuery("Select * from Win32_Process where name LIKE '%" & ProcessName & "%' OR name LIKE '%" & ProcessName & ".exe%'")
            Dim oSearcher As ManagementObjectSearcher = New ManagementObjectSearcher(oms, oQuery)
            For Each oReturn As ManagementObject In oSearcher.Get()
                Dim o(2) As Object

                If HasWildCard = True Then
                    oReturn.InvokeMethod("GetOwner", o)
                    user = o(0).ToString
                    If String.IsNullOrEmpty(user) = False Then
                        dt.Rows.Add(oReturn.GetPropertyValue("Handle"), user.ToString)
                    End If
                Else
                    If LCase(oReturn("Name")) = LCase(ProcessName) OrElse LCase(oReturn("Name")) = LCase(ProcessName) & ".exe" OrElse LCase(oReturn("Name")).Contains(LCase(ProcessName) & "#") OrElse LCase(oReturn("Name")).Contains(LCase(ProcessName) & ".exe#") Then
                        oReturn.InvokeMethod("GetOwner", o)
                        user = o(0).ToString
                        If String.IsNullOrEmpty(user) = False Then
                            dt.Rows.Add(oReturn.GetPropertyValue("Handle"), user.ToString)
                        End If
                    End If
                End If




            Next
            If dt.rows.count = 0 Then Throw New Exception("Process does not exist")
            Return Me.GenerateActivityResult(dt)

        End Function


    End Class
End Namespace

