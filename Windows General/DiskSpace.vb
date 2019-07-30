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
        Public SelectedDeviceStatus As String
        Public UserName As String
        Public Password As String
        Public Path As String

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

            If LCase(HostName) = "localhost" OrElse HostName = "127.0.0.1" Then
                oms = New ManagementScope("\\.\root\cimv2")
            Else
                oms = New ManagementScope("\\" & HostName & "\root\cimv2", connectionOptions)
            End If
            If Path.Length = 1 Then Path = Path & ":"
            Dim Fnd As Boolean = False
            If SelectedDeviceStatus = "Available" Then
                Try
                    Dim oQuery As ObjectQuery = New System.Management.ObjectQuery("Select FreeSpace from Win32_LogicalDisk Where DeviceID = '" & Path & "'")
                    Dim oSearcher As ManagementObjectSearcher = New ManagementObjectSearcher(oms, oQuery)
                    For Each oReturn As ManagementObject In oSearcher.Get()
                        Fnd = True
                        dt.Rows.Add(Math.Round(val(oReturn("FreeSpace")) / 1048576, 2))
                    Next
                Catch ex As Exception
                    If (ex.Message = "Value does not fall within the expected range.") Then
                        Throw New Exception("Unknown host")
                    Else
                        Throw New Exception(ex.Message.Split("(")(0))
                    End If
                End Try
                If Fnd = False Then Throw New Exception("No such volume")
            ElseIf SelectedDeviceStatus = "Usage" Then

                Try
                    Dim oQuery As ObjectQuery = New System.Management.ObjectQuery("Select Size , FreeSpace from Win32_LogicalDisk Where DeviceID = '" & Path & "'")
                    Dim oSearcher As ManagementObjectSearcher = New ManagementObjectSearcher(oms, oQuery)
                    For Each oReturn As ManagementObject In oSearcher.Get()
                        Fnd = True
                        dt.Rows.Add(Math.Round((val(oReturn("Size")) - val(oReturn("FreeSpace"))) / 1048576, 2))
                    Next
                Catch ex As Exception
                    If (ex.Message = "Value does not fall within the expected range.") Then
                        Throw New Exception("Unknown host")
                    Else
                        Throw New Exception(ex.Message.Split("(")(0))
                    End If
                End Try
                If Fnd = False Then Throw New Exception("No such volume")
            ElseIf SelectedDeviceStatus = "Available %" Then

                Try
                    Dim oQuery As ObjectQuery = New System.Management.ObjectQuery("Select Size, FreeSpace from Win32_LogicalDisk Where DeviceID = '" & Path & "'")
                    Dim oSearcher As ManagementObjectSearcher = New ManagementObjectSearcher(oms, oQuery)
                    For Each oReturn As ManagementObject In oSearcher.Get()
                        Fnd = True
                        dt.Rows.Add((Math.Round((val(oReturn("FreeSpace"))) / (val(oReturn("Size"))), 3)) * 100)
                    Next
                Catch ex As Exception
                    If (ex.Message = "Value does not fall within the expected range.") Then
                        Throw New Exception("Unknown host")
                    Else
                        Throw New Exception(ex.Message.Split("(")(0))
                    End If
                End Try
                If Fnd = False Then Throw New Exception("No such volume")
            Else
                Try
                    Dim oQuery As ObjectQuery = New System.Management.ObjectQuery("Select Size, FreeSpace from Win32_LogicalDisk Where DeviceID = '" & Path & "'")
                    Dim oSearcher As ManagementObjectSearcher = New ManagementObjectSearcher(oms, oQuery)
                    For Each oReturn As ManagementObject In oSearcher.Get()
                        Fnd = True
                        dt.Rows.Add((Math.Round((((val(oReturn("Size"))) - (val(oReturn("FreeSpace")))) / (val(oReturn("Size")))), 3)) * 100)
                    Next
                Catch ex As Exception
                    If (ex.Message = "Value does not fall within the expected range.") Then
                        Throw New Exception("Unknown host")
                    Else
                        Throw New Exception(ex.Message.Split("(")(0))
                    End If
                End Try
                If Fnd = False Then Throw New Exception("No such volume")
            End If


            Return Me.GenerateActivityResult(dt)

        End Function


    End Class
End Namespace

