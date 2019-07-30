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

            Dim sw2 As New StringWriter
            If SCVMM = "1" Then
                sw2.WriteLine("Stop-SCVirtualMachine -VM " + chr(34) + VMName + chr(34) + "–Force")
            Else
                sw2.WriteLine("Stop-VM -VMName " + chr(34) + VMName + chr(34) + "–TurnOff")
            End If


            powershell.AddScript(sw2.ToString)


            Dim results As Object = powershell.Invoke()

            If powershell.Streams.Error.Count > 0 Then
                Dim sw3 As New StringWriter
                For Each errorRecord As Object In powershell.Streams.Error
                    sw3.WriteLine(errorRecord.ToString())
                Next
                runspace.close()
                powershell.Dispose()
                Throw New Exception(sw3.ToString)
            End If


            dt.Rows.Add("Success")

            runspace.close()
            powershell.Dispose()


            Return Me.GenerateActivityResult(dt)


        End Function





    End Class
End Namespace

