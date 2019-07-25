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
        Public ConnectionString As String
        Public UserName As String
        Public Password As String
        Public TimeInSeconds As String

        Public Function Execute() As ICustomActivityResult Implements IActivity.Execute


            Dim rowsAffected As Integer = 0
            Dim logMsg As String
            Dim dt As DataTable = New DataTable("resultSet")
            dt.Columns.Add("rowsAffected", GetType(String))

            If Query <> "" Then
                Query = Query.Replace("{SQ}", "'")
                Query = Query.Replace("{CM}", ",")
                Query = Query.Replace("{DQ}", Chr(34))
                Query = Query.Replace("{PP}", "|")
                Query = Query.Replace("'", " ' ")
                Query = Query.Replace(" ' ", "'")

            End If

            If Not String.IsNullOrEmpty(UserName) Then
                Try
                    Dim ocsb As New OracleConnectionStringBuilder(ConnectionString)
                    ocsb.UserID = UserName
                    ocsb.Password = Password
                    ConnectionString = ocsb.ConnectionString
                Catch ex As Exception

                End Try
            End If

            If OracleConnection.IsAvailable = False OrElse OracleConnection.IsAvailable = True Then
                Using conn As New OracleConnection(ConnectionString)
                    conn.Open()
                    Using cmd As New OracleCommand(Query, conn)
                        cmd.CommandTimeout = TimeInSeconds
                        rowsAffected = cmd.ExecuteNonQuery()
                    End Using
                    conn.Close()
                End Using
                dt.Rows.Add(rowsAffected)
                logMsg = dt.Rows(0)("rowsAffected").ToString
            End If

            dt.Rows.Add(rowsAffected)
            logMsg = dt.Rows(0)("rowsAffected").ToString

            Return Me.GenerateActivityResult(dt)

        End Function


    End Class
End Namespace

