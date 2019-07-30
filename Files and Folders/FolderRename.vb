Imports Ayehu.Sdk.ActivityCreation.Interfaces
Imports Ayehu.Sdk.ActivityCreation.Extension
Imports System.Text
Imports System
Imports System.Xml
Imports System.Data
Imports System.IO
Imports System.Management
Imports System.Collections.Generic
Imports ActivitiesUtilsLib
Imports Microsoft.VisualBasic

Namespace Ayehu.Sdk.ActivityCreation
    Public Class ActivityClass
        Implements IActivity


        Public HostName As String
        Public UserName As String
        Public Password As String
        Public Path As String
        Public FolderName As String
        Public NewFolderName As String

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

            If ActivitiesUtils.IsLocalhost(HostName) Then
                oms = New ManagementScope("\\.\root\cimv2")
            Else
                oms = New ManagementScope("\\" & HostName & "\root\cimv2", connectionOptions)
            End If
            If Path.EndsWith("\") = False Then
                Path += "\"
            End If
            Dim Volume As String = System.IO.Path.GetPathRoot(Path)
            Dim PathOnly As String = System.IO.Path.GetDirectoryName(Path)
            If PathOnly = Nothing Then PathOnly = Path
            Dim PathOnlyNoVolume As String = PathOnly
            PathOnlyNoVolume = PathOnlyNoVolume.Replace(Volume, "")
            PathOnlyNoVolume = "\\" & PathOnlyNoVolume.Replace("\", "\\") & "\\"
            PathOnlyNoVolume = PathOnlyNoVolume.Replace("\\\\", "\\")
            PathOnlyNoVolume = PathOnlyNoVolume.Replace("\\\\", "\\")
            Dim Fnd As Boolean = False

            If Path.StartsWith("\\") Then


                Dim RemotePath As New DirectoryInfo(PathOnly)

                If Directory.Exists(Path) Then
                    Fnd = True
                    Directory.Move(System.IO.Path.Combine(Path, FolderName), System.IO.Path.Combine(Path, NewFolderName))
                End If


                If Fnd = True Then
                    dt.Rows.Add("Success")
                Else
                    Throw New Exception("Folder not found")
                End If
            Else


                Dim errResult As Integer = 0
                FolderName = System.IO.Path.Combine(PathOnly, FolderName)
                FolderName = FolderName.GetQueryPath()
                Dim oQuery As ObjectQuery = New System.Management.ObjectQuery("SELECT Name FROM Win32_Directory WHERE Name = '" + FolderName + "'")
                Dim oSearcher As ManagementObjectSearcher = New ManagementObjectSearcher(oms, oQuery)

                Dim RenamePath() As Object = {Path & NewFolderName}
                For Each oReturn As ManagementObject In oSearcher.Get()
                    Fnd = True
                    errResult = oReturn.InvokeMethod("Rename", RenamePath)
                Next
                If Fnd = False Then
                    Throw New Exception("Folder not found")
                Else
                    Select Case errResult
                        Case 0
                            dt.Rows.Add("Success")
                        Case 2
                            Throw New Exception("Access denied.")
                        Case 8
                            Throw New Exception("Unspecified failure.")
                        Case 9
                            Throw New Exception("Invalid object.")
                        Case 10
                            Throw New Exception("Object already exists.")
                        Case 11
                            Throw New Exception("File system not NTFS.")
                        Case 12
                            Throw New Exception("Platform not Windows NT or Windows 2000.")
                        Case 13
                            Throw New Exception("Drive not the same.")
                        Case 14
                            Throw New Exception("Directory not empty.")
                        Case 15
                            Throw New Exception("Sharing violation.")
                        Case 16
                            Throw New Exception("Invalid start file.")
                        Case 17
                            Throw New Exception("Privilege not held.")
                        Case 21
                            Throw New Exception("Invalid parameter.")
                        Case Else
                            Throw New Exception("Failure")
                    End Select
                End If
            End If

            Return Me.GenerateActivityResult(dt)

        End Function



    End Class
End Namespace

