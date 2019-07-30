Imports Ayehu.Sdk.ActivityCreation.Interfaces
Imports Ayehu.Sdk.ActivityCreation.Extension
Imports System.Text
Imports System
Imports System.Xml
Imports System.Data
Imports System.IO
Imports System.Collections
Imports System.Collections.Generic
Imports Microsoft.VisualBasic
Imports System.Management

Namespace Ayehu.Sdk.ActivityCreation
    Public Class ActivityClass
        Implements IActivity


        Public Query As String
        Public WmiNamespace As String
        Public HostName As String
        Public UserName As String
        Public Password As String
        Public TimeInSeconds As Integer

        Public Function Execute() As ICustomActivityResult Implements IActivity.Execute


            Dim dt As DataTable = New DataTable("resultSet")


            If String.IsNullOrEmpty(Trim(Query)) = True Then Throw New Exception("WMI Query error")
            If String.IsNullOrEmpty(Trim(WmiNamespace)) = True Then Throw New Exception("Unknown namespace")


            Dim wmiPath As String
            Dim queryParams As String
            Dim WMIQueryWithSlash As String = ""
            Dim WMIQueryWithoutSlash As String = ""

            Try
                Query = Query.Replace("''", "'")

                WmiNamespace = Trim(WmiNamespace)
                If Mid(WmiNamespace, 1, 1) <> "\" Then
                    WMIQueryWithoutSlash = WmiNamespace
                    WMIQueryWithSlash = "\" + WmiNamespace
                Else
                    WMIQueryWithSlash = WmiNamespace
                    WMIQueryWithoutSlash = Mid(WmiNamespace, 2, Len(WmiNamespace))
                End If

                wmiPath = String.Format("\\{0}{1}", HostName, WMIQueryWithSlash)
                Query = Query.Replace(Chr(10), " ")
                queryParams = LCase(Query)
                queryParams = queryParams.Remove(queryParams.IndexOf("from"))
                queryParams = queryParams.Substring(queryParams.IndexOf("select") + 6)
            Catch
                Throw New Exception("WMI Query error")
            End Try
            Dim items As New ArrayList()
            Dim Counter As Integer = 0

            Dim connectionOptions As ConnectionOptions = New ConnectionOptions
            connectionOptions.Username = UserName
            connectionOptions.Password = Password
            connectionOptions.Authentication = 6

            Dim WMIParamerers As New List(Of String)
            Dim WMIParamerersFields As New List(Of String)
            Query = Trim(Query)
            queryParams = queryParams.ToString.Replace(" ", "")
            WMIParamerers.AddRange(Trim(queryParams).Split(New String() {","}, StringSplitOptions.RemoveEmptyEntries))
            WMIParamerersFields.AddRange(Trim(queryParams).Split(New String() {","}, StringSplitOptions.RemoveEmptyEntries))

            Query = Query.Replace(Chr(10), " ")
            Dim myarray As Array
            Dim PropertyName As String = ""
            Dim Classname As String = ""
            myarray = Split(Query, " ")
            For Each txt As String In myarray
                txt = LCase(txt)
                If Counter = 1 And txt <> "" And txt <> "from" Then PropertyName = PropertyName + txt
                If Counter = 2 And txt <> "" Then
                    Classname = txt
                    Exit For
                End If
                If txt = "select" Then Counter = 1
                If txt = "from" Then Counter = 2
            Next
            PropertyName = Trim(PropertyName)
            Classname = Trim(Classname)

            Dim oMs As ManagementScope
            oMs = New ManagementScope(wmiPath, connectionOptions)
            Dim wmiClass As ManagementClass = New ManagementClass(Classname)
            WMIParamerers.Clear()
            For Each prop As PropertyData In wmiClass.Properties
                If WMIParamerersFields.Item(0) <> "*" Then
                    For Each ItemToSearch As String In WMIParamerersFields
                        If LCase(ItemToSearch) = LCase(prop.Name) Then
                            dt.Columns.Add(Trim(prop.Name), GetType(String))
                            WMIParamerers.Add(Trim(prop.Name))
                        End If
                    Next
                Else
                    dt.Columns.Add(Trim(prop.Name), GetType(String))
                    WMIParamerers.Add(Trim(prop.Name))
                End If
            Next

            Dim exittime As Date = DateAdd(DateInterval.Second, TimeInSeconds, Date.UtcNow)
            Dim exitc As Boolean = False

            If UserName <> "" Then
                oMs = New ManagementScope(wmiPath, connectionOptions)
            Else
                oMs = New ManagementScope(wmiPath)
            End If

            Dim oQuery As ObjectQuery = New System.Management.ObjectQuery(Query)
            Dim oSearcher As ManagementObjectSearcher = New ManagementObjectSearcher(oMs, oQuery)
            Dim oReturnCollection As ManagementObjectCollection = oSearcher.Get()

            For Each oReturn As ManagementObject In oReturnCollection
                For Each WMIParamerer As String In WMIParamerers
                    items.Add(oReturn(Trim(WMIParamerer)))
                    If exittime < Date.UtcNow Then Exit For
                Next

                If items.Count > 0 Then
                    dt.Rows.Add(items.ToArray())
                    items.Clear()
                End If
                If exittime < Date.UtcNow Then Exit For
            Next

            Return Me.GenerateActivityResult(dt)

        End Function


    End Class
End Namespace

