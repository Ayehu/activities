Imports Ayehu.Sdk.ActivityCreation.Interfaces
Imports Ayehu.Sdk.ActivityCreation.Extension
Imports System.Text
Imports System
Imports System.Xml
Imports System.Data
Imports System.IO
Imports System.Collections.Generic
Imports Microsoft.VisualBasic
Imports System.Data.SqlClient
Imports System.Data.Odbc

Namespace Ayehu.Sdk.ActivityCreation
    Public Class ActivityClass
        Implements IActivity


        Public Query As String
        Public ConnectionString As String
        Public UserName As String
        Public Password As String
        Public TimeInSeconds As String

        Public Function Execute() As ICustomActivityResult Implements IActivity.Execute


            Dim logMsg As String
            Dim dt As DataTable = New DataTable("resultSet")
            Dim SELECT_STRING As String = Query
            Dim CONNECT_STRING As String = ConnectionString

            If Query <> "" Then
                Query = Query.Replace("{SQ}", "'")
                Query = Query.Replace("{CM}", ",")
                Query = Query.Replace("{DQ}", Chr(34))
                Query = Query.Replace("{PP}", "|")
                Query = Query.Replace("'", " ' ")
                Query = Query.Replace(" ' ", "'")
            End If

            If ConnectionString.IndexOf("DSN=") >= 0 Or ConnectionString.IndexOf("Driver=") >= 0 Then
                ConnectionString = ConnectionString.Replace("User Id", "UID")
                ConnectionString = ConnectionString.Replace("Password", "PWD")
                Dim ocsb As New OdbcConnectionStringBuilder(ConnectionString)
                If Not String.IsNullOrEmpty(UserName) Then
                    ocsb("Uid") = UserName
                    ocsb("Pwd") = Password
                End If


                Using conn As New Odbc.OdbcConnection(ocsb.ConnectionString)
                    conn.Open()
                    Dim data_adapter As OdbcDataAdapter = New OdbcDataAdapter(SELECT_STRING, conn)
                    data_adapter.SelectCommand.CommandTimeout = TimeInSeconds
                    data_adapter.TableMappings.Add("Table", "Table1")
                    data_adapter.Fill(dt)
                    conn.Close()
                End Using
            Else
                Dim csb As New SqlConnectionStringBuilder(ConnectionString)
                If Not String.IsNullOrEmpty(UserName) Then
                    csb.UserID = UserName
                    csb.Password = Password
                End If
                Using conn As New SqlClient.SqlConnection(csb.ConnectionString)
                    conn.Open()
                    Dim data_adapter = New SqlDataAdapter(SELECT_STRING, conn)
                    data_adapter.SelectCommand.CommandTimeout = TimeInSeconds
                    data_adapter.TableMappings.Add("Table", "Table1")
                    data_adapter.Fill(dt)
                    conn.Close()
                End Using
            End If




            Return Me.GenerateActivityResult(dt)

        End Function


    End Class
End Namespace

