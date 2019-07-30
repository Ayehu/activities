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


        Dim Counter As Double = 0
        Public HostName As String
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

            If mid(Path, len(Path), 1) <> "\" Then Path = Path & "\"

            Dim Volume As String = System.IO.Path.GetPathRoot(Path)
            Dim FileName As String = System.IO.Path.GetFileNameWithoutExtension(Path)
            Dim FileNameExtension As String = System.IO.Path.GetExtension(Path)
            Dim PathOnly As String = System.IO.Path.GetDirectoryName(Path)
            If PathOnly = Nothing Then PathOnly = Path
            Dim PathOnlyNoVolume As String = PathOnly
            PathOnlyNoVolume = PathOnlyNoVolume.Replace(Volume, "")
            PathOnlyNoVolume = "\\" & PathOnlyNoVolume.Replace("\", "\\") & "\\"
            PathOnlyNoVolume = PathOnlyNoVolume.Replace("\\\\", "\\")
            PathOnlyNoVolume = PathOnlyNoVolume.Replace("\\\\", "\\")

            If Path.StartsWith("\\") Then


                Dim RemotePath As New DirectoryInfo(PathOnly)


                If Directory.Exists(Path) = False Then Throw New Exception("Folder not found")
                ScanUNCFolders(PathOnly)
                dt.Rows.Add(Math.Round(Counter / 1024 / 1024, 2))


            Else

                If FileNameExtension = "" Then ' recognize last work as a name of folder and not a file
                    FileName = ""
                    If Right(Path, 1) <> "\" Then Path = Path & "\"
                End If

                Dim oQuery As ObjectQuery = New System.Management.ObjectQuery("SELECT Name  FROM Win32_Directory WHERE Name = '" & PathOnly.Replace("\", "\\") & "'")
                Dim oSearcher As ManagementObjectSearcher = New ManagementObjectSearcher(oms, oQuery)
                Dim Fnd As Boolean = False
                For Each oReturn As ManagementObject In oSearcher.Get()
                    Fnd = True
                Next

                If Fnd = False Then Throw New Exception("Folder not found")
                ScanSubFolders(oms, oQuery, oSearcher, PathOnlyNoVolume, Volume)
                If Fnd = True Then dt.Rows.Add(Math.Round(Counter / 1024 / 1024, 2))

            End If

            Return Me.GenerateActivityResult(dt)
        End Function


        Sub ScanSubFolders(ByVal oms As Management.ManagementScope, ByVal oQuery As ObjectQuery, ByVal oSearcher As ManagementObjectSearcher, ByVal PathOnlyNoVolume As String, ByVal Volume As String)

            oQuery = New System.Management.ObjectQuery("SELECT Name FROM Win32_Directory WHERE Drive = '" & Volume.Replace("\", "") & "' and path = '" & PathOnlyNoVolume & "'")
            oSearcher = New ManagementObjectSearcher(oms, oQuery)
            Dim FolderName As String = ""
            For Each oReturn As ManagementObject In oSearcher.Get()
                FolderName = oReturn("Name")

                Dim shortFolderName As String
                If (FolderName.Length > 200) Then
                    shortFolderName = FolderName.Substring(0, 200)
                Else
                    shortFolderName = FolderName
                End If
                FolderName = "\\" & FolderName.Replace(System.IO.Path.GetPathRoot(shortFolderName), "").Replace("\", "\\") & "\\"
                ScanSubFolders(oms, oQuery, oSearcher, FolderName, Volume)
            Next

            oQuery = New System.Management.ObjectQuery("SELECT  Name , FileSize FROM CIM_DataFile WHERE Drive = '" & Volume.Replace("\", "") & "' and path = '" & PathOnlyNoVolume & "'")
            oSearcher = New ManagementObjectSearcher(oms, oQuery)

            For Each oReturn As ManagementObject In oSearcher.Get()
                Counter = Counter + (oReturn("FileSize"))
            Next

        End Sub

        Sub ScanUNCFolders(ByVal Path As String)

            Dim RemotePath As New DirectoryInfo(Path)
            Dim Directories() As DirectoryInfo = RemotePath.GetDirectories
            For Each DirTemp As DirectoryInfo In Directories
                ScanUNCFolders(DirTemp.FullName)
            Next

            Dim Files() As FileInfo = RemotePath.GetFiles
            For Each FileName As FileInfo In Files
                Counter = Counter + FileName.Length
            Next

        End Sub


    End Class
End Namespace

