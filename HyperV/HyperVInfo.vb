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
Imports System.Management.Automation
Imports System.Management.Automation.Runspaces

Namespace Ayehu.Sdk.ActivityCreation
    Public Class ActivityClass
        Implements IActivity


        Public HostName As String
        Public UserName As String
        Public Password As String
        Public VMName As String
        Public SCVMM As String

        Public Function Execute() As ICustomActivityResult Implements IActivity.Execute


            Dim sw As New StringWriter
            Dim dt As DataTable = New DataTable("resultSet")
            dt.Columns.Add("Result", GetType(String))
            Dim fnd As Boolean = False



            Dim serverUri As System.Uri = New Uri("http://" + HostName + ":5985/wsman")
            Dim shellUri As String = "http://schemas.microsoft.com/powershell/Microsoft.PowerShell"

            Dim securePassword As System.Security.SecureString = New System.Security.SecureString()
            For Each c As Char In Password.ToCharArray()
                securePassword.AppendChar(c)
            Next

            Dim creds As PSCredential = New PSCredential(UserName, securePassword)

            Dim rc As RunspaceConfiguration = RunspaceConfiguration.Create()

            Dim wsManInfo As WSManConnectionInfo = New WSManConnectionInfo(serverUri, shellUri, creds)
            wsManInfo.SkipCNCheck = True
            wsManInfo.SkipCACheck = True

            Dim runspace As Object = RunspaceFactory.CreateRunspace(wsManInfo)
            runspace.Open()
            Dim powershell As PowerShell = powershell.Create()
            powershell.Runspace = runspace


            If SCVMM = "1" Then
                powershell.AddCommand("Get-SCVirtualMachine")
                powershell.Commands.AddParameter("-Name", VMName)
            Else
                powershell.AddCommand("Get-VM")
                powershell.Commands.AddParameter("VMName", VMName)
            End If

            Dim results As Object = powershell.Invoke()

            For Each psObject As PSObject In results
                fnd = True
                For Each propInfo As PSPropertyInfo In psObject.Properties
                    If dt.Columns.Contains(propInfo.Name) = False Then dt.Columns.Add(propInfo.Name)
                Next
                Dim newrow As DataRow = dt.NewRow
                For Each propInfo As PSPropertyInfo In psObject.Properties
                    newrow(propInfo.Name) = propInfo.Value
                Next
                dt.Rows.Add(newrow)
            Next
            runspace.close()
            powershell.Dispose()
            If fnd = False Then Throw New Exception("Unknown VM")


            Return Me.GenerateActivityResult(dt)


        End Function





    End Class
End Namespace

