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
Imports System.DirectoryServices
Imports System.Net

Namespace Ayehu.Sdk.ActivityCreation
    Public Class ActivityClass
        Implements IActivity


        Public HostName As String
        Public UserName As String
        Public Password As String

        Public Function Execute() As ICustomActivityResult Implements IActivity.Execute


            Dim dt As DataTable = New DataTable("resultSet")
            dt.Columns.Add("Result", GetType(String))
            Dim Domain As String = ""

            Try


                Dim connectionOptions As ConnectionOptions = New ConnectionOptions
                connectionOptions.Username = UserName
                connectionOptions.Password = Password
                connectionOptions.Authentication = AuthenticationLevel.PacketPrivacy
                connectionOptions.Impersonation = ImpersonationLevel.Impersonate
                connectionOptions.EnablePrivileges = True
                Dim oMs As ManagementScope
                If LCase(HostName) = "localhost" OrElse HostName = "127.0.0.1" Then
                    oMs = New ManagementScope("\\.\root\cimv2")
                Else
                    oMs = New ManagementScope("\\" & HostName & "\root\cimv2", connectionOptions)
                End If

                Dim oQuery As ObjectQuery = New System.Management.ObjectQuery("SELECT Name FROM CIM_DataFile WHERE Drive = '" & "C:" & "' and path = '" & "\\" & "'")
                Dim oSearcher As ManagementObjectSearcher = New ManagementObjectSearcher(oMs, oQuery)
                For Each oReturn As ManagementObject In oSearcher.Get()
                    dt.Rows.Add("Success")
                    Exit For
                Next
            Catch
                If lcase(err.description).contains("access") Then
                    dt.rows.add("Failure")
                Else
                    Throw
                End If
            End Try

            Return Me.GenerateActivityResult(dt)

        End Function



    End Class
End Namespace

