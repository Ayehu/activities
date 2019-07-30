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
      Public ApplicationPoolName as string

      Public Function Execute() As ICustomActivityResult Implements IActivity.Execute

          Dim sw As New StringWriter
          Dim dt As DataTable = New DataTable("resultSet")
          dt.Columns.Add("Result", GetType(String))
          If ApplicationPoolName= "" then  Throw New Exception ("Application pool name is missing")
          try
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
          Dim Fnd as boolean = false
          For Each oReturn As ManagementObject In oReturnCollection
          try
          If LCase(Mid(oReturn("Name"), Len(oReturn("Name")) - Len(ApplicationPoolName) + 1, Len(oReturn("Name")))) = LCase(ApplicationPoolName) Then
          fnd = true
          oReturn.InvokeMethod("recycle", Nothing)
          end if
          catch
          end try
          Next
          
          If Fnd = False Then
          throw new exception("Unknown application pool")
          else
          dt.Rows.Add("Success")
          end if
          Catch e As Exception
          
          
          
          if err.description.contains("Win32: The object identifier does not represent a valid object.") then
          throw new exception("Error recycling application pool")
          else
          throw
          end if
          
          End Try
      Return Me.GenerateActivityResult(dt)
          Return sw.ToString
          
          End Function
          

  End Class
End Namespace

