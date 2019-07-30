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
Imports Microsoft.Win32

Namespace Ayehu.Sdk.ActivityCreation
    Public Class ActivityClass
        Implements IActivity


        Public HostName As String
        Public UserName As String
        Public Password As String
        Public RegistryKey As String
        Public Path As String

        Public Function Execute() As ICustomActivityResult Implements IActivity.Execute


            Dim dt As DataTable = New DataTable("resultSet")
            dt.Columns.Add("Result", GetType(String))
            Dim Result As String = ""
            Dim SearchKey As String = ""
            Dim Myarray As Array
            Dim Resultkey As Boolean
            Path = Trim(Path)
            If Path.StartsWith("\") Then Path = Path.Substring(1, Len(Path) - 1)
            Myarray = Split(Path, "\")
            If Myarray.Length > 0 Then
                If Path.EndsWith("\") = False Then If Path.Contains("\") Then Path = Path.Remove(Path.LastIndexOf("\"))

                SearchKey = Myarray(Myarray.Length - 1)

                Select Case RegistryKey
                    Case "HKEY_CLASSES_ROOT"
                        Resultkey = RegistryDeleteFunction(HostName, RegistryHive.ClassesRoot, Path, SearchKey, UserName, Password)
                    Case "HKEY_LOCAL_MACHINE"
                        Resultkey = RegistryDeleteFunction(HostName, RegistryHive.LocalMachine, Path, SearchKey, UserName, Password)
                    Case "HKEY_CURRENT_USER"
                        Resultkey = RegistryDeleteFunction(HostName, RegistryHive.CurrentUser, Path, SearchKey, UserName, Password)
                    Case "HKEY_USERS"
                        Resultkey = RegistryDeleteFunction(HostName, RegistryHive.Users, Path, SearchKey, UserName, Password)
                    Case "HKEY_CURRENT_CONFIG"
                        Resultkey = RegistryDeleteFunction(HostName, RegistryHive.CurrentConfig, Path, SearchKey, UserName, Password)
                End Select
            End If
            If Resultkey = True Then dt.Rows.Add("Success")

            Return Me.GenerateActivityResult(dt)

        End Function

        Public Function RegistryDeleteFunction(ByVal strComputerName As String, ByVal intRegistryHive As RegistryHive, ByVal strSubKeyName As String, ByVal strValueName As String, ByVal UserName As String, ByVal Password As String) As Boolean



            Dim WMIFullNameSpace As String = "\\" + strComputerName + "\root\cimv2"
            Dim strStringValue As String = ""
            Dim objManagementClass As ManagementClass
            Dim objManagementBaseObject As ManagementBaseObject
            Dim objManagementScope As ManagementScope
            strSubKeyName = strSubKeyName.Trim
            strValueName = strValueName.Trim
            If (strSubKeyName.Length > 0) Then
                objManagementScope = New ManagementScope
                With objManagementScope
                    With .Path
                        .Server = strComputerName
                        .NamespacePath = "root\default"
                    End With

                    With .Options
                        .EnablePrivileges = True
                        .Impersonation = ImpersonationLevel.Impersonate
                        If UserName <> "" Then
                            .Username = UserName
                            .Password = Password
                            .Authentication = AuthenticationLevel.PacketPrivacy
                        End If
                    End With

                    .Connect()
                End With

                If objManagementScope.IsConnected Then
                    objManagementClass = New ManagementClass("StdRegProv")

                    With objManagementClass
                        .Scope = objManagementScope
                        objManagementBaseObject = .GetMethodParameters("GetStringValue")

                        With objManagementBaseObject
                            .SetPropertyValue("hDefKey", CType("&H" & Hex(intRegistryHive), Long))
                            .SetPropertyValue("sSubKeyName", strSubKeyName)
                            .SetPropertyValue("sValueName", strValueName)
                        End With
                        Dim outParams As Management.ManagementBaseObject

                        Try
                            If strSubKeyName.EndsWith("\") Then
                                outParams = objManagementClass.InvokeMethod("DeleteKey", objManagementBaseObject, Nothing)
                            Else
                                outParams = objManagementClass.InvokeMethod("DeleteValue", objManagementBaseObject, Nothing)
                            End If
                        Catch
                        End Try

                        If CInt(outParams.Properties("ReturnValue").Value) = 0 Then
                            Return True
                        Else
                            Throw New Exception("Registry key not found")
                        End If

                    End With
                End If
            End If

        End Function


    End Class
End Namespace

