Imports Ayehu.Sdk.ActivityCreation.Interfaces
Imports Ayehu.Sdk.ActivityCreation.Extension
Imports System.Text
Imports System
Imports System.Xml
Imports System.Data
Imports System.IO
Imports System.Collections.Generic
Imports Microsoft.VisualBasic
Imports MySql.Data.MySqlClient

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
            Dim rowsAffected As Integer = 0
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


            Dim SELECT_STRING As String = Query

            Dim csb As New MySqlConnectionStringBuilder(ConnectionStringTextBox)
            If Not String.IsNullOrEmpty(UserName) Then
                csb.UserID = UserName
                csb.Password = Password
            End If

            Dim data_adapter As MySqlDataAdapter
            Using conn As New MySqlConnection(csb.ConnectionString)
                conn.Open()
                Using cmd As New MySqlCommand(Query, conn)
                    cmd.CommandTimeout = TimeInSeconds
                    rowsAffected = cmd.ExecuteNonQuery()
                End Using
                conn.Close()

            End Using



            dt.Rows.Add(rowsAffected)
            logMsg = dt.Rows(0)("rowsAffected").ToString

            Return Me.GenerateActivityResult(dt)

        End Function


    End Class
End Namespace

