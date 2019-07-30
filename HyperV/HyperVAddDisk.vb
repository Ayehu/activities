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
        Public VMFileName As String
        Public VMSize As String
        Public SCVMM As String
        Public VMType As String
        Public VMBus As String
        Public VMLun As String

        Public Function Execute() As ICustomActivityResult Implements IActivity.Execute

            Dim sw As StringWriter = New StringWriter()
            Dim dt As DataTable = New DataTable("resultSet")
            dt.Columns.Add("Result", GetType(String))

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

            powershell.AddScript(GetScript(HostName, VMName, VMFileName, VMSize, VMType, VMBus, VMLun))
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

            runspace.close()
            powershell.Dispose()
            dt.Rows.Add("Success")

            Return Me.GenerateActivityResult(dt)


        End Function

        ''' <summary>
        ''' Gets the script.
        ''' </summary>
        ''' <param name="Hostname">The hostname.</param>
        ''' <param name="VMName">Name of the vm.</param>
        ''' <param name="VMFileName">Name of the vm file.</param>
        ''' <param name="VMSize">Size of the vm.</param>
        ''' <param name="VMType">Type of the vm.</param>
        ''' <param name="VMBus">The vm bus.</param>
        ''' <param name="VMLun">The vm lun.</param>
        ''' <returns></returns>
        ''' <example>
        '''     New-SCVirtualDiskDrive -VMMServer scvmm2016.rnddc.com -IDE -Bus 0 -LUN 1 -VirtualHardDiskSizeMB 7168 -Dynamic -Filename "Batman_disk_2" -VolumeType None
        ''' </example>
        Function GetScript(Hostname As String, VMName As String, VMFileName As String, VMSize As String, VMType As String, VMBus As String, VMLun As String) As String

            Dim sw As New StringWriter

            sw.WriteLine("$VM = Get-SCVirtualMachine -Name " + Chr(34) + VMName + Chr(34) + " -VMMServer " + Hostname)

            Dim fileName As String = System.IO.Path.GetFileName(VMFileName)

            sw.WriteLine("New-SCVirtualDiskDrive -VM $VM[0] -Dynamic -Filename " + Chr(34) + fileName + Chr(34) + " -" + VMType + " -VirtualHardDiskSizeMB " + VMSize + " -Bus " + VMBus + " -LUN " + VMLun + " -VMMServer " + Hostname)

            Dim result As String = sw.ToString
            Return result
        End Function

    End Class
End Namespace

