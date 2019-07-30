Imports Ayehu.Sdk.ActivityCreation.Interfaces
Imports Ayehu.Sdk.ActivityCreation.Extension
Imports System.Text
Imports System
Imports System.Xml
Imports System.Data
Imports System.IO
Imports System.Collections.Generic
Imports Microsoft.VisualBasic
Imports nsoftware.IPWorks

Namespace Ayehu.Sdk.ActivityCreation
    Public Class ActivityClass
        Implements IActivity


        Dim Tracert As New Traceroute
        Dim sw As New StringWriter
        Dim dt As DataTable = New DataTable("resultSet")
        Public Address As String
        Public MaxHops As Integer
        Public HopTimeout As Integer
        Public Timeout As Integer

        Public Function Execute() As ICustomActivityResult Implements IActivity.Execute


            If String.IsNullOrEmpty(Address) Then
                Throw New ApplicationException("Illegal or empty address")
            End If

            If MaxHops <= 0 Then
                Throw New ApplicationException("Illegal or empty Max Hops parameter. Minimum value is 1")
            End If

            If HopTimeout <= 0 Then
                Throw New ApplicationException("Illegal time-out value. Minimum is 1")
            End If

            dt.Columns.Add("Hop", GetType(String))
            dt.Columns.Add("Address", GetType(String))
            dt.Columns.Add("MS", GetType(String))
            Tracert.RuntimeLicense = ""

            AddHandler Tracert.OnHop, AddressOf Traceroute1_OnHop

            Try

                Tracert.Timeout = Timeout - 3
                Tracert.HopLimit = MaxHops
                Tracert.HopTimeout = HopTimeout
                Tracert.TraceTo(Address)

            Catch e As Exception
                If Err.Description.Contains("Timeout") Then
                    dt.Rows.Add(dt.Rows.Count + 1, "Request timed out", "*")
                Else
                    Throw
                End If
            End Try

            Return Me.GenerateActivityResult(dt)


        End Function

        Private Sub Traceroute1_OnHop(ByVal sender As System.Object, ByVal e As nsoftware.IPWorks.TracerouteHopEventArgs)
            dt.Rows.Add(e.HopNumber, e.HostAddress, e.Duration)
        End Sub

    End Class
End Namespace

