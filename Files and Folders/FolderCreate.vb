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

        Public Function Execute() As ICustomActivityResult Implements IActivity.Execute


            Dim dt As DataTable = New DataTable("resultSet")
            dt.Columns.Add("Result", GetType(String))

            Dim Volume As String = System.IO.Path.GetPathRoot(Path)
            'Dim FileName As String = Path.GetFileName()
            'Dim FileNameExtension As String = Path.GetExtension()
            If Right(Path, 1) <> "\" Then Path = Path & "\"

            Dim PathOnly As String = System.IO.Path.GetDirectoryName(Path)
            If PathOnly = Nothing Then PathOnly = Path
            Dim PathOnlyNoVolume As String = PathOnly
            PathOnlyNoVolume = PathOnlyNoVolume.Replace(Volume, "")
            PathOnlyNoVolume = "\\" & PathOnlyNoVolume.Replace("\", "\\") & "\\"
            PathOnlyNoVolume = PathOnlyNoVolume.Replace("\\\\", "\\")
            PathOnlyNoVolume = PathOnlyNoVolume.Replace("\\\\", "\\")


            If Path.StartsWith("\\") Then


                Dim RemotePath As New DirectoryInfo(PathOnly)
                If Directory.Exists(Path) Then
                    dt.Rows.Add("Failure: Folder " & Path & " already exist")
                Else
                    Try
                        Directory.CreateDirectory(Path)
                        dt.Rows.Add("Success")
                    Catch
                        dt.Rows.Add("Failure " + Err.Description)
                    End Try
                End If
            Else
                Dim connectionOptions As ConnectionOptions = New ConnectionOptions
                connectionOptions.Username = UserName
                connectionOptions.Password = Password
                connectionOptions.Authentication = Management.AuthenticationLevel.PacketPrivacy
                connectionOptions.Impersonation = ImpersonationLevel.Impersonate
                connectionOptions.EnablePrivileges = True

                Dim oms As Management.ManagementScope

                If ActivitiesUtils.IsLocalhost(HostName) Then
                    oms = New ManagementScope("\\.\root\cimv2")
                Else
                    oms = New ManagementScope("\\" & HostName & "\root\cimv2", connectionOptions)
                End If
                PathOnly = PathOnly.GetQueryPath()

                Dim oQuery As ObjectQuery = New System.Management.ObjectQuery("SELECT Name FROM Win32_Directory WHERE  Name = '" + PathOnly + "'")
                Dim oSearcher As ManagementObjectSearcher = New ManagementObjectSearcher(oms, oQuery)
                Dim Fnd As Boolean = False
                For Each oReturn As ManagementObject In oSearcher.Get()
                    Fnd = True
                Next
                If Fnd = True Then
                    dt.Rows.Add("Failure: Folder " & Path & " already exist")
                Else
                    Dim Locator = CreateObject("WbemScripting.SWbemLocator")
                    Dim objService = Locator.ConnectServer(HostName, "root\cimv2", UserName, Password)
                    objService.Security_.AuthenticationLevel = 6
                    Dim objStartup = objService.Get("Win32_ProcessStartup")
                    Dim objConfig = objStartup.SpawnInstance_
                    objConfig.ShowWindow = 1
                    Dim strCmd As String = "cmd.exe /c md " & Chr(34) & Path & Chr(34)
                    Dim objProcess = objService.Get("Win32_Process")
                    Dim intProcessID As Integer = 0
                    Dim intReturn = objProcess.Create(strCmd, "c:\", objConfig, intProcessID)


                    System.Threading.Thread.Sleep(2000)
                    Dim oQuery2 As ObjectQuery = New System.Management.ObjectQuery("SELECT Name FROM Win32_Directory WHERE  Name = '" + PathOnly + "'")
                    Dim oSearcher2 As ManagementObjectSearcher = New ManagementObjectSearcher(oms, oQuery2)
                    Dim Fnd2 As Boolean = False
                    For Each oReturn As ManagementObject In oSearcher2.Get()
                        Fnd2 = True
                    Next
                    If Fnd2 = True Then
                        dt.Rows.Add("Success")
                    Else
                        Throw New Exception("Failure")
                    End If
                End If
            End If

            Return Me.GenerateActivityResult(dt)
        End Function



    End Class
End Namespace

