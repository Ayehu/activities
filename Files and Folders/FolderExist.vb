Imports Ayehu.Sdk.ActivityCreation.Interfaces
Imports Ayehu.Sdk.ActivityCreation.Extension
Imports System.Text
Imports System
Imports System.Xml
Imports System.Data
Imports System.IO
Imports System.Management
Imports System.Collections.Generic
Imports Microsoft.VisualBasic

Namespace Ayehu.Sdk.ActivityCreation
    Public Class ActivityClass
        Implements IActivity


        Public HostName As String
        Public UserName As String
        Public Password As String
        Public Path As String

        Public Function Execute() As ICustomActivityResult Implements IActivity.Execute


            Dim dt As DataTable = New DataTable("resultSet")
            dt.Columns.Add("Result", GetType(String))
            If String.IsNullOrEmpty(Path) Then Throw New Exception("Folder not found")

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
            Dim Fnd As Boolean = False

            Dim Volume As String = System.IO.Path.GetPathRoot(Path)
            Dim FileName As String = System.IO.Path.GetFileNameWithoutExtension(Path)
            Dim FileNameExtension As String = System.IO.Path.GetExtension(Path)
            If FileNameExtension = "" Then ' recognize last work as a name of folder and not a file
                FileName = ""
                If Right(Path, 1) <> "\" Then Path = Path & "\"
            End If
            Dim PathOnly As String = System.IO.Path.GetDirectoryName(Path)
            If PathOnly = Nothing Then PathOnly = Path
            Dim PathOnlyNoVolume As String = PathOnly
            If String.IsNullOrEmpty(Volume) = False Then PathOnlyNoVolume = PathOnlyNoVolume.Replace(Volume, "")
            PathOnlyNoVolume = "\\" & PathOnlyNoVolume.Replace("\", "\\") & "\\"
            PathOnlyNoVolume = PathOnlyNoVolume.Replace("\\\\", "\\")
            PathOnlyNoVolume = PathOnlyNoVolume.Replace("\\\\", "\\")

            If Path.StartsWith("\\") Then


                Dim RemotePath As New DirectoryInfo(PathOnly)


                If Directory.Exists(Path) Then Fnd = True



                If Fnd = True Then
                    dt.Rows.Add("True")
                Else
                    dt.Rows.Add("False")
                End If
            Else


                Dim oQuery As ObjectQuery = New System.Management.ObjectQuery("SELECT Name FROM Win32_Directory WHERE Name = '" & PathOnly.Replace("\", "\\") & "'")
                Dim oSearcher As ManagementObjectSearcher = New ManagementObjectSearcher(oms, oQuery)


                For Each oReturn As ManagementObject In oSearcher.Get()
                    Fnd = True

                Next

                If Fnd = True Then
                    dt.Rows.Add("True")
                Else
                    dt.Rows.Add("False")
                End If
            End If

            Return Me.GenerateActivityResult(dt)

        End Function


    End Class
End Namespace

