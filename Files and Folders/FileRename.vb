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
        Public FileName As String
        Public NewFileName As String

        Public Function Execute() As ICustomActivityResult Implements IActivity.Execute


            Dim dt As DataTable = New DataTable("resultSet")
            dt.Columns.Add("Name", GetType(String))
            dt.Columns.Add("New Name", GetType(String))
            dt.Columns.Add("Result", GetType(String))


            Dim regex As Text.RegularExpressions.Regex = New Text.RegularExpressions.Regex("^(?!^(PRN|AUX|CLOCK\$|NUL|CON|COM\d|LPT\d|\..*)(\..+)?$)[^\x00-\x1f\\?*:\" & Chr(34) & ";|/]+$")
            Dim match As Text.RegularExpressions.Match = regex.Match(FileName)
            If match.Success = False Then Throw New Exception("File name is invalid")
            match = regex.Match(NewFileName)
            If match.Success = False Then Throw New Exception("New file name is invalid")
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

            Dim OrignalFileNameWithoutExtension As String = System.IO.Path.GetFileNameWithoutExtension(FileName)
            Dim OrignalFileNameExtension As String = System.IO.Path.GetExtension(FileName)

            Dim TagetFileNameWithoutExtension As String = System.IO.Path.GetFileNameWithoutExtension(NewFileName)
            Dim TagetFileNameExtension As String = System.IO.Path.GetExtension(NewFileName)

            If Path.EndsWith("\") = False Then Path = Path & "\"

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
                        If ValidateFile(LCase(OrignalFileNameWithoutExtension), LCase(OrignalFileNameExtension), LCase(oReturn.FullName)) = True Then
                            Fnd = True
                            Dim NewName As String = GetNewName(TagetFileNameWithoutExtension, TagetFileNameExtension, System.IO.Path.GetFileNameWithoutExtension(oReturn.Name), System.IO.Path.GetExtension(oReturn.Name))
                            Try
                                Dim CurerntName As String = oReturn.Name
                                oReturn.MoveTo(Path & NewName)
                                dt.Rows.Add(CurerntName, NewName, "Success")
                            Catch
                                dt.Rows.Add(oReturn.Name, NewName, Err.Description)
                            End Try
                        End If
                    Catch
                    End Try
                Next


                If Fnd = False Then

                    Throw New Exception("File not found")
                Else
                    If dt.Rows.Count = 1 Then
                        Dim f As String = dt.Rows(0)("Result")
                        dt = New DataTable("resultSet")
                        dt.Columns.Add("Result", GetType(String))
                        dt.Rows.Add(f.ToString)
                    End If
                End If

            Else
                Dim oQuery As ObjectQuery = New System.Management.ObjectQuery("SELECT Name , FileName , Extension FROM CIM_DataFile WHERE Drive = '" & Volume.Replace("\", "") & "' and path = '" & PathOnlyNoVolume & "'")
                Dim oSearcher As ManagementObjectSearcher = New ManagementObjectSearcher(oms, oQuery)

                Dim errResult As Integer = 0
                For Each oReturn As ManagementObject In oSearcher.Get()
                    If ValidateFile(LCase(OrignalFileNameWithoutExtension), LCase(OrignalFileNameExtension), LCase(oReturn("Name"))) = True Then
                        Fnd = True
                        Dim NewName As String = GetNewName(TagetFileNameWithoutExtension, TagetFileNameExtension, System.IO.Path.GetFileNameWithoutExtension(oReturn("Name")), System.IO.Path.GetExtension(oReturn("Name")))
                        Dim RenameTheFile() As Object = {Path & NewName}
                        errResult = oReturn.InvokeMethod("Rename", RenameTheFile)
                        Select Case errResult
                            Case 0
                                dt.Rows.Add(oReturn("FileName") & "." & oReturn("Extension"), NewName, "Success")
                            Case 2
                                Throw New Exception("Access denied")
                            Case 8
                                Throw New Exception("Unspecified failure")
                            Case 9
                                Throw New Exception("Invalid object")
                            Case 10
                                Throw New Exception("Object already exists")
                            Case 11
                                Throw New Exception("File system not NTFS")
                            Case 12
                                Throw New Exception("Platform not Windows NT or Windows 2000")
                            Case 13
                                Throw New Exception("Drive not the same")
                            Case 14
                                Throw New Exception("Directory not empty")
                            Case 15
                                Throw New Exception("Sharing violation")
                            Case 16
                                Throw New Exception("Invalid start file")
                            Case 17
                                Throw New Exception("Privilege not held")
                            Case 21
                                Throw New Exception("Invalid parameter")
                            Case Else
                                Throw New Exception("Failure")
                        End Select
                    End If
                Next
                If Fnd = False Then

                    Throw New Exception("File not found")
                Else
                    If dt.Rows.Count = 1 Then
                        Dim f As String = dt.Rows(0)("Result")
                        dt = New DataTable("resultSet")
                        dt.Columns.Add("Result", GetType(String))
                        dt.Rows.Add(f.ToString)
                    End If
                End If
            End If

            Return Me.GenerateActivityResult(dt)

        End Function

        Function GetNewName(ByVal TagetFileNameWithoutExtension As String, ByVal TagetFileNameExtension As String, ByVal FoundFileWithoutExtension As String, ByVal FoundFileExtention As String) As String

            If TagetFileNameExtension.StartsWith(".") Then TagetFileNameExtension = TagetFileNameExtension.Substring(1)
            If FoundFileExtention.StartsWith(".") Then FoundFileExtention = FoundFileExtention.Substring(1)

            Dim NewFileName As String = ""
            If TagetFileNameWithoutExtension = "*" Then
                NewFileName = FoundFileWithoutExtension
            ElseIf TagetFileNameWithoutExtension.StartsWith("*") Then
                If Len(TagetFileNameWithoutExtension.Substring(TagetFileNameWithoutExtension.IndexOf("*") + 1)) < Len(FoundFileWithoutExtension) Then
                    NewFileName = FoundFileWithoutExtension.Substring(0, Len(FoundFileWithoutExtension) - Len(TagetFileNameWithoutExtension.Substring(TagetFileNameWithoutExtension.IndexOf("*") + 1))) & TagetFileNameWithoutExtension.Substring(TagetFileNameWithoutExtension.IndexOf("*") + 1)
                Else
                    NewFileName = TagetFileNameWithoutExtension.Substring(TagetFileNameWithoutExtension.IndexOf("*"))
                End If

            ElseIf TagetFileNameWithoutExtension.EndsWith("*") Then
                NewFileName = TagetFileNameWithoutExtension.Substring(0, TagetFileNameWithoutExtension.IndexOf("*"))
                If Len(FoundFileWithoutExtension) > Len(NewFileName) Then
                    NewFileName = NewFileName & Mid(FoundFileWithoutExtension, Len(NewFileName) + 1, Len(FoundFileWithoutExtension))
                End If
            Else
                NewFileName = TagetFileNameWithoutExtension
            End If

            Dim NewFileExtension As String = ""

            If TagetFileNameExtension = "*" Then
                NewFileExtension = FoundFileExtention
            ElseIf TagetFileNameExtension.StartsWith("*") Then

                If Len(TagetFileNameExtension.Substring(TagetFileNameExtension.IndexOf("*") + 1)) < Len(FoundFileExtention) Then
                    NewFileExtension = FoundFileExtention.Substring(0, Len(FoundFileExtention) - Len(TagetFileNameExtension.Substring(TagetFileNameExtension.IndexOf("*") + 1))) & TagetFileNameExtension.Substring(TagetFileNameExtension.IndexOf("*") + 1)
                Else
                    NewFileExtension = TagetFileNameExtension.Substring(TagetFileNameExtension.IndexOf("*"))
                End If

            ElseIf TagetFileNameExtension.EndsWith("*") Then
                NewFileExtension = TagetFileNameExtension.Substring(0, TagetFileNameExtension.IndexOf("*"))
                If Len(FoundFileExtention) > Len(NewFileExtension) Then
                    NewFileExtension = NewFileExtension & Mid(FoundFileExtention, Len(NewFileExtension), Len(FoundFileExtention))
                End If
            Else
                NewFileExtension = TagetFileNameExtension
            End If

            Return NewFileName & "." & NewFileExtension


        End Function


        Function ValidateFile(ByVal FileName As String, ByVal FileNameExtension As String, ByVal TargetFile As String) As Boolean

            If System.IO.Path.GetFileNameWithoutExtension(TargetFile) Like FileName AndAlso System.IO.Path.GetExtension(TargetFile) Like FileNameExtension Then Return True

        End Function



    End Class
End Namespace

