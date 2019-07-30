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
        Public Path As String
        Public UserName As String
        Public Password As String

        Public Function Execute() As ICustomActivityResult Implements IActivity.Execute


            Dim dt As DataTable = New DataTable("resultSet")
            dt.Columns.Add("Name", GetType(String))
            dt.Columns.Add("Size", GetType(String))

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
            Dim Fnd As Boolean = False
            If Path.StartsWith("\\") Then


                Dim RemotePath As New DirectoryInfo(PathOnly)


                Dim Files() As FileInfo = RemotePath.GetFiles
                For Each oReturn As FileInfo In Files
                    Try
                        If ValidateFile(LCase(FileName), LCase(FileNameExtension), LCase(oReturn.FullName)) = True Then
                            Fnd = True
                            dt.Rows.Add(oReturn.Name, Convert.ToDecimal(Math.Round(oReturn.Length / 1024 / 1024, 5)))
                        End If
                    Catch
                    End Try
                Next

                If Fnd = False Then

                    Throw New Exception("File not found")
                ElseIf dt.Rows.Count = 1 Then
                    Dim f As Double = dt.Rows(0)("Size")
                    dt = New DataTable("resultSet")
                    dt.Columns.Add("Result", GetType(String))
                    dt.Rows.Add(f.ToString)
                End If
            Else

                Dim oQuery As ObjectQuery = New System.Management.ObjectQuery("SELECT Name , FileName  , Extension, FileSize FROM CIM_DataFile WHERE Drive = '" & Volume.Replace("\", "") & "' and path = '" & PathOnlyNoVolume & "'")
                Dim oSearcher As ManagementObjectSearcher = New ManagementObjectSearcher(oms, oQuery)

                For Each oReturn As ManagementObject In oSearcher.Get()
                    If ValidateFile(lcase(FileName), lcase(FileNameExtension), lcase(oReturn("Name"))) = True Then
                        Fnd = True
                        dt.Rows.Add(oReturn("FileName") & "." & oReturn("Extension"), Convert.ToDecimal(Math.Round(oReturn("FileSize") / 1024 / 1024, 5)))
                    End If
                Next
                If Fnd = False Then

                    Throw New Exception("File not found")
                ElseIf dt.Rows.Count = 1 Then
                    Dim f As Double = dt.Rows(0)("Size")
                    dt = New DataTable("resultSet")
                    dt.Columns.Add("Result", GetType(String))
                    dt.Rows.Add(f.ToString)
                End If
            End If

            Return Me.GenerateActivityResult(dt)

        End Function


        Function ValidateFile(ByVal FileName As String, ByVal FileNameExtension As String, ByVal TargetFile As String) As Boolean

            If System.IO.Path.GetFileNameWithoutExtension(TargetFile) Like FileName AndAlso System.IO.Path.GetExtension(TargetFile) Like FileNameExtension Then Return True

        End Function




    End Class
End Namespace

