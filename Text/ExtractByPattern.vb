Imports Ayehu.Sdk.ActivityCreation.Interfaces
Imports Ayehu.Sdk.ActivityCreation.Extension
Imports System.Text
Imports System
Imports System.Xml
Imports System.Data
Imports System.IO
Imports System.Collections.Generic
Imports Microsoft.VisualBasic

Namespace Ayehu.Sdk.ActivityCreation
    Public Class ActivityClass
        Implements IActivity


        Public TheValue As String
        Public StartString As String
        Public Endstring As String
        Public coulmnNameStart As String
        Public ColumnNameEnds As String
        Public ColumnDataStart As String
        Public ColumnDataEnds As String
        Public CreateTable As Integer

        Public Function Execute() As ICustomActivityResult Implements IActivity.Execute


            Dim dt As DataTable = New DataTable("resultSet")



            Dim resultlist As New List(Of String)

            Dim ParrsedValue As String = TheValue
            ParrsedValue = ParrsedValue.Replace("&lt;", "<").Replace("&gt;", ">")


            ' Locate all strings that start with StartString and Ends with EndString

            If ParrsedValue.Contains(StartString) = False OrElse ParrsedValue.Contains(Endstring) = False Then Throw New Exception("no pattern match found")
            Try
                Dim nomatch As Boolean = False
                Dim ps As Integer = ParrsedValue.IndexOf(StartString)
                Do While nomatch = False
                    If ps >= Len(ParrsedValue) Then Exit Do
                    Dim y As Integer = ParrsedValue.IndexOf(Endstring, ps)
                    If y = -1 Then Exit Do
                    If y = ps AndAlso y + 1 < Len(ParrsedValue) Then y = ParrsedValue.IndexOf(Endstring, ps + 1)
                    If y = -1 Then Exit Do
                    resultlist.Add(ParrsedValue.Substring(ps, y - ps + 1))
                    ps = ParrsedValue.IndexOf(StartString, ps + 1)
                    If ps = -1 Then Exit Do
                Loop

            Catch
                Throw New Exception("Unable to extract pattern " + Err.Description)
            End Try

            If resultlist.Count = 0 Then Throw New Exception("Pattern string was not found")


            If CreateTable = 1 Then
                Dim Data As New Dictionary(Of String, String)
                Try
                    For Each item As String In resultlist
                        Dim nomatch As Boolean = False
                        Dim ps As Integer = item.IndexOf(coulmnNameStart)
                        Dim ColumnName As String = ""
                        Do While nomatch = False
                            If ps >= Len(item) Then Exit Do
                            Dim y As Integer = item.IndexOf(ColumnNameEnds, ps)
                            If y = -1 Then Exit Do
                            If y = ps AndAlso y + 1 < Len(item) Then y = item.IndexOf(ColumnNameEnds, ps + 1)
                            If y = -1 Then Exit Do
                            ColumnName = item.Substring(ps, y - ps + 1)
                            If ColumnName.StartsWith(coulmnNameStart) Then ColumnName = ColumnName.Substring(1)
                            If ColumnName.EndsWith(ColumnNameEnds) Then ColumnName = ColumnName.Substring(0, ColumnName.LastIndexOf(ColumnNameEnds))
                            ps = item.IndexOf(coulmnNameStart, ps + 1)
                            If ps = -1 Then Exit Do
                        Loop

                        Dim DataValue As String = ""
                        Dim psdata As Integer = item.IndexOf(ColumnDataStart)
                        Do While nomatch = False
                            If psdata >= Len(item) Then Exit Do
                            Dim y As Integer = item.IndexOf(ColumnDataEnds, psdata)
                            If y = -1 Then Exit Do
                            If y = psdata AndAlso y + 1 < Len(item) Then y = item.IndexOf(ColumnDataEnds, psdata + 1)
                            If y = -1 Then Exit Do
                            DataValue = item.Substring(psdata, y - psdata + 1)
                            If DataValue.StartsWith(ColumnDataStart) Then DataValue = DataValue.Substring(Len(ColumnDataStart))
                            If DataValue.EndsWith(ColumnDataEnds) Then DataValue = DataValue.Substring(0, DataValue.LastIndexOf(ColumnDataEnds))
                            If DataValue.EndsWith(ColumnDataEnds.Substring(0, 1)) Then DataValue = DataValue.Substring(0, DataValue.LastIndexOf(ColumnDataEnds.Substring(0, 1)))
                            psdata = item.IndexOf(ColumnDataStart, psdata + 1)
                            If psdata = -1 Then Exit Do
                        Loop
                        If Data.ContainsKey(ColumnName) Then
                            Dim ColumnExist As Boolean = True
                            Dim ColumnCounter As Integer = 1
                            Do While ColumnExist = True
                                If Data.ContainsKey(ColumnName & ColumnCounter.ToString) = False Then
                                    Data(ColumnName & ColumnCounter.ToString) = DataValue
                                    Exit Do
                                Else
                                    ColumnCounter = ColumnCounter + 1
                                End If
                            Loop
                        Else
                            Data(ColumnName) = DataValue
                        End If
                    Next

                    For Each item As KeyValuePair(Of String, String) In Data
                        dt.Columns.Add(item.Key)
                    Next
                    Dim NewRow As DataRow = dt.NewRow
                    Dim Counter As Integer = 0
                    For Each item As KeyValuePair(Of String, String) In Data
                        NewRow(Counter) = item.Value
                        Counter = Counter + 1
                    Next
                    dt.Rows.Add(NewRow)
                Catch
                    Throw New Exception("Unable to build table columns or data")
                End Try
            Else ' Just return the list of lines
                dt.Columns.Add("Result")
                For Each item As String In resultlist
                    dt.Rows.Add(item)
                Next

            End If

            Return Me.GenerateActivityResult(dt)
        End Function


    End Class
End Namespace

