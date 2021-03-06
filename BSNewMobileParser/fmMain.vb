Imports System.Data.SqlClient
Imports System.IO
Imports System.Net
Imports System.Security.AccessControl
Imports System.Windows.Forms
Imports MySql.Data.MySqlClient


Public Class fmMain
    Public dg As New clsDataGetter(My.Settings.BluesheetsLocalConnectionString)

    Public StartRecp As Integer
    Public EndRecp As Integer

    Dim gotALTaxLien As Boolean = False
    Dim gotUSTaxLien As Boolean = False
    Dim gotForeclosureDeed As Boolean = False

    Dim origInst As String
    Dim gotOrigDoc As Boolean = False


    Const TBSNO = 9999
    Private Sub fmMain_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        'CheckForUpdate()
        Text = "Bluesheet CLOUD Mobile Doc Parser v." + My.Application.Info.Version.ToString
        DelIECache()
        DeleteScanneddocs()
    End Sub

    Private Sub btnMobile_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnMobile.Click
        btnClose.Enabled = False
        Me.CheckBox1.Checked = True
        If ValidateDupes(1) Then
            StartRecp = tbStartRecp.Text
            EndRecp = Me.tbEndRecp.Text
            CurrTBSDate = ConvertDate(dpTBSDate.Value.ToShortDateString)
            ParseMobileDocRange()
        End If
        MsgBox("Done parsing - you can close the parser")
        btnClose.Enabled = True
    End Sub

    Private Sub ParseMobileDocRange()
        Dim wr As New System.Net.WebClient
        Dim i As Integer
        Dim EndCounter As Integer = 1
        Dim tBodyCount As Integer = 0
        Dim docType As String

        CountyID = 1
        ShowStatus("Setting next reciept")
        ShowStatus("processing Bluesheet " + Str(TBSNO) + " from " + Str(StartRecp))

        For i = StartRecp To EndRecp
            Try
                DelIECache()
            Catch ex As Exception

            End Try

            If isAborted Then
                Exit For
            End If

            currInst = Str(i)

            ShowStatus("checking inst no " + Str(i))

            Dim fmML As New fmWebViewer
            fmML.currInstStr = currInst
            fmML.ShowDialog()
            Try
                GetDocumentImage(fmML.currSessionString)
                gotOrigDoc = True
            Catch ex As Exception
                gotOrigDoc = False
            End Try
            docType = GetMobileDocType()
            AddMobileInstrument(docType)
            fmML.Dispose()
            fmML = Nothing
        Next
    End Sub
    Private Function GetMobileDocType() As String
        Dim retDocType As String = ""
        Try

            Dim fName As String = "c:\inetpub\wwwroot\scanneddocs\mPage" + Trim(currInst) + ".htm"
            Dim treader = New StreamReader(File.OpenRead(fName))

            Dim idex As Integer = 0

            Dim wb As New WebBrowser
            wb.ScriptErrorsSuppressed = True
            wb.Navigate("about:blank")

            Dim hDocALL As HtmlDocument = wb.Document.OpenNew(True)

            While Not treader.EndOfStream
                hDocALL.Write(treader.ReadLine)
            End While
            Dim element As HtmlElement

            element = hDocALL.GetElementById("Document")
            Dim htmlStr As String = element.InnerHtml

            Dim sr As New StringReader(htmlStr)
            Dim line As String = "TD"
            Dim lineCount As Integer = 0
            While Not line Is Nothing
                line = sr.ReadLine
                If Not line Is Nothing Then
                    If line.Contains("TD") Then
                        Select Case lineCount
                            Case 3
                                retDocType = StripHTML(line)
                                Exit While
                        End Select
                        lineCount += 1
                    End If
                End If
            End While
            treader.Close()
            treader.Dispose()
            treader = Nothing
        Catch ex As Exception

        End Try

        Return retDocType
    End Function

    Private Function AddMobileInstrument(ByVal dType As String) As String
        Dim DocType As String

        DocType = dType

        If dg.HasData("SELECT * from dontadd WHERE doctitle = '" + DocType + "'") Then Return ""

        If DocType.Contains("MAP") Then
            DocType = "MAPS"
        End If

        Select Case DocType
            Case "AGMT"
                ShowStatus("an agreement...")
                Dim AG = New MasterDoc(BSDocConstants.bsDocType.bsMobileDoc, currInst, "Agreement")
                AG.ProcessDocument()
                AG.TableName = "AG"
                AG.AuxTable = 1
                AG.Desc = "An Agreement between " + AG.Grantor + " and " + AG.Grantee
                'ConvertFile(currInst)
                AG.AddToDatabase()
                AG = Nothing
                Return True
            Case "AMD", "PACN", "PAMD", "NPAMD", "LACN", "LAMD"
                ShowStatus("an amendment...")
                Dim AM = New MasterDoc(BSDocConstants.bsDocType.bsDeltaDoc, currInst, "Amendment")
                AM.ProcessDocument()
                AM.TableName = "AM"
                AM.AuxTable = 1
                AM.Desc = AM.Grantor + " name change"
                'ConvertFile(currInst)
                AM.AddToDatabase()
                AM = Nothing
                Return True
                Return True
            Case "PINC"
                ShowStatus("an art of incorp...")
                Dim AI = New MasterDoc(BSDocConstants.bsDocType.bsDeltaDoc, currInst, "Articles of Incorporation, Non Profit")
                AI.ProcessDocument()
                AI.TableName = "AI"
                AI.AuxTable = 1
                AI.Desc = AI.Grantor + "- Purpose is to transact any and all lawful business"
                'ConvertFile(currInst)
                AI.AddToDatabase()
                AI = Nothing
                Return True
                Return True
            Case "LAOR", "NPINC"
                ShowStatus("an art. of organization...")
                Dim AO = New MasterDoc(BSDocConstants.bsDocType.bsDeltaDoc, currInst, "Article of Organization")
                AO.ProcessDocument()
                AO.TableName = "AO"
                AO.AuxTable = 1
                AO.Desc = AO.Grantor + "- Purpose is to transact any and all lawful business"
                'ConvertFile(currInst)
                AO.AddToDatabase()
                AO = Nothing
                Return True
            Case "D"
                ShowStatus("a deed...")
                Dim DEED = New MasterDoc(BSDocConstants.bsDocType.bsDeltaDoc, currInst, "Deed")
                DEED.ProcessDocument()
                DEED.TableName = "D"
                DEED.AuxTable = 2
                PendingDeedInstrument = Val(currInst)
                If DEED.Desc = "" Or DEED.Desc.Contains("&nbsp;") Then
                    DEED.Desc = DEED.Subdivision
                End If
                'ConvertFile(currInst)
                DEED.AddToDatabase()
                DEED = Nothing
                Return True
            Case "MAPS"
                ShowStatus("an map...")
                Dim MAP = New MasterDoc(BSDocConstants.bsDocType.bsDeltaDoc, currInst, "Agreement")
                MAP.ProcessDocument()
                MAP.TableName = "MAP"
                MAP.AuxTable = 1
                'ConvertFile(currInst)
                MAP.AddToDatabase()
                MAP = Nothing
                Return True

            Case "M"
                ShowStatus("a mortgage...")
                Dim MORT = New MortgageDoc(BSDocConstants.bsDocType.bsDeltaDoc, currInst, "Mortgage")
                MORT.ProcessDocument()
                'ConvertFile(currInst, "M", False, True)
                MORT.AuxTable = 2
                ' MORT.Value = GetMortAmount(currInst)
                MORT.AddToDatabaseMort()
                MORT = Nothing
                Return True
            Case "COJ"
                ShowStatus("a judgement...")
                Dim JUDGE = New MasterDoc(BSDocConstants.bsDocType.bsDeltaDoc, currInst, "Judgement")
                JUDGE.ProcessDocument()
                JUDGE.TableName = "J"
                JUDGE.AuxTable = 5

                'SMTODO
                If gotOrigDoc Then
                    If ConvertFile(currInst, "", False, True) Then
                        JUDGE.Value = GetBCJudgeAmount(currInst)
                        JUDGE.CaseNo = GetBCJudgeCase(currInst)
                    End If
                Else
                    'ConvertFile(currInst)
                End If
                'CheckAndRotate(currInst, "J")

                JUDGE.AddToDatabase()
                JUDGE = Nothing
                Return True
            Case "REL", "UTRM", "UPR"
                ShowStatus("a release..")
                Dim REL = New RDoc(BSDocConstants.bsDocType.bsDeltaDoc, currInst, "Release")
                REL.ProcessDocument()
                Select Case REL.TableName
                    Case "UR"
                        If ConvertFile(currInst, "UR", False, True) Then
                            REL.PrevInst = GetUSRelPrevInst(currInst)
                        End If
                    Case Else
                        'ConvertFile(currInst)
                End Select

                REL.AddToDatabaseRel()
                REL = Nothing
                Return True
            Case "LISP"
                ShowStatus("lis pendens..")
                Dim LIS = New MasterDoc(BSDocConstants.bsDocType.bsDeltaDoc, currInst, "Lis Pendens")
                LIS.ProcessDocument()
                LIS.TableName = "LIS"
                LIS.AuxTable = 1
                'ConvertFile(currInst)
                LIS.AddToDatabase()
                LIS = Nothing
                Return True
            Case "LDIS", "PDIS"
                ShowStatus("a dissolution..")
                Dim DI = New MasterDoc(BSDocConstants.bsDocType.bsDeltaDoc, currInst, "Dissolution")
                DI.ProcessDocument()
                DI.TableName = "DI"
                DI.AuxTable = 1
                DI.Desc = DI.Grantor + " has been dissolved"
                'ConvertFile(currInst)
                DI.AddToDatabase()
                DI = Nothing
                Return True

            Case "MINI"
                Return True
            Case "H", "LIEN", "VLD"
                ShowStatus("a lien..")
                Dim LI = New LIDoc(BSDocConstants.bsDocType.bsDeltaDoc, currInst, "Lien")
                LI.AuxTable = 3
                LI.ProcessDocument()
                If LI.TableName = "ST" Then
                    If ConvertFile(currInst, LI.TableName, False, True) Then
                        LI.Value = GetALTaxAmount(currInst)
                        LI.KindTax = GetALKindTax(currInst)
                        LI.Address = GetALTaxAddress(currInst)
                        LI.AuxTable = 5
                    End If
                ElseIf LI.TableName = "H" Then
                    If ConvertFile(currInst, LI.TableName, False, True) Then
                        LI.Value = GetHospAmount(currInst, LI.Grantor)
                        LI.Desc += " " + LI.Value
                    End If
                ElseIf LI.TableName = "LI" Then
                    If ConvertFile(currInst, LI.TableName, False, True) Then
                        LI.Value = GetLienAmount(currInst, LI.Grantor)
                        LI.Desc += " " + LI.Value
                    End If
                Else
                    'ConvertFile(currInst)
                End If

                LI.AddToDatabase()
                LI = Nothing
                Return True
            Case "TAX LIEN", "UINF", "UTAX"
                ShowStatus("a tax lien..")
                Dim TLien = New TaxLienDoc(BSDocConstants.bsDocType.bsDeltaDoc, currInst, "Tax Lien")
                TLien.ProcessDocument()
                If ConvertFile(currInst, TLien.TableName, False, True) Then
                    Select Case TLien.TableName
                        Case "US"
                            TLien.KindTax = GetUSKindTax(currInst)
                            TLien.Value = GetUSTaxAmount(currInst)
                            TLien.Address = GetUSTaxAddress(currInst)
                        Case "ST"
                            TLien.KindTax = GetUSKindTax(currInst)
                            TLien.Value = GetUSTaxAmount(currInst)
                    End Select
                End If

                TLien.AddToDatabaseTaxLien()
                TLien = Nothing
                Return True
            Case "TRLE"
                ShowStatus("a lease..")
                Dim LE = New MasterDoc(BSDocConstants.bsDocType.bsDeltaDoc, currInst, "Lease")
                LE.ProcessDocument()
                LE.TableName = "LE"
                LE.AuxTable = 1
                'ConvertFile(currInst)
                LE.AddToDatabase()
                LE = Nothing
                Return True
            Case "ORDR"
                ShowStatus("an order..")
                Dim ORD = New MasterDoc(BSDocConstants.bsDocType.bsDeltaDoc, currInst, "Order")
                ORD.ProcessDocument()
                ORD.TableName = "ORD"
                ORD.AuxTable = 1
                'ConvertFile(currInst)
                ORD.AddToDatabase()
                ORD = Nothing
                Return True
            Case Else
                Dim MISC As New MasterDoc(BSDocConstants.bsDocType.bsDeltaDoc, currInst, "Misc")
                MISC.ProcessDocument()
                MISC.TableName = "??"
                MISC.DocType = DocType
                'ConvertFile(currInst)
                MISC.AddToDatabase()
        End Select

    End Function

    Private Function ValidateDupes(ByVal cid As Integer) As Boolean
        If cid = 1 Then
            If dg.HasData("SELECT * FROM instrumentmasterflat WHERE InstrumentNo BETWEEN " + Me.tbStartRecp.Text + " AND " + Me.tbEndRecp.Text) Then
                If MsgBox("Instrument range has existing Instruments in it - delete and reparse?", MsgBoxStyle.YesNo) = MsgBoxResult.Yes Then
                    dg.RunCommand("DELETE FROM instrumentmasterflat WHERE InstrumentNo BETWEEN " + Me.tbStartRecp.Text + " AND " + Me.tbEndRecp.Text)
                Else
                    Return False
                End If
                Return True
            End If
        End If
        Return True
        dg.Close()
    End Function

    Public Sub DelIECache()

        Dim path As String = Environment.GetFolderPath(Environment.SpecialFolder.InternetCache) + "\Content.IE5"
        ShowStatus(path)
        Dim dInfo, dInfos() As DirectoryInfo
        dInfo = New DirectoryInfo(path)
        Dim FolderAcl As New DirectorySecurity
        FolderAcl.AddAccessRule(New FileSystemAccessRule("Everyone", FileSystemRights.Modify, InheritanceFlags.ContainerInherit Or InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow))
        FolderAcl.SetAccessRuleProtection(True, False) 'uncomment to remove existing permissions
        dInfo.SetAccessControl(FolderAcl)
        dInfos = dInfo.GetDirectories()
        For i = 0 To dInfos.Count - 1
            dInfos(i).Delete(True)
        Next
    End Sub

    Private Sub GetDocumentImage(session As String)
        'Dim fName As String = "c:\inetpub\wwwroot\scanneddocs\" + Trim(Str(currInst)) + ".pdf"
        'Dim cookieStr As String ' = BuildCookieStr("2017023712")
        'Dim instStr As String = Trim(Str(currInst))
        'Dim url As String = "https://roam.probate.mobilecountyal.gov/ailis/search.do?indexName=mobimages&lq=Instrument%3A" + instStr
        ''url += "&nodeName=RECORDSMANAGEMENT&clientName=MOBILE&ecomServerURL=https%3A%2F%2Feaccept.granicus.com%2Fecom%2F&ecomClientURL=https%3A%2F%2Froam.probate.mobilecountyal.gov%2Failis%2F&defaultSiteName=MOBILE&isFirmNumberRequired=false&isBarNumberRequired=false&isAuthorizationPending=false&nodeName=RECORDSMANAGEMENT&clientName=MOBILE&ecomServerURL=https%3A%2F%2Feaccept.granicus.com%2Fecom%2F&ecomClientURL=https%3A%2F%2Froam.probate.mobilecountyal.gov%2Failis%2F&defaultSiteName=MOBILE&isFirmNumberRequired=false&isBarNumberRequired=false&isAuthorizationPending=false"
        'Dim request As WebRequest = WebRequest.Create(url)
        'Dim mycache As New CredentialCache()
        'mycache.Add(New Uri(url), "Basic", New NetworkCredential("stumay111@gmail.com", "shadow111"))
        'request.Credentials = mycache
        ''        cookieStr = BuildCookieStr(instStr, session)
        'cookieStr = "JSESSIONID=584A4CCEDA829EE78E1DF1AB0D4D67BB; Mainq=; bso=NameSort%7CN; oprrecentQueries=2; oprrecentQuery0=%26lq%3D; oprrecentQuery1=2017023712+%26lq%3D; username=stumay111%40gmail.com; searchstring=%26searchType%3D1%26q%3D2017023712+; _ga=GA1.2.1073938819.1495307666; _gid=GA1.2.1581532915.1495370605"
        'request.Headers.Add("Cookie", cookieStr)

        'Dim response As WebResponse = request.GetResponse()
        'Dim stream As System.IO.FileStream = System.IO.File.Create(fName)
        'response.GetResponseStream().CopyTo(stream)

        'Dim path As String = Environment.GetFolderPath(Environment.SpecialFolder.InternetCache) + "\Content.IE5"
        'ShowStatus(path)
        'Dim fInfo, fInfos() As FileInfo
        'Dim dInfo, dInfos() As DirectoryInfo
        'dInfo = New DirectoryInfo(path)
        'Dim FolderAcl As New DirectorySecurity
        'FolderAcl.AddAccessRule(New FileSystemAccessRule("Everyone", FileSystemRights.Modify, InheritanceFlags.ContainerInherit Or InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow))
        'FolderAcl.SetAccessRuleProtection(True, False) 'uncomment to remove existing permissions
        'dInfo.SetAccessControl(FolderAcl)
        'dInfos = dInfo.GetDirectories()

        'For i = 0 To dInfos.Count
        '    fInfos = dInfos(i).GetFiles("*")
        '    For Each fInfo In fInfos
        '        If fInfo.Name.Contains("document") And fInfo.Extension = ".pdf" Then
        '            File.Copy(fInfo.FullName, fName, True)
        '            File.Delete(fInfo.FullName)
        '            Exit For
        '        End If
        '    Next
        '    Exit For
        'Next



        'DI = Nothing

    End Sub

    Private Function BuildCookieStr(instStr As String, session As String) As String
        Dim cookieStr As String = "JSESSIONID = " + session + "; "
        cookieStr += "Mainq=" + instStr + "%20; "
        cookieStr += "bso=NameSort%7CN; "
        cookieStr += "oprrecentQuery0=" + instStr + "+%26Lq%3D; "
        cookieStr += "oprrecentQueries=1; "
        cookieStr += "searchstring=searchQuery%3D" + instStr + "+%26SearchType%3D1%26q%3D" + instStr
        cookieStr += "+%26Searchable%3DDisplayName%252CDisplayNameStarts%"
        cookieStr += "252CDisplayNameExact%252CInstrument%252CPartyRole"
        cookieStr += "%252CRecDate%252CBook%252CPage%252CDocTypeDesc"
        cookieStr += "%252CDocTypeCode%252CBlock%252CPhase%252CSection"
        cookieStr += "%252CLotNum%252CLot%252CMapBook%252CMapPage"
        cookieStr += "%252CSubdivision%252CFreeForm1Other"
        cookieStr += "%252CPID%252CFirstReference"
        cookieStr += "%252CConsideration"
        cookieStr += "%252CLegalDescription%26SortBy"
        cookieStr += "%3DNameSort%26Desc%3DN; "
        cookieStr += "username=stumay111%40gmail.com; "
        cookieStr += "_ga=GA1.2.1073938819.1495307666; _gid=GA1.2.1054930370.1495361401"
        Return cookieStr

    End Function
    Private Sub btnClose_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnClose.Click
        Close()
    End Sub

    Private Sub Button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button2.Click
        isAborted = True
    End Sub


    Private Sub CheckForUpdate()
        'Dim myVersion As String = My.Application.Info.Version.ToString
        'Dim currVersion As String = Trim(dg.GetScalarString("SELECT currVersion FROM currVersions WHERE appName='docParser'"))
        'If currVersion <> myVersion Then
        '    MsgBox("Doc Parser needs to be updated - YOU MUST RESTART THE APPLICATION AFTER UPDATE.")
        '    System.Diagnostics.Process.Start("http://www.thebluesheetonline.com/installs/BSDocParser.msi")
        'End If
    End Sub

    Private Sub DeleteScanneddocs()
        Dim dir As New DirectoryInfo("c:\inetpub\wwwroot\scanneddocs")
        Dim fl As FileInfo
        Dim fls() As FileInfo
        fls = dir.GetFiles("*.tif")
        Try
            For Each fl In fls
                File.Delete(fl.FullName)
            Next
        Catch ex As Exception

        End Try

        fls = dir.GetFiles("*.txt")
        Try
            For Each fl In fls
                File.Delete(fl.FullName)
            Next
        Catch ex As Exception

        End Try


        fls = dir.GetFiles("*.htm")
        Try
            For Each fl In fls
                File.Delete(fl.FullName)
            Next
        Catch ex As Exception

        End Try


        fls = dir.GetFiles("*crp.tif")
        Try
            For Each fl In fls
                File.Delete(fl.FullName)
            Next
        Catch ex As Exception

        End Try
        dir = Nothing
    End Sub


    Private Sub tbStartRecp_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles tbStartRecp.TextChanged

    End Sub
End Class
