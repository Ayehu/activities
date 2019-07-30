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
        Public IP As String
        Public Subnet As String
        Public GWAddr As String
        Public DNS1 As String
        Public DNS2 As String
        Public SCVMM As String

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

            powershell.AddScript(GetScript(VMName, IP, Subnet, GWAddr, DNS1, DNS2))
            Dim results As Object = powershell.Invoke()

            If powershell.Streams.Error.Count > 0 Then
                runspace.close()
                powershell.Dispose()
                Throw New Exception("Unable to set IP Address")
                'For Each errorRecord As Object In powershell.Streams.Error
                '    MsgBox(errorRecord.ToString())
                'Next
            End If

            Dim fnd As Boolean = False
            For Each psObject As PSObject In results

                For Each propInfo As PSPropertyInfo In psObject.Properties
                    If propInfo.Name = "IPAddresses" AndAlso propInfo.Value.ToString.StartsWith(IP) Then
                        dt.Rows.Add("Success")
                        fnd = True
                        Exit For
                    End If
                Next
            Next

            runspace.close()
            powershell.Dispose()

            If fnd = False Then Throw New Exception("Unable to set IP Address")
            Return Me.GenerateActivityResult(dt)

        End Function

        Function GetScript(VMName As String, IP As String, Subnet As String, MyGateWay As String, DNS1 As String, DNS2 As String) As String

            Dim sw As New StringWriter
            sw.WriteLine("$vmName = " + Chr(34) + VMName + Chr(34))
            sw.WriteLine("$Msvm_VirtualSystemManagementService = Get-WmiObject -Namespace root\virtualization\v2 -Class Msvm_VirtualSystemManagementService")
            sw.WriteLine("$Msvm_ComputerSystem = Get-WmiObject -Namespace root\virtualization\v2 -Class Msvm_ComputerSystem -Filter ""ElementName='$vmName'""")
            sw.WriteLine("$Msvm_VirtualSystemSettingData = ($Msvm_ComputerSystem.GetRelated(""Msvm_VirtualSystemSettingData"", ""Msvm_SettingsDefineState"", $null, $null, ""SettingData"", ""ManagedElement"", $false, $null) | % {$_})")
            sw.WriteLine("$Msvm_SyntheticEthernetPortSettingData = $Msvm_VirtualSystemSettingData.GetRelated(""Msvm_SyntheticEthernetPortSettingData"")")
            sw.WriteLine("$Msvm_GuestNetworkAdapterConfiguration = ($Msvm_SyntheticEthernetPortSettingData.GetRelated(""Msvm_GuestNetworkAdapterConfiguration"", ""Msvm_SettingDataComponent"", $null, $null, ""PartComponent"", ""GroupComponent"", $false, $null) | % {$_})")

            sw.WriteLine("$Msvm_GuestNetworkAdapterConfiguration.DHCPEnabled = $false")
            sw.WriteLine("$Msvm_GuestNetworkAdapterConfiguration.IPAddresses = @(""eyeShareIP"")")
            sw.WriteLine("$Msvm_GuestNetworkAdapterConfiguration.Subnets = @(""eyeShareSN"")")
            sw.WriteLine("$Msvm_GuestNetworkAdapterConfiguration.DefaultGateways = @(""eyeShareGW"")")
            sw.WriteLine("$Msvm_GuestNetworkAdapterConfiguration.DNSServers = @(""eyeShareDNS1"", ""eyeShareDNS2"")")

            sw.WriteLine("$Msvm_VirtualSystemManagementService.SetGuestNetworkAdapterConfiguration($Msvm_ComputerSystem.Path, $Msvm_GuestNetworkAdapterConfiguration.GetText(1))")

            sw.WriteLine("Get-VMNetworkAdapter -VMName " + Chr(34) + VMName + Chr(34))

            Dim result As String = sw.ToString
            result = result.Replace("eyeShareIP", IP).Replace("eyeShareSN", Subnet).Replace("eyeShareGW", MyGateWay).Replace("eyeShareDNS1", DNS1).Replace("eyeShareDNS2", DNS2)

            Return result
        End Function

    End Class
End Namespace

