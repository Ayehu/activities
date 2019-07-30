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


      Public HostName as string
      Public UserName as string
      Public Password as string

      Public Function Execute() As ICustomActivityResult Implements IActivity.Execute

          Dim sw As New StringWriter
          Dim dt As DataTable = New DataTable("resultSet")
          dt.Columns.Add("Result", GetType(String))
          
          
          Dim connectionOptions As ConnectionOptions = New ConnectionOptions
          connectionOptions.Username = Username
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
          
          Dim oQuery As ObjectQuery = New System.Management.ObjectQuery("Select * from IIsApplicationPool")
          Dim oSearcher As ManagementObjectSearcher = New ManagementObjectSearcher(oms, oQuery)
          Dim oReturnCollection As ManagementObjectCollection = oSearcher.Get()
          Dim Fnd As Boolean = False
          For Each oReturn As ManagementObject In oReturnCollection
          Fnd = True
          dt.Rows.Add(oReturn("Name"))
          Next
          
          If Fnd = False Then throw new exception("No applications pool")
          
          
      Return Me.GenerateActivityResult(dt)
          Return sw.ToString
          
          End Function
          

  End Class
End Namespace

