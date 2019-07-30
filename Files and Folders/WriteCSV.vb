Imports Ayehu.Sdk.ActivityCreation.Interfaces
Imports Ayehu.Sdk.ActivityCreation.Extension
Imports System.Text
Imports System
Imports System.Xml
Imports System.Data
Imports System.IO
Imports System.Collections.Generic
Imports Microsoft.VisualBasic
Imports System.Net
Imports System.Text.RegularExpressions
Imports System.Runtime.InteropServices
Imports Syncfusion.XlsIO

Namespace Ayehu.Sdk.ActivityCreation
    Public Class ActivityClass
        Implements IActivity


        Public HostName As String
        Public UserName As String
        Public Password As String
        Public Path As String
        Public NewValue As String
        Public ExcelEngine As Object
        Public Delimiter As String
        Public IgnoreHeaders As Integer

        Public Function Execute() As ICustomActivityResult Implements IActivity.Execute


            Dim dt As DataTable = New DataTable("resultSet")



            dt = ReadXLSRemote(HostName, UserName, Password, Path, NewValue, ExcelEngine, Delimiter, IgnoreHeaders)



            Return Me.GenerateActivityResult(dt)

        End Function


        Function ReadXLSRemote(ByVal HostName As String, ByVal UserName As String, ByVal Password As String, ByVal Path As String, ByVal Value As String, ByVal ExcelEngine As Object, ByVal Delimiter As String, ByVal IgnoreHeaders As Integer) As DataTable

            Dim dtResult As New DataTable("resultSet")
            dtResult.Columns.Add("Result", GetType(String))
            If String.IsNullOrEmpty(Value) Then Throw New Exception("Variable is not a result set")

            Dim Table As DataTable
            Dim sr As StringReader = New StringReader(Value)
            Table = New DataTable()
            Try
                Table.ReadXml(sr)
            Catch
                Throw New Exception("Variable is not a result set")
            End Try
            If System.IO.File.Exists(Path) Then System.IO.File.Delete(Path)

            Try
                Dim application As IApplication = ExcelEngine.Excel
                application.UseNativeStorage = False
                Dim workbook As IWorkbook = application.Workbooks.Create(1)
                Dim sheet As IWorksheet = workbook.Worksheets(0)

                Dim showHeaders As Boolean = True
                If IgnoreHeaders = 1 Then
                    showHeaders = False
                End If

                sheet.ImportDataTable(Table, showHeaders, 2, 1, -1, -1, True)
                sheet.UsedRange.AutofitRows()
                sheet.UsedRange.AutofitColumns()
                sheet.SaveAs(Path, Delimiter, Encoding.UTF8)
                workbook.Close()
                ExcelEngine.ThrowNotSavedOnDestroy = False
                ExcelEngine.Dispose()
                dtResult.Rows.Add("Success")
            Catch
                Throw New Exception("Unable to save file")
            End Try

            Return dtResult

        End Function


    End Class
End Namespace

