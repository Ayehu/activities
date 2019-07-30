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
Imports Syncfusion.XlsIO

Namespace Ayehu.Sdk.ActivityCreation
    Public Class ActivityClass
        Implements IActivity


        Public HostName As String
        Public UserName As String
        Public Password As String
        Public Path As String
        Public ExcelEngine As Object
        Public Delimiter As String
        Public RemoveHeaders As String

        Public Function Execute() As ICustomActivityResult Implements IActivity.Execute


            Dim dt As DataTable = New DataTable("resultSet")
            dt.Columns.Add("Result", GetType(String))
            If File.Exists(Path) = False Then Throw New Exception("File not found")

            dt = ReadFileRemote(HostName, UserName, Password, Path, ExcelEngine, Delimiter, RemoveHeaders)

            Return Me.GenerateActivityResult(dt)

        End Function




        Function ReadFileRemote(ByVal HostName As String, ByVal UserName As String, ByVal Password As String, ByVal Path As String, ByVal ExcelEngine As Object, Delimiter As String, RemoveHeaders As String) As DataTable

            Dim dtResult As New DataTable("resultSet")
            dtResult.Columns.Add("Result", GetType(String))





            If String.IsNullOrEmpty(Delimiter) Then Delimiter = ","


            Dim application As IApplication = ExcelEngine.Excel
            application.DefaultVersion = ExcelVersion.Excel2010
            application.UseNativeStorage = False
            Dim workbook As IWorkbook
            workbook = application.Workbooks.Open(Path, Delimiter)
            Dim sheet As IWorksheet = workbook.Worksheets(0)
            workbook.Worksheets(0).Name = "resultSet"

            If String.IsNullOrEmpty(RemoveHeaders) Then
                dtResult = sheet.ExportDataTable(sheet.UsedRange, ExcelExportDataTableOptions.ColumnNames)
            Else
                If Boolean.Parse(RemoveHeaders) Then
                    dtResult = sheet.ExportDataTable(sheet.UsedRange, ExcelExportDataTableOptions.None)
                Else
                    dtResult = sheet.ExportDataTable(sheet.UsedRange, ExcelExportDataTableOptions.ColumnNames)
                End If
            End If

            workbook.Close()
            ExcelEngine.ThrowNotSavedOnDestroy = False
            ExcelEngine.Dispose()


            For Each col As DataColumn In dtResult.Columns
                col.ColumnName = Trim(col.ColumnName)
            Next
            Return dtResult



        End Function




    End Class
End Namespace

