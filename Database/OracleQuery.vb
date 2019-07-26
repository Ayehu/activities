Imports Ayehu.Sdk.ActivityCreation.Interfaces
Imports Ayehu.Sdk.ActivityCreation.Extension
Imports System.Text
Imports System
Imports System.Xml
Imports System.Data
Imports System.IO
Imports System.Collections.Generic
Imports Microsoft.VisualBasic
Imports Oracle.DataAccess.Client

Namespace Ayehu.Sdk.ActivityCreation
    Public Class ActivityClass
        Implements IActivity


        Public Query As String
        Public ConnectionStringTextBox As String
        Public UserName As String
        Public Password As String
        Public TimeInSeconds As String

        Public Function Execute() As ICustomActivityResult Implements IActivity.Execute


            Dim logMsg As String
            Dim dt As DataTable = New DataTable("resultSet")


            If Query <> "" Then
                Query = Query.Replace("{SQ}", "'")
                Query = Query.Replace("{CM}", ",")
                Query = Query.Replace("{DQ}", Chr(34))
                Query = Query.Replace("{PP}", "|")
                Query = Query.Replace("'", " ' ")
                Query = Query.Replace(" ' ", "'")
            End If

            Dim QueryTemp As String = LCase(Query)


            Dim SELECT_STRING As String = Query

            If Not String.IsNullOrEmpty(UserName) Then
                Try
                    Dim ocsb As New OracleConnectionStringBuilder(ConnectionStringTextBox)
                    ocsb.UserID = UserName
                    ocsb.Password = Password
                    ConnectionStringTextBox = ocsb.ConnectionString
                Catch ex As Exception

                End Try
            End If

            Dim data_adapter As OracleDataAdapter
            If OracleConnection.IsAvailable = False OrElse OracleConnection.IsAvailable = False Then
                Using conn As New OracleConnection(ConnectionStringTextBox)
                    conn.Open()
                    data_adapter = New OracleDataAdapter(SELECT_STRING, conn)
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

