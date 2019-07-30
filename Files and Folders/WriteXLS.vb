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
        Public PreserveTypes As Integer

        Public Function Execute() As ICustomActivityResult Implements IActivity.Execute


            Dim dt As DataTable = New DataTable("resultSet")
            dt = ReadXLSRemote(HostName, UserName, Password, Path, NewValue, ExcelEngine, PreserveTypes)
            Return Me.GenerateActivityResult(dt)
        End Function

        Function ReadXLSRemote(ByVal HostName As String, ByVal UserName As String, ByVal Password As String, ByVal Path As String, ByVal Value As String, ExcelEngine As Object, ByVal PreserveTypes As Integer) As DataTable

            Dim dtResult As New DataTable("resultSet")
            dtResult.Columns.Add("Result", GetType(String))
            Try
                Value = Trim(Value)
                If String.IsNullOrEmpty(Value) Then
                    Throw New Exception("Table field must contain a table type variable")
                End If
                Dim ExcelByte() As Byte = Convert.FromBase64String(Value)
                File.WriteAllBytes(Path, ExcelByte)
                dtResult.Rows.Add("Success")
            Catch
                Dim Table As DataTable
                Table = New DataTable()
                Try
                    Dim sr As StringReader = New StringReader(Value)
                    Table.ReadXml(sr)
                Catch ex As Exception
                    Throw New Exception("Table field must contain a table type variable")
                End Try
                Try
                    If System.IO.File.Exists(Path) Then System.IO.File.Delete(Path)

                    Dim application As IApplication = ExcelEngine.Excel
                    application.DefaultVersion = ExcelVersion.Excel2010
                    application.UseNativeStorage = False
                    Dim workbook As IWorkbook = application.Workbooks.Create(1)
                    Dim sheet As IWorksheet = workbook.Worksheets(0)

                    Dim prsrvTypes As Boolean = False
                    If PreserveTypes = 1 Then
                        prsrvTypes = True
                    End If

                    sheet.ImportDataTable(Table, True, 1, 1, -1, -1, prsrvTypes)
                    sheet.UsedRange.AutofitRows()
                    sheet.UsedRange.AutofitColumns()

                    workbook.Version = ExcelVersion.Excel2010
                    workbook.SaveAs(Path)
                    workbook.Close()
                    ExcelEngine.ThrowNotSavedOnDestroy = False
                    ExcelEngine.Dispose()
                    dtResult.Rows.Add("Success")
                Catch ex As Exception
                    Throw New Exception("Unable to write into file at provided path")
                End Try
            End Try

            Return dtResult

        End Function


    End Class
End Namespace

