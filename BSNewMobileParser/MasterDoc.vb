Imports System.IO
Imports System.Text.RegularExpressions

Public Class MasterDoc
    Public DocType As String = ""
    Public AuxTable As String = "1"
    Public TBSDate As String = ConvertDate(CurrTBSDate)
    Public NotaryDate As String
    Public RecDate As String
    Public Grantor As String = ""
    Public Grantee As String = ""
    Public Address As String = ""
    Public DeedBook As String = ""
    Public Value As String = ""
    Public MineralAcres As String = ""
    Public Lots As String = ""
    Public DownPayment As String = "0"
    Public STR As String = ""
    Public Section As String = ""
    Public TownRange As String = ""
    Public Subdivision As String = ""
    Public Lot As String = ""
    Public Block As String = ""
    Public Remarks As String = ""
    Public DocFile As String = ""
    Public DeedTax As String = ""
    Public LegalDescription As String = ""
    Public Desc As String = ""
    Public PrevInst As String = ""
    Public dontAdd As Boolean = False
    Public TableName As String = ""
    Public DeedType As String = ""

    Public City As String = ""
    Public State As String = ""
    Public Zip As String = ""
    Public KindTax As String = ""
    Public CaseNo As String = ""

    Protected ScannedText As String

    Public rdr As StreamReader

    Public dg As New clsDataGetter(My.Settings.BlueSheetsLocalConnectionString)

    Public Sub New(ByVal dType As bsDocType, ByVal currInst As String, ByVal dName As String)
        DocType = dName
        currInst = currInst
    End Sub
    Public Sub ProcessDocument()
        Try
            rdr = New StreamReader(File.OpenRead("c:\inetpub\wwwroot\scanneddocs\mPage" + Trim(currInst) + ".htm"))
            ParseInstrumentDoc(rdr)
            Grantee = ConvertToTitle(Grantee)
            Grantor = ConvertToTitle(Grantor)
            Address = ConvertToTitle(Address)
            Desc = ConvertToTitle(Desc)
            Subdivision = ConvertToTitle(Subdivision)

            Grantee = ReplaceFromTable(Grantee)
            Grantor = ReplaceFromTable(Grantor)
            Address = ReplaceFromTable(Address)
            Desc = ReplaceFromTable(Desc)
            Subdivision = ReplaceFromTable(Subdivision)


        Catch ex As Exception
            LogError(ex.Message)
        End Try
    End Sub

    Public Function ParseInstrumentDoc(ByVal rdr As StreamReader) As String
        Dim gotForeclosureDeed As Boolean = False
        Dim gotUSTaxLien As Integer = -1
        Dim gotALTaxLien As Integer = -1
        Dim reader As StreamReader


        If Not dg.HasData("SELECT DocTitle FROM dontadd WHERE DocTitle='" + DocType + "'") Then
            Dim imgURL As String = "https://roam.probate.mobilecountyal.gov/ailis/search.do?indexName=mobimages&lq=Instrument%3A" + currInst

            DocFile = "http://74.208.132.102/scanneddocs/" + Trim(currInst) + ".pdf"

            reader = rdr

            Dim idex As Integer = 0

            Dim wb As New WebBrowser
            wb.ScriptErrorsSuppressed = True
            wb.Navigate("about:blank")

            Dim hDocALL As HtmlDocument = wb.Document.OpenNew(True)

            While Not reader.EndOfStream
                hDocALL.Write(reader.ReadLine)
            End While
            Dim element As HtmlElement

            element = hDocALL.GetElementById("Document")
            Dim htmlStr As String = element.InnerHtml

            Dim sr As New StringReader(htmlStr)
            Dim line As String = "TD"
            Dim pLine As String = ""

            Dim lineCount As Integer = 0
            While Not line Is Nothing
                line = sr.ReadLine
                If Not line Is Nothing Then
                    If line.Contains("TD") Then
                        Select Case lineCount
                            Case 0
                                currInst = StripHTML(line) '1
                                lineCount += 1
                            Case 1
                                NotaryDate = StripHTML(line) '2
                                lineCount += 1
                            Case 2
                                '    cinstObj.Desc = StripHTML(line) '3
                                lineCount += 1
                            Case 3
                                DocType = StripHTML(line)
                                lineCount += 1
                        End Select
                    End If
                End If
            End While

            element = hDocALL.GetElementById("Document2")
            htmlStr = element.InnerHtml

            sr = New StringReader(htmlStr)
            line = "TD"
            pLine = ""

            lineCount = 0
            While Not line Is Nothing
                line = sr.ReadLine
                If Not line Is Nothing Then
                    If line.Contains("TD") Then
                        lineCount += 1

                        Select Case lineCount
                            Case 3
                                DeedTax = StripHTML(line) '1
                                Exit While
                        End Select
                    End If
                End If
            End While

            element = hDocALL.GetElementById("legal")
            If Not element Is Nothing Then

                htmlStr = element.InnerHtml

                sr = New StringReader(htmlStr)
                line = "TD"
                pLine = ""

                lineCount = 0
                While Not line Is Nothing
                    line = sr.ReadLine
                    If Not line Is Nothing Then
                        If line.Contains("TD") Then
                            lineCount += 1
                            Select Case lineCount
                                Case 9
                                    Subdivision = StripHTML(line) '1
                                Case 10
                                    Desc = StripHTML(line) '1
                                    Exit While
                            End Select
                        End If
                    End If
                End While
            End If


            element = hDocALL.GetElementById("entity")
            If Not element Is Nothing Then

                htmlStr = element.InnerHtml
                sr = Nothing
                sr = New StringReader(htmlStr)
                Dim totalLines As Integer = (htmlStr.Split(vbCrLf).Length)

                line = "TD"
                lineCount = 0
                While lineCount < 100
                    pLine = line
                    line = sr.ReadLine
                    lineCount += 1
                    If Not line Is Nothing Then
                        If line.Contains("Grantor") Then
                            Grantor = StripHTML(pLine) '1
                            If DocType = "LAOR" Or DocType = "AFFS" Then
                                Exit While
                            End If
                        ElseIf line.Contains("Grantee") Then
                            Grantee = StripHTML(pLine) '2
                            Exit While
                        End If
                    End If
                End While
            End If

            element = hDocALL.GetElementById("docref")
            If Not element Is Nothing Then
                htmlStr = element.InnerHtml
                sr = Nothing
                sr = New StringReader(htmlStr)


                line = "TD"
                lineCount = 0
                While True
                    line = sr.ReadLine
                    If Not line Is Nothing Then
                        If line.Contains("TD") Then
                            Select Case lineCount
                                Case 1
                                    PrevInst = StripHTML(line) '1
                                    Exit While
                            End Select
                            lineCount += 1
                        End If
                    End If
                End While
            End If
            reader.Close()
            reader = Nothing
        End If
    End Function

    Public Sub AddToDatabase()

        If dontAdd Then Exit Sub
        Dim cmdStr As String = "INSERT INTO instrumentmasterflat("
        cmdStr = cmdStr + "InstrumentNo, County_ID, DocType,"
        cmdStr = cmdStr + "TableName, Grantor, Grantee, "
        cmdStr = cmdStr + "AuxTableType, NotaryDate, Remarks,"
        cmdStr = cmdStr + "DocFileName, Description, TBSNo,"
        cmdStr = cmdStr + "TBSDate,"
        cmdStr = cmdStr + "Address, Address2, DownPayment, Amount,"
        cmdStr = cmdStr + "DeedTax, Sect, TownRange, Subdivision,"
        cmdStr = cmdStr + "Lot, City, State, Zip, KindTax,CaseNo,PrevInst,DeedType)"

        cmdStr = cmdStr + "VALUES('"
        cmdStr = cmdStr + CStr(currInst) + "',"
        cmdStr = cmdStr + CStr(CountyID) + ",'"
        cmdStr = cmdStr + DocType + "','"
        cmdStr = cmdStr + TableName + "','"
        cmdStr = cmdStr + FixSingleQuote(Grantor) + "','"
        cmdStr = cmdStr + FixSingleQuote(Grantee) + "',"
        cmdStr = cmdStr + AuxTable + ",'"
        cmdStr = cmdStr + NotaryDate + "','"
        cmdStr = cmdStr + Remarks + "','"
        cmdStr = cmdStr + DocFile + "','"
        cmdStr = cmdStr + FixSingleQuote(Desc) + "',"
        cmdStr = cmdStr + CStr(TBSNo) + ",'"
        cmdStr = cmdStr + TBSDate + "','"
        cmdStr = cmdStr + Desc + "','"
        cmdStr = cmdStr + "" + "','"
        cmdStr = cmdStr + DownPayment + "','"
        cmdStr = cmdStr + Value + "','"
        cmdStr = cmdStr + DeedTax + "','"
        cmdStr = cmdStr + Section + "','"
        cmdStr = cmdStr + TownRange + "','"
        cmdStr = cmdStr + FixSingleQuote(Subdivision) + "','"
        cmdStr = cmdStr + Lot + "','"
        cmdStr = cmdStr + City + "','"
        cmdStr = cmdStr + State + "','"
        cmdStr = cmdStr + Zip + "','"
        cmdStr = cmdStr + KindTax + "','"
        cmdStr = cmdStr + CaseNo + "','"
        cmdStr = cmdStr + PrevInst + "','"
        cmdStr = cmdStr + DeedType + "')"

        Try
            dg.RunCommand(cmdStr)
        Catch ex As Exception
            LogError(ex.Message)
        End Try
    End Sub


    Protected Sub FixSingleQuotes()
        Grantor = FixSingleQuote(Grantor)
        Grantee = FixSingleQuote(Grantee)
        Desc = FixSingleQuote(Desc)
        Address = FixSingleQuote(Address)
        Subdivision = FixSingleQuote(Subdivision)
        Remarks = FixSingleQuote(Remarks)
        LegalDescription = FixSingleQuote(LegalDescription)
    End Sub

End Class
