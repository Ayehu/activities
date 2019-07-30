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
      Public ApplicationPoolName As String

      Public Function Execute() As ICustomActivityResult Implements IActivity.Execute

          Dim sw As New StringWriter
          Dim dt As DataTable = New DataTable("resultSet")
          dt.Columns.Add("Result", GetType(String))
          If ApplicationPoolName = "" Then Throw New Exception("Application pool name is missing")
          
          Dim connectionOptions As ConnectionOptions = New ConnectionOptions
          connectionOptions.Username = UserName
          connectionOptions.Password = Password
          connectionOptions.Authentication = AuthenticationLevel.PacketPrivacy
          connectionOptions.Impersonation = ImpersonationLevel.Impersonate
          connectionOptions.EnablePrivileges = True
          
          Dim oms As Management.ManagementScope
          
          If LCase(HostName) = "localhost" OrElse HostName = "127.0.0.1" Then
          oms = New ManagementScope("\\.\root\microsoftiisv2")
          Else
          oms = New ManagementScope("\\" & HostName & "\root\microsoftiisv2", connectionOptions)
          End If
          
          Dim oQuery As ObjectQuery = New System.Management.ObjectQuery("Select * from IIsApplicationPoolsetting")
          Dim oSearcher As ManagementObjectSearcher = New ManagementObjectSearcher(oms, oQuery)
          Dim oReturnCollection As ManagementObjectCollection = oSearcher.Get()
          Dim Fnd As Boolean = False
          Dim FoundAnyApplicationPool As Boolean = False
          Dim Myarray As Array
          For Each oReturn As ManagementObject In oReturnCollection
          Fnd = False
          If Trim(LCase(oReturn("Name"))) = Trim(LCase(ApplicationPoolName)) Then
          Fnd = True
          ElseIf oReturn("Name").ToString.Contains("/") Then
          Myarray = Split(oReturn("Name").ToString, "/")
          If Trim(LCase(Myarray(Myarray.Length - 1))) = Trim(LCase(ApplicationPoolName)) Then
          Fnd = True
          End If
          End If
          If Fnd = True Then
          FoundAnyApplicationPool = True
          Select Case oReturn("AppPoolState")
          
          Case 1
          dt.Rows.Add("Starting")
          
          Case 2
          dt.Rows.Add("Running")
          
          Case 3
          dt.Rows.Add("Stopping")
          
          Case 4
          dt.Rows.Add("Stopped")
          
          Case Else
          dt.Rows.Add("Unknown")
          End Select
          
          End If
          Next
          
          If FoundAnyApplicationPool = False Then throw new exception("Unknown application pool")
          
          
      Return Me.GenerateActivityResult(dt)
          Return sw.ToString
          
          End Function
          

  End Class
End Namespace

