Imports System.Text.RegularExpressions
Imports System.Drawing
Imports MySql.Data.MySqlClient
Imports Bytescout.PDFRenderer
Imports System.IO


Public Module bsCommonProcs

    Public isAborted As Boolean = False

    Public CurrTBSDate As Date

    Public PendingDeedInstrument As Integer = 0
    Public excludeList As ArrayList

    Public CountyID As Integer


    Public Sub ShowStatus(ByVal s As String)
        fmMain.ListBox1.Items.Add(s)
        fmMain.ListBox1.TopIndex = fmMain.ListBox1.Items.Count - 1

    End Sub

    Public Function StripHTML(ByVal s As String) As String
        If s Is Nothing Then
            Return ""
        End If
        s = s.Replace("<TD align=right>", "")
        s = s.Replace("<TD>", "")
        s = s.Replace("</TD>", "")
        If s.Contains("</A>") Then
            Dim parts() As String
            s = s.Replace("</A>", "")
            parts = s.Split(">")
            s = parts(UBound(parts))
        End If
        Return Trim(s)
    End Function

    Public Sub CheckAndRotate(ByVal CurrInst As String, ByVal TableName As String)

        Dim fName As String = "c:\inetpub\wwwroot\scanneddocs\" + Trim(CurrInst) + ".tif.txt"
        Dim sr As New System.IO.StreamReader(fName)
        Dim s As String = sr.ReadToEnd
        If TableName = "J" And CountyID = 0 Then
            If Not s.ToUpper.Contains("DEFENDANT") Then
                ConvertFile(CurrInst, "", True, True)
            End If
        End If
    End Sub

    Public Function ConvertFile(ByVal CurrInst As String, Optional ByVal TableName As String = "", Optional ByVal Rotate As Boolean = False, Optional ByVal doOCR As Boolean = False) As Boolean

        Dim imageConv As New MODI.Document
        Dim img As MODI.Image
        Dim i As Integer
        Dim xText As String = ""
        Dim fName As String
        Dim pdfName As String



        Try

            pdfName = "c:\inetpub\wwwroot\scanneddocs\" + Trim(CurrInst) + ".pdf"

            ConvertPDFToPNG(pdfName)
            If doOCR Then

                fName = "c:\inetpub\wwwroot\scanneddocs\" + Trim(CurrInst) + ".tif"

                'If TableName = "ST" Then

                '    Dim bmp As Bitmap = Bitmap.FromFile(fName)
                '    bmp = CropBitmap(bmp, 0, 0, CInt(bmp.Width / 2), bmp.Height)
                '    fName = Regex.Replace(fName, "\.\w*", "")
                '    fName = fName + "crp.tif"
                '    bmp.Save(fName, Imaging.ImageFormat.Png)
                '    bmp.Dispose()
                'ElseIf Rotate Then
                '    Dim bmp As Bitmap = Bitmap.FromFile(fName)
                '    bmp.RotateFlip(System.Drawing.RotateFlipType.Rotate90FlipNone)
                '    bmp.RotateFlip(System.Drawing.RotateFlipType.Rotate180FlipNone)
                '    fName = Regex.Replace(fName, "\.\w*", "")
                '    fName = fName + "crp.tif"
                '    bmp.Save(fName, Imaging.ImageFormat.Png)
                '    bmp.Dispose()
                'End If

                imageConv.Create(fName)

                imageConv.OCR(MODI.MiLANGUAGES.miLANG_ENGLISH, True, True)
                'imageConv.Save()
                For i = 0 To imageConv.Images.Count - 1
                    img = imageConv.Images(i)
                    xText += img.Layout.Text
                Next
                Dim fStream As New System.IO.StreamWriter(fName + ".txt")
                fStream.Write(xText)
                fStream.Close()
            End If
            Return True

        Catch ex As Exception
            Return False
        End Try
    End Function

    Private Sub ConvertPDFToPNG(ByVal fName As String)
        Dim renderer As New RasterRenderer()
        renderer.RegistrationName = "demo"
        renderer.RegistrationKey = "demo"

        ' Load PDF document.
        renderer.LoadDocumentFromFile(fName)
        Dim StartPageIndex As Integer = 0
        ' define page to end on
        Dim EndPageIndex As Integer = renderer.GetPageCount() - 1
        ' Render PDF to TIFF image file.
        renderer.RenderPageRangeToMultipageTIFFFile(StartPageIndex, EndPageIndex, TIFFCompression.JPEG, "c:\inetpub\wwwroot\scanneddocs\" + Trim(currInst) + ".tif")
    End Sub
    Public Sub LogItem(ByVal str As String)

    End Sub
    Private Function CropBitmap(ByRef bmp As Bitmap, ByVal cropX As Integer, ByVal cropY As Integer, ByVal cropWidth As Integer, ByVal cropHeight As Integer) As Bitmap
        Dim rect As New Rectangle(cropX, cropY, cropWidth, cropHeight)
        Dim cropped As Bitmap = bmp.Clone(rect, bmp.PixelFormat)
        Return cropped
    End Function
    Public Function ConvertDate(ByVal s As String) As String
        Dim dArray() As String
        If s = "/  /" Then
            Return "9999-12-31"
        End If

        Try
            dArray = s.Split("/")
            If dArray(2).Contains("AM") Or dArray(2).Contains("PM") Then
                dArray(2) = Mid(dArray(2), 1, 4)
            End If

            If Trim(dArray(0)).Length = 1 Then
                dArray(0) = "0" + Trim(dArray(0))
            End If

            If Trim(dArray(1)).Length = 1 Then
                dArray(1) = "0" + Trim(dArray(1))
            End If

            'ConvertDate = New Date(CInt(dArray(2)), CInt(dArray(0)), CInt(dArray(1)))
            ConvertDate = dArray(2) + "-" + dArray(0) + "-" + dArray(1)
            Return ConvertDate



        Catch ex As Exception
            'ConvertDate = New Date(9999, 12, 31)
            ConvertDate = "9999-12-31"
        End Try
    End Function
    Public Function DownloadDeltaDocument(ByVal s As String) As String
        Dim wr As New System.Net.WebClient
        Dim FileName As String = ""
        Dim begFileName As Integer
        Dim endFileName As Integer

        Try
            begFileName = InStr(s, "a href=") + 8
            endFileName = InStr(s, "target")
            FileName = Mid(s, begFileName, (endFileName - begFileName) - 2)
            FileName = "http://www.deltacomputersystems.com" + FileName

            ShowStatus(FileName + "downld")

            wr.DownloadFile(FileName, "c:\inetpub\wwwroot\scanneddocs\" + Trim(currInst) + ".tif")
        Catch ex As Exception
            LogItem("couldn't download image of Inst No: " + Str(currInst))
        End Try
        Return FileName
    End Function


    Public Function MidString(ByVal s As String, Optional ByVal startStr As String = "", Optional ByVal endStr As String = "") As String
        Dim startPos As Integer
        Dim endPos As Integer
        Dim retStr As String = ""


        Dim str1 As String
        Dim str2 As String
        Dim str3 As String

        If startStr = "" Then
            startPos = 1
        Else
            startPos = s.IndexOf(startStr) + startStr.Length

        End If

        If endStr = "" Then
            endPos = s.Length
        Else
            endPos = s.IndexOf(endStr)
        End If

        Try
            str1 = s.Substring(0, startPos)
            str2 = s.Substring(startPos, s.Length - startPos)
            If endPos > 0 Then
                str3 = s.Substring(endPos, s.Length - endPos)
                retStr = Trim(Mid(str2, 1, s.Length - str3.Length - str1.Length))
            Else
                retStr = Trim(Mid(str2, 1, s.Length - str1.Length))
            End If
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try '
        Return retStr
    End Function

    Public Sub LogError(ByVal s As String)
        ShowStatus(s)
    End Sub

    Public Function SwapNames(ByVal nameStr As String) As String
        Dim names() As String
        Dim i As Integer
        Dim AllNames As New ArrayList
        Dim fName() As String
        Dim sName() As String
        Dim retStr As String = ""

        nameStr = Regex.Replace(nameStr, " And ", "&")


        names = Trim(nameStr).Split("&")

        For i = 0 To UBound(names)
            AllNames.Add(SwapName(names(i)))
        Next

        For i = 0 To AllNames.Count - 1
            If AllNames.Count = 1 Then Return AllNames(0)
            If AllNames.Count > 1 Then
                Dim j As Integer
                For j = 1 To AllNames.Count - 1
                    Dim fLName As String
                    Dim sLName As String
                    fName = AllNames(j - 1).split(" ")
                    sName = AllNames(j).Split(" ")
                    If fName(UBound(fName)).ToUpper = "JR." Or fName(UBound(fName)).ToUpper = "JR" Or fName(UBound(fName)).ToUpper = "III" Or fName(UBound(fName)).ToUpper = "IV" Then
                        fLName = fName(UBound(fName) - 1)
                    Else
                        fLName = fName(UBound(fName))
                    End If
                    sLName = sName(UBound(sName))
                    If fLName = sLName Then
                        If UBound(sName) > j - 1 Then
                            AllNames(j) = sName(j - 1)
                        Else
                            AllNames(j) = sName(UBound(sName))
                        End If
                    End If

                Next
            End If

            If i = 0 And i < AllNames.Count Then
                retStr = retStr + AllNames(i) + " and "
            Else
                retStr = retStr + " " + AllNames(i)
            End If
        Next
        Return retStr
    End Function

    Public Function SwapName(ByVal nameStr As String) As String
        Dim busList As ArrayList = getBusinessNameList()
        Dim i As Integer
        For i = 0 To busList.Count - 1
            If nameStr.ToUpper.Contains(busList(i)) Then
                If nameStr.ToUpper.Contains(" LLC ") Then
                    Microsoft.VisualBasic.Strings.Replace(nameStr, "LLC", "LLC", 1, -1, Constants.vbTextCompare)
                End If
                Return nameStr
            End If
        Next

        Dim names() As String = Trim(nameStr).Split(" ")
        Dim tName As String = nameStr
        If nameStr.ToUpper.Contains("CITY OF") Then
            If nameStr.ToUpper.EndsWith("OF") Then
                nameStr = nameStr.Substring(7, nameStr.Length - 7)
                nameStr = "The City of " + Trim(nameStr)
                Return nameStr
            End If

        End If
        If nameStr.ToUpper.Contains("STATE OF") Then
            nameStr = nameStr.Substring(8, nameStr.Length - 8)
            nameStr = "The State of " + Trim(nameStr)
            Return nameStr
        End If

        If nameStr.ToUpper.Contains("UNITED STATES") Then
            nameStr = "United States of America"
            Return nameStr
        End If
        Select Case UBound(names)
            Case 1
                tName = names(1) + " " + names(0)
            Case 2
                If names(2).Length = 1 Then names(2) = names(2).ToUpper
                tName = names(1) + " " + names(2) + " " + names(0)
            Case 3
                If names(2).Length = 1 Then names(2) = names(2).ToUpper
                If names(3) = "Iii" Or names(3) = "Iv" Then names(3) = names(3).ToUpper
                tName = names(1) + " " + names(2) + " " + names(0) + " " + names(3)

        End Select
        If Regex.Match(tName, "O'").Success Then
            Dim tNameArray() As Char = tName.ToCharArray
            Dim tName2 As String = ""
            Dim isBreak As Boolean = False
            For i = 0 To UBound(tNameArray)
                If i > 2 Then
                    If tNameArray(i - 2) = "'" Then
                        If tNameArray(i - 3) = "O" Then
                            isBreak = True
                        End If
                    End If
                End If
                If Not isBreak Then
                    tName2 += tNameArray(i)
                Else
                    tName2 += Char.ToUpper(tNameArray(i))
                    isBreak = False
                End If
            Next
            tName = tName2
        End If

        If tName.Contains("Mc") Then
            Dim tNameArray() As Char = tName.ToCharArray
            Dim tName2 As String = ""
            Dim isBreak As Boolean = False
            For i = 0 To UBound(tNameArray)
                If i > 1 Then
                    If tNameArray(i - 1) = "c" Then
                        If tNameArray(i - 2) = "M" Then
                            isBreak = True
                        End If
                    End If
                End If
                If Not isBreak Then
                    tName2 += tNameArray(i)
                Else
                    tName2 += Char.ToUpper(tNameArray(i))
                    isBreak = False
                End If
            Next
            tName = tName2
        End If

        If tName.Contains("Mac") Then
            Dim tNameArray() As Char = tName.ToCharArray
            Dim tName2 As String = ""
            Dim isBreak As Boolean = False
            For i = 0 To UBound(tNameArray)
                If i > 2 Then
                    If tNameArray(i) = "c" Then
                        If tNameArray(i - 2) = "a" Then
                            If tNameArray(i - 3) = "M" Then
                                isBreak = True
                            End If
                        End If
                    End If
                End If
                If Not isBreak Then
                    tName2 += tNameArray(i)
                Else
                    tName2 += Char.ToUpper(tNameArray(i))
                    isBreak = False
                End If
            Next
            tName = tName2
        End If
        Return tName
    End Function
    Public Function isForeclosure(ByVal instNo As String) As Boolean
        If Not System.IO.File.Exists("c:\inetpub\wwwroot\scanneddocs\" + Trim(Str(instNo)) + ".tif.txt") Then Return False
        Dim fStream As New System.IO.StreamReader("c:\inetpub\wwwroot\scanneddocs\" + Trim(Str(instNo)) + ".tif.txt")
        Dim line As String
        While Not fStream.EndOfStream
            line = fStream.ReadLine
            If line.ToUpper.Contains("FORECLOSURE DEED") Then
                fStream.Close()
                Return True
            End If
        End While
        fStream.Close()
        Return False
    End Function

    Public Function GetDeedAddress(ByVal instNo As String, Optional ByVal isForeclosure As Boolean = False) As String
        If Not System.IO.File.Exists("c:\inetpub\wwwroot\scanneddocs\" + Trim(Str(instNo)) + ".tif.txt") Then Return False
        Dim fStream As New System.IO.StreamReader("c:\inetpub\wwwroot\scanneddocs\" + Trim(Str(instNo)) + ".tif.txt")
        Dim allText As String
        Dim allWords As New ArrayList
        Dim allTextArray() As String

        Dim i As Integer
        Dim j As Integer

        Dim retStr As String = ""
        Dim startIndex As Integer
        Try
            allText = fStream.ReadToEnd
            fStream.Close()
            allText = Regex.Replace(allText, "\n", " ")
            allText = Regex.Replace(allText, "\r", " ")
            If allText.ToUpper.Contains("TO-WIT:") Then
                allText = Regex.Replace(allText, "TO-WIT:", " xxx1xxx ", RegexOptions.IgnoreCase)
            ElseIf allText.ToUpper.Contains("VIZ:") Then
                allText = Regex.Replace(allText, "VIZ:", " xxx1xxx ", RegexOptions.IgnoreCase)
            ElseIf allText.ToUpper.Contains("WIT:") Then
                allText = Regex.Replace(allText, "WIT:", " xxx1xxx ", RegexOptions.IgnoreCase)
            ElseIf allText.ToUpper.Contains("OF THE RECORDS IN THE OFFICE OF THE JUDGE OF PROBATE OF MOBILE COUNTY,  ALABAMA:") Then
                allText = Regex.Replace(allText, "OF THE RECORDS IN THE OFFICE OF THE JUDGE OF PROBATE OF MOBILE COUNTY,  ALABAMA:", " xxx1xxx ", RegexOptions.IgnoreCase)
            ElseIf allText.ToUpper.Contains("DESCRIBED AS FOLLOWS:") Then
                allText = Regex.Replace(allText, "DESCRIBED AS FOLLOWS:", " xxx1xxx ", RegexOptions.IgnoreCase)
            ElseIf allText.ToUpper.Contains("TO-WIT") Then
                allText = Regex.Replace(allText, "TO-WIT", " xxx1xxx ", RegexOptions.IgnoreCase)
            End If
            If allText.ToUpper.Contains("ACCORDING TO PLAT") Then
                allText = Regex.Replace(allText, "ACCORDING TO THE PLAT", " xxx2xxx ", RegexOptions.IgnoreCase)
            ElseIf allText.ToUpper.Contains("OF THE RECORDS") Then
                allText = Regex.Replace(allText, "OF THE RECORDS", " xxx2xxx ", RegexOptions.IgnoreCase)
            ElseIf allText.ToUpper.Contains("AS RECORDED") Then
                allText = Regex.Replace(allText, "AS RECORDED", " xxx2xxx ", RegexOptions.IgnoreCase)
            ElseIf allText.ToUpper.Contains("AS PER PLAT") Then
                allText = Regex.Replace(allText, "AS PER PLAT", " xxx2xxx ", RegexOptions.IgnoreCase)
            ElseIf allText.ToUpper.Contains("TO HAVE AND TO HOLD") Then
                allText = Regex.Replace(allText, "TO HAVE AND TO HOLD", " xxx2xxx ", RegexOptions.IgnoreCase)
            End If


            allTextArray = allText.Split(" ")
            allWords.AddRange(allTextArray)
            If isForeclosure Then
                startIndex = allWords.LastIndexOf("xxx1xxx")
            Else
                startIndex = allWords.IndexOf("xxx1xxx")

            End If
            j = startIndex

            For i = j - 1 To allWords.Count - 1
                If allWords(i) = "xxx1xxx" Then
                    While allWords(j) <> "xxx2xxx"
                        retStr += allWords(j) + " "
                        j = j + 1
                    End While
                End If
            Next
            retStr = Regex.Replace(retStr, "xxx1xxx", "")
            retStr = Trim(retStr)
            If retStr.EndsWith(",") Then
                retStr = retStr.Substring(0, retStr.Length - 1)
            End If
            retStr = ConvertToTitle(retStr)
            Return FixSingleQuote(retStr)
        Catch
            Return ""
        End Try
    End Function


    Public Function GetMortAmount(ByVal instNo As String) As String
        Dim fStream As System.IO.StreamReader

        Try

            fStream = New System.IO.StreamReader("c:\inetpub\wwwroot\scanneddocs\" + Trim(Str(instNo)) + ".tif.txt")
            Dim amtStr As String = ""
            Dim i As Integer
            Dim gotMoney As Boolean = False
            Dim testAmt As Decimal = 0.0
            Dim tAmts As New ArrayList
            Dim words() As String


            Dim line As String
            While Not fStream.EndOfStream
                line = fStream.ReadLine
                If line.Contains("$") Then
                    words = line.Split(" ")
                    For i = 0 To UBound(words)
                        If IsNumeric(words(i)) Or words(i).Contains("$") Then
                            tAmts.Add(words(i))
                        End If
                    Next
                End If

            End While

            Return GetLargestAmt(tAmts)
        Catch ex As Exception

        End Try
        Return "0.00"
    End Function

    Public Function GetAMDesc(ByVal instNo As String) As String
        'Dim fStream As New System.IO.StreamReader("c:\inetpub\wwwroot\scanneddocs\" + Trim(Str(instNo)) + ".tif.txt")
        'Dim tDes As String = "-"
        'Dim line As String = fStream.ReadLine
        'While Not fStream.EndOfStream
        '    line = fStream.ReadLine
        '    If line.ToUpper.Contains("CHANGED") Or line.ToUpper.Contains("CORRECTED") Or line.ToUpper.Contains("REMOVED") Or line.ToUpper.Contains("AMENDED") Or line.ToUpper.Contains("ADDITIONS") Then
        '        If line.ToUpper.Contains("ARTICLE") Then
        '            line = fStream.ReadLine
        '        End If
        '        While Not line.ToUpper.Contains("ARTICLE")
        '            tDes += line
        '            line = fStream.ReadLine
        '            If line = Nothing Then
        '                Exit While
        '            End If
        '        End While
        '    End If
        'End While
        'Return FixSingleQuote(tDes)
    End Function

    Public Function GetLeaseDesc(ByVal instNo As String, ByVal Grantee As String, ByVal Grantor As String) As String
        Dim term As String
        Dim type As String
        Dim location As String = ""
        Dim tDesc As String = ""
        Dim sectString As String = ""

        Dim cArray() As Char
        Try
            Dim fStream As New System.IO.StreamReader("c:\inetpub\wwwroot\scanneddocs\" + Trim(Str(instNo)) + ".tif.txt")
            Dim line As String = fStream.ReadLine
            Dim i As Integer
            Dim j As Integer

            While Not fStream.EndOfStream
                line = fStream.ReadLine
                If line.ToUpper.Contains("OIL, GAS AND MINERAL LEASE") Then
                    type = "OIL & GAS LEASE-" + Grantor + "(Lessor) and " + Grantee + "(Lessee)-"
                End If
                If line.ToUpper.Contains("FOR A TERM OF") Then
                    cArray = line.ToCharArray
                    For i = 0 To UBound(cArray)
                        If cArray(i) = "(" Then
                            j = i + 1
                            While cArray(j) <> ")"
                                term = cArray(j)
                                j = j + 1
                                If cArray(j) = ")" Then
                                    Exit For
                                End If
                            End While

                        End If
                    Next
                End If
                If line.ToUpper.Contains("TOWNSHIP") Then
                    Dim sectList As New ArrayList
                    location = line + " "
                    line = fStream.ReadLine
                    While line.ToUpper.Contains("SECTION")
                        cArray = line.ToCharArray
                        For i = 0 To UBound(cArray)
                            If Char.IsDigit(cArray(i)) Then
                                sectString += cArray(i)
                            ElseIf cArray(i) = ":" Then
                                sectList.Add(sectString)
                                sectString = ""
                                Exit For
                            End If
                        Next
                        line = fStream.ReadLine
                    End While
                    location += "Sections: " + sectList(0) + "-" + sectList(sectList.Count - 1)
                End If

            End While
        Catch ex As Exception

        End Try
        Return type + "for a term of " + term + " years " + location
    End Function

    Public Function GetBCTaxRelPrevInst(ByVal instNo As String) As String
        Try

            Dim fStream As New System.IO.StreamReader("c:\inetpub\wwwroot\scanneddocs\" + Trim(Str(instNo)) + ".tif.txt")
            Dim line As String = fStream.ReadLine
            Dim StrArray() As String
            Dim tDesc As String = ""

            While Not fStream.EndOfStream
                line = fStream.ReadLine
                If line.Contains("Baldwin County Probate Office Instrument Number") Then
                    StrArray = line.Split(" ")
                    For i = 0 To UBound(StrArray)
                        If IsNumeric(StrArray(i)) Then
                            Return "Inst# " + Trim(StrArray(i))
                        End If
                    Next
                    line = fStream.ReadLine
                    StrArray = line.Split(" ")
                    For i = 0 To UBound(StrArray)
                        If IsNumeric(StrArray(i)) Then
                            Return "Inst# " + Trim(StrArray(i))
                        End If
                    Next
                End If
            End While
        Catch ex As Exception

        End Try
        Return "Unreadable"
    End Function
    Public Function GetALTaxRelPrevInst(ByVal instNo As String) As String
        Try

            Dim fStream As New System.IO.StreamReader("c:\inetpub\wwwroot\scanneddocs\" + Trim(Str(instNo)) + ".tif.txt")
            Dim line As String = fStream.ReadLine
            Dim StrArray() As String
            Dim tDesc As String = ""

            While Not fStream.EndOfStream
                line = fStream.ReadLine
                If line.Contains("Baldwin County Probate Office") Then
                    StrArray = line.Split(" ")
                    For i = 0 To UBound(StrArray)
                        If IsNumeric(StrArray(i)) Then
                            Return "Inst# " + Trim(StrArray(i))
                        End If
                    Next
                    line = fStream.ReadLine
                    StrArray = line.Split(" ")
                    For i = 0 To UBound(StrArray)
                        If IsNumeric(StrArray(i)) Then
                            Return "Inst# " + Trim(StrArray(i))
                        End If
                    Next
                End If
            End While
        Catch ex As Exception

        End Try
        Return "Unreadable"
    End Function

    Public Function GetUSRelPrevInst(ByVal instNo As String) As String
        Try

            Dim fStream As New System.IO.StreamReader("c:\inetpub\wwwroot\scanneddocs\" + Trim(Str(instNo)) + ".tif.txt")
            Dim line As String = fStream.ReadLine
            Dim StrArray() As String


            While Not fStream.EndOfStream
                line = fStream.ReadLine
                If line.Contains("UCC No") Then
                    line = fStream.ReadLine
                    StrArray = line.Split(" ")
                    For i = 0 To UBound(StrArray)
                        If IsNumeric(StrArray(i)) Then
                            Return "UCC# " + Trim(StrArray(i))
                        End If
                    Next
                    StrArray = line.Split(" ")
                ElseIf line.Contains("at #") Then
                    StrArray = line.Split(" ")
                    For i = 0 To UBound(StrArray)
                        StrArray(i) = StrArray(i).Replace("#", "")
                        StrArray(i) = StrArray(i).Replace(",", "")
                        If IsNumeric(StrArray(i)) Then
                            If CInt(StrArray(i)) > 2050 Then
                                Return "UCC# " + Trim(StrArray(i))
                            End If
                        End If
                    Next
                    StrArray = line.Split(" ")
                End If
            End While

        Catch ex As Exception

        End Try
        Return "Unreadable"
    End Function
    Public Function GetBCJudgePrevInst(ByVal instNo As String) As String
        Try

            Dim fStream As New System.IO.StreamReader("c:\inetpub\wwwroot\scanneddocs\" + Trim(Str(instNo)) + ".tif.txt")
            Dim line As String = fStream.ReadLine
            Dim strArray() As String
            Dim tDesc As String = ""

            While Not fStream.EndOfStream

                line = fStream.ReadLine
                If line.ToUpper.Contains("INSTRUMENT") Then
                    strArray = line.Split(" ")
                    For i = 0 To UBound(strArray)
                        If strArray(i) = "INSTRUMENT" Then
                            Return strArray(i + 1).Replace(",", "")
                        End If
                    Next
                End If
                line = fStream.ReadLine
                If line.ToUpper.Contains("INSTRUMENT NO.") Then
                    strArray = line.Split(" ")
                    For i = 0 To UBound(strArray)
                        If strArray(i) = "INSTRUMENT" Then
                            Return strArray(i + 2).Replace(",", "")
                        End If
                    Next
                End If
                If line.ToUpper.Contains("INSTRUMENT NUMBER") Then
                    strArray = line.Split(" ")
                    For i = 0 To UBound(strArray)
                        If strArray(i) = "NUMBER" Then
                            Return strArray(i + 1).Replace(",", "")
                        End If
                    Next
                End If
                If line.ToUpper.Contains("NUMBER #") Then
                    strArray = line.Split(" ")
                    For i = 0 To UBound(strArray)
                        If strArray(i) = "#" Then
                            Return strArray(i + 1).Replace(",", "")
                        End If
                    Next
                End If
                If line.Contains("inst #") Then
                    strArray = line.Split(" ")
                    For i = 0 To UBound(strArray)
                        If strArray(i) = "#" Then
                            Return strArray(i + 1).Replace(",", "")
                        End If
                    Next
                End If
                If line.Contains("inst no") Then
                    strArray = line.Split(" ")
                    For i = 0 To UBound(strArray)
                        If strArray(i) = "inst" Then
                            Return strArray(i + 2).Replace(",", "")
                        End If
                    Next
                End If
                If line.Contains("JUDGMENT BOOK") Then
                    strArray = line.Split(" ")
                    For i = 0 To UBound(strArray)
                        If strArray(i) = "JUDGMENT" Then
                            Return strArray(i + 2).Replace(",", "")
                        End If
                    Next
                End If

                If line.Contains("BOOK ") Then
                    strArray = line.Split(" ")
                    For i = 0 To UBound(strArray)
                        If strArray(i) = "BOOK" Then
                            Return strArray(i + 1).Replace(",", "")
                        End If
                    Next
                End If

                If line.Contains("#") Then
                    strArray = line.Split(" ")
                    For i = 0 To UBound(strArray)
                        If IsNumeric(strArray(i).Replace("#", "")) Then
                            Return strArray(i).Replace("#", "")
                        End If
                    Next
                End If
                If line.ToUpper.Contains("BOOK/PAGE NUMBER") Then
                    strArray = line.Split(" ")
                    For i = 0 To UBound(strArray)
                        If IsNumeric(strArray(i)) Then
                            Return strArray(i)
                        End If
                    Next
                End If

            End While

        Catch ex As Exception

        End Try
        Return "Unreadable"
    End Function

    Public Function GetFODesc(ByVal instNo As String, ByVal grantor As String, ByVal grantee As String) As String
        Dim tDes As String = " - "
        Dim auctioneer As String = ""
        Dim gotAuctioneer As Boolean = False
        Dim prop As String = ""

        Dim fStream As New System.IO.StreamReader("c:\inetpub\wwwroot\scanneddocs\" + Trim(Str(instNo)) + ".tif.txt")
        Dim line As String = fStream.ReadLine
        While Not fStream.EndOfStream
            line = fStream.ReadLine
            If Not gotAuctioneer Then
                If line.Contains("as auctioneer") Or line.Contains("as Auctioneer") Or line.Contains("the Auctioneer") Or line.Contains("the auctioneer") Then
                    Dim matches As MatchCollection
                    Dim i As Integer
                    matches = Regex.Matches(line, "(?<FirstName>[A-Z]\.?\w*\-?[A-Z]?\w*)\s?(?<MiddleName>[A-Z]\w+|[A-Z]?\.?)\s(?<LastName>[A-Z]?\w{0,3}[A-Z]\w+\-?[A-Z]?\w*)(?:,\s|)(?<Suffix>Jr\.|Sr\.|IV|III|II|)")
                    'matches = Regex.Matches(line, "(?<FirstName>^[A-Z]+[a-zA-Z]*$\.?\w*\-?^[A-Z]+[a-zA-Z]*$?\w*)\s?(?<MiddleName>[A-Z]\w+|[A-Z]?\.?)\s(?<LastName>[A-Z]?\w{0,3}[A-Z]\w+\-?[A-Z]?\w*)(?:,\s|)(?<Suffix>Jr\.|Sr\.|IV|III|II|)")
                    If matches.Count > 0 Then
                        For i = 0 To matches.Count - 1
                            If Not isExcluded(matches(i).Value.ToUpper) Then
                                auctioneer = matches(i).Value
                                Exit For
                            End If
                        Next
                        auctioneer += " as auctioneer,"
                        gotAuctioneer = True
                    End If
                End If
            End If
        End While
        fStream.Close()
        fStream = Nothing

        fStream = New System.IO.StreamReader("c:\inetpub\wwwroot\scanneddocs\" + Trim(Str(instNo)) + ".tif.txt")
        line = fStream.ReadLine
        While Not fStream.EndOfStream
            line = fStream.ReadLine
            If Not gotAuctioneer Then
                If line.Contains("as auctioneer") Or line.Contains("as Auctioneer") Or line.Contains("the Auctioneer") Or line.Contains("the auctioneer") Then
                    Dim matches As MatchCollection
                    Dim i As Integer
                    matches = Regex.Matches(line, "(?<FirstName>[A-Z]\.?\w*\-?[A-Z]?\w*)\s?(?<MiddleName>[A-Z]\w+|[A-Z]?\.?)\s(?<LastName>[A-Z]?\w{0,3}[A-Z]\w+\-?[A-Z]?\w*)(?:,\s|)(?<Suffix>Jr\.|Sr\.|IV|III|II|)")
                    'matches = Regex.Matches(line, "(?<FirstName>^[A-Z]+[a-zA-Z]*$\.?\w*\-?^[A-Z]+[a-zA-Z]*$?\w*)\s?(?<MiddleName>[A-Z]\w+|[A-Z]?\.?)\s(?<LastName>[A-Z]?\w{0,3}[A-Z]\w+\-?[A-Z]?\w*)(?:,\s|)(?<Suffix>Jr\.|Sr\.|IV|III|II|)")
                    If matches.Count > 0 Then
                        For i = 0 To matches.Count - 1
                            If Not isExcluded(matches(i).Value.ToUpper) Then
                                auctioneer = matches(i).Value
                                Exit For
                            End If
                        Next
                        auctioneer += " as auctioneer,"
                        gotAuctioneer = True
                    End If
                End If
            End If
        End While
        fStream.Close()

        Return auctioneer + " to " + grantee
    End Function

    Public Function GetDIDesc(ByVal instNo As String) As String
        Dim fStream As New System.IO.StreamReader("c:\inetpub\wwwroot\scanneddocs\" + Trim(Str(instNo)) + ".tif.txt")
        Dim tDes As String = " - "
        Dim line As String = fStream.ReadLine
        While Not fStream.EndOfStream
            line = fStream.ReadLine
            If line.ToUpper.Contains("PRESIDENT") Then
                tDes += line
                Return FixSingleQuote(tDes)
            End If
            If line.ToUpper.Contains("OWNER") Then
                tDes += line
                Return FixSingleQuote(tDes)
            End If
            If line.ToUpper.Contains("MEMBER") Then
                tDes += line
                Return FixSingleQuote(tDes)
            End If
            If line.Contains("______________________") Then
                line = fStream.ReadLine
                tDes += line
                Return FixSingleQuote(tDes)
            End If
        End While
        Return "Unreadable"
    End Function

    Public Function GetAODesc(ByVal instNo As String) As String
        Try

            Dim fStream As New System.IO.StreamReader("c:\inetpub\wwwroot\scanneddocs\" + Trim(Str(instNo)) + ".tif.txt")
            Dim tDes As String = ""
            Dim tAgent As String = ""
            Dim allText As String = fStream.ReadToEnd
            Dim allWordsArray() As String
            Dim allWords As New ArrayList
            Dim gotPurpose As Boolean = False
            Dim gotAgent As Boolean = False
            Dim i As Integer
            Dim j As Integer

            allText = Regex.Replace(allText, "/n", " ")
            allText = Regex.Replace(allText, "/r", " ")
            allText = Regex.Replace(allText, Chr(13), " ")
            allText = Regex.Replace(allText, Chr(10), " ")

            allWordsArray = allText.Split(" ")
            allWords.AddRange(allWordsArray)

            For i = 0 To allWords.Count - 1
                If gotPurpose And gotAgent Then Exit For
                If Not gotPurpose Then
                    If Regex.Match(allWords(i), "purpose", RegexOptions.IgnoreCase).Success = True Then
                        For j = i + 1 To allWords.Count - 1
                            If Regex.Match(allWords(j), "\([aA1]\)|:").Success Then
                                If Regex.Match(allWords(j), ":").Success Then
                                    j += 1
                                End If
                                While Not Regex.Match(allWords(j), "\([bB2]\)").Success
                                    tDes += allWords(j) + " "
                                    j += 1
                                    If Regex.Match(allWords(j), "\([bB2]\)").Success Then
                                        gotPurpose = True
                                        Exit While
                                    End If
                                    If j > i + 50 Then
                                        gotPurpose = True
                                        Exit While
                                    End If
                                End While
                                tDes = Regex.Replace(tDes, "\([aA1]\)", " - ")
                                Exit For
                            End If
                        Next
                    End If
                End If
                If gotPurpose And Not gotAgent Then
                    If Regex.Match(allWords(i), "agent", RegexOptions.IgnoreCase).Success = True Then
                        For j = i + 1 To allWords.Count - 1
                            tAgent += allWords(j) + " "
                            If j > i + 20 Then
                                gotAgent = True
                                Exit For
                            End If
                        Next
                    End If
                End If
            Next

            tDes = "Purpose is to transact any & all lawful business " + tDes
            tDes = tDes + "-" + tAgent
            Return FixSingleQuote(tDes)
        Catch ex As Exception
            Return "unreadable"
        End Try

    End Function
    Public Function GetAmount2(ByVal instNo As String, Optional ByVal GetLargest As Boolean = False, Optional ByVal AddAmts As Boolean = False) As String
        Dim fStream As New System.IO.StreamReader("c:\inetpub\wwwroot\scanneddocs\" + Trim(Str(instNo)) + ".tif.txt")
        Dim lineChars() As Char
        Dim amtStr As String
        Dim i As Integer
        Dim gotMoney As Boolean = False
        Dim amtDex As Integer
        Dim tAmts As New ArrayList
        Dim line As String = fStream.ReadLine
        While Not fStream.EndOfStream
            line = fStream.ReadLine
            If line.Contains("$") Then
                line = Regex.Replace(line, "\D ", "|")
                If line.Contains("U S") Or line.Contains("U.S.") Or line.Contains("U. S.") Then
                    line.Replace(" ", "")
                End If
                lineChars = line.ToCharArray()
                For i = 0 To UBound(lineChars)
                    If lineChars(i) = "$" Then
                        gotMoney = True
                        amtDex = i
                    ElseIf lineChars(i) = " " And gotMoney And amtDex <> i - 1 Then
                        If i + 1 < UBound(lineChars) Then
                            If Not Char.IsDigit(lineChars(i + 1)) Then
                                gotMoney = False
                                tAmts.Add(amtStr)
                                amtStr = ""
                            End If
                        End If

                    ElseIf lineChars(i) = ")" And gotMoney And amtDex <> i - 1 Then
                        gotMoney = False
                        tAmts.Add(amtStr)
                        amtStr = ""
                    End If
                    If gotMoney Then
                        amtStr += lineChars(i)
                    End If
                Next
                If gotMoney Then
                    gotMoney = False
                    tAmts.Add(amtStr)
                    amtStr = ""
                End If
            End If
        End While
        If GetLargest Then
            Return GetLargestAmt(tAmts)
        End If
        If AddAmts Then
            Return AddAmounts(tAmts)
        End If
        Try
            Dim tDec As Decimal
            tDec = CDec(tAmts(0))
            Return FormatCurrency(tDec)
        Catch ex As Exception

        End Try
        Return "0.00"
    End Function
    Public Function GetAmount(ByVal instNo As String, Optional ByVal GetLargest As Boolean = False, Optional ByVal AddAmts As Boolean = False, Optional ByVal isTaxDoc As Boolean = False, Optional ByVal isRegions As Boolean = False) As String
        Dim fStream As System.IO.StreamReader
        If Not System.IO.File.Exists("c:\inetpub\wwwroot\scanneddocs\" + Trim(Str(instNo)) + ".tif.txt") Then
            Return "0.0"
        End If
        fStream = New System.IO.StreamReader("c:\inetpub\wwwroot\scanneddocs\" + Trim(Str(instNo)) + ".tif.txt")
        Dim amtStr As String = ""
        Dim i As Integer
        Dim gotMoney As Boolean = False
        Dim testAmt As Decimal = 0.0
        Dim tAmts As New ArrayList
        Dim line As String = fStream.ReadToEnd
        line = Regex.Replace(line, "\$ ", "!~!")
        line = Regex.Replace(line, "\$", "!~!")
        line = Regex.Replace(line, ",", "")
        line = Regex.Replace(line, " ", "")
        line = Regex.Replace(line, "\)", " ")
        Dim matches As MatchCollection
        matches = Regex.Matches(line, "!~!\d*,*\.*\d{2}")
        For i = 0 To matches.Count - 1
            tAmts.Add(Regex.Replace(matches(i).Value, "!~!", ""))
        Next
        If isRegions Then
            Return GetLargestAmt(tAmts, True)
        Else
            Return GetLargestAmt(tAmts)
        End If
    End Function

    Private Function AddAmounts(ByVal amtList As ArrayList) As String
        Dim i As Integer
        Dim decStr As String
        Dim amtDec As Decimal = 0.0
        Dim totalAmt As Decimal = 0.0
        For i = 0 To amtList.Count - 1
            decStr = amtList(i).ToString.Replace("(", "")
            decStr = decStr.Replace(")", "")
            decStr = decStr.Replace("$", "")
            decStr = decStr.Replace(" ", "")
            decStr = decStr.Replace(",", "")
            If decStr.EndsWith(".") Then
                decStr = decStr.Substring(0, decStr.Length - 1)
            End If
            Try
                amtDec = CDec(decStr)
                totalAmt += amtDec
            Catch ex As Exception

            End Try
        Next
        Return FormatCurrency(totalAmt)
    End Function

    Public Function GetKindTax(ByVal instNo As String, ByVal DocType As String) As String
        If DocType = "US" Then
            Return GetUSKindTax(instNo)
        ElseIf DocType = "ST" Then
            Return GetALKindTax(instNo)
        ElseIf DocType = "BT" Then
            Return GetBCKindTax(instNo)
        End If
        Return "unknown tax lien type"

    End Function
    Public Function GetUSKindTax(ByVal instNo As String)
        Try

            Dim fStream As New System.IO.StreamReader("c:\inetpub\wwwroot\scanneddocs\" + Trim(Str(instNo)) + ".tif.txt")
            Dim tDes As String = ""
            Dim line As String = fStream.ReadLine
            While Not fStream.EndOfStream
                line = fStream.ReadLine
                If line.ToUpper.Contains("1040") Then
                    fStream.Close()
                    Return "1040"
                ElseIf line.ToUpper.Contains("941") Then
                    fStream.Close()
                    Return "941"
                ElseIf line.ToUpper.Contains("6672") Then
                    fStream.Close()
                    Return "6672"
                End If
            End While
            fStream.Close()
            Return ""
        Catch ex As Exception

        End Try
        Return ""
    End Function
    Public Function GetALKindTax(ByVal instNo As String)
        Dim tDes As String = ""
        Try
            Dim fStream As System.IO.StreamReader
            Try
                If System.IO.File.Exists("c:\inetpub\wwwroot\scanneddocs\" + Trim(Str(instNo)) + "crp.tif.txt") Then
                    fStream = New System.IO.StreamReader("c:\inetpub\wwwroot\scanneddocs\" + Trim(Str(instNo)) + "crp.tif.txt")
                Else
                    fStream = New System.IO.StreamReader("c:\inetpub\wwwroot\scanneddocs\" + Trim(Str(instNo)) + ".tif.txt")
                End If

            Catch ex As Exception
                Return "Unknown"
            End Try
            Dim line As String = fStream.ReadLine
            While Not fStream.EndOfStream
                line = fStream.ReadLine
                If line.ToUpper.Contains("TYPE OF TAX") Or line.ToUpper.Contains("KIND OF TAX") Then
                    'line = fStream.ReadLine
                    tDes = ConvertToTitle(line)
                    Exit While
                End If
            End While
            fStream.Close()
            If tDes.ToUpper.Contains("PENALTY") Or tDes.ToUpper.Contains("ASSESSMENT") Or tDes.ToUpper.Contains("TRUST FUND") Then
                tDes = "Penalty Assessment"
            End If
            tDes = tDes.Replace("Type Of Tax:", "")
            tDes = tDes.Replace("Type Of Tax", "")
            tDes = tDes.Replace(".", "")
            tDes = tDes.Replace("Thdividual", "Indiv.")
            tDes = tDes.Replace("Thdividual", "Indiv.")
            tDes = tDes.Replace("Hndividual", "Indiv.")
        Catch ex As Exception

        End Try

        Return tDes
    End Function
    Public Function GetBCKindTax(ByVal instNo As String) As String
        Try

            Dim fStream As New System.IO.StreamReader("c:\inetpub\wwwroot\scanneddocs\" + Trim(Str(instNo)) + ".tif.txt")
            Dim tDes As String = ""
            Dim line As String = fStream.ReadLine
            While Not fStream.EndOfStream
                line = fStream.ReadLine
                If line.ToUpper.Contains("BALDWIN COUNTY REVENUE COMMISSION") Then
                    Return "Pers Prop"
                Else

                    If line.ToUpper.Contains("RENT") Or line.ToUpper.Contains("SALE") Then
                        Select Case line.ToUpper
                            Case "RENT"
                                tDes = "Rental Tax"
                                Exit While
                            Case "SALE"
                                tDes = "Sales Tax"
                                Exit While
                        End Select
                    End If
                End If
            End While

            fStream.Close()
            Return tDes
        Catch ex As Exception
            Return ""
        End Try
    End Function

    Public Function GetMTKindTax(ByVal instNo As String) As String
        Try

            Dim fStream As System.IO.StreamReader
            fStream = New System.IO.StreamReader("c:\inetpub\wwwroot\scanneddocs\" + Trim(Str(instNo)) + ".tif.txt")
            Dim line As String
            Dim lineStrs() As String


            While Not fStream.EndOfStream
                line = fStream.ReadLine
                If line.Contains("KIND OF TAX:") Then
                    lineStrs = line.Split(":")
                    Return lineStrs(1)
                End If
            End While
            Return "ureadable"
        Catch ex As Exception
            Return ""
        End Try
    End Function


    Public Function GetTaxAddress(ByVal instNo As String, ByVal DocType As String) As String
        Try

            If DocType = "US" Then
                Return GetUSTaxAddress(instNo)
            ElseIf DocType = "ST" Then
                Return GetALTaxAddress(instNo)
            ElseIf DocType = "BT" Then
                Return GetBCTaxAddress(instNo)
            End If
        Catch ex As Exception
            Return "unknown tax lien type"
        End Try
        Return "unknown tax lien type"
    End Function
    Public Function GetTaxAddressPP(ByVal instNo As String, ByVal DocType As String) As String
        If DocType = "US" Then
            Return GetUSTaxAddress(instNo)
        ElseIf DocType = "ST" Then
            Return GetALTaxAddress(instNo)
        ElseIf DocType = "BT" Then
            Return GetBCTaxAddressPP(instNo)
        End If
        Return "unknown tax lien type"

    End Function
    Public Function GetTaxAmount(ByVal instNo As String, ByVal DocType As String) As String
        If DocType = "US" Then
            Return GetUSTaxAmount(instNo)
        ElseIf DocType = "ST" Then
            Return GetALTaxAmount(instNo)
        ElseIf DocType = "BT" Then
            Return GetBCTaxAmount(instNo)
        End If
        Return "unknown tax lien type"

    End Function
    Public Function CalcDeedTax(ByVal downPay As String) As String
        Dim tTax As Double
        Dim pennies As Double
        Dim diff As Double
        If downPay.Contains("/") Then Return "0.00"
        If downPay <> "0" Then
            tTax = ((downPay * 100) / 50000) * 0.5
            pennies = tTax - Math.Floor(tTax)
            diff = 0.5 - pennies
            tTax += diff
        ElseIf downPay = "0" Then
            tTax = 0.5
        End If
        Return Str(tTax)
    End Function
    Public Function GetALTaxAddress(ByVal instNo As String)
        Dim fStream As System.IO.StreamReader

        If System.IO.File.Exists("c:\inetpub\wwwroot\scanneddocs\" + Trim(Str(instNo)) + "crp.tif.txt") Then
            fStream = New System.IO.StreamReader("c:\inetpub\wwwroot\scanneddocs\" + Trim(Str(instNo)) + "crp.tif.txt")
        Else
            fStream = New System.IO.StreamReader("c:\inetpub\wwwroot\scanneddocs\" + Trim(Str(instNo)) + ".tif.txt")
        End If
        Dim tDes As String = ""
        Dim line As String

        Dim i As Integer

        Dim addrArray As New ArrayList
        Dim lineArray As New ArrayList
        Try

            While Not fStream.EndOfStream
                line = fStream.ReadLine
                line = line.ToUpper
                If line.ToUpper.Contains("ALABAMA") Then
                    line = fStream.ReadLine
                    If line.ToUpper.Contains("STATE") Then
                        line = fStream.ReadLine()
                    End If
                    If line.Contains("(w") Then
                        line = fStream.ReadLine()
                    End If
                    addrArray.Add(line)
                    line = fStream.ReadLine
                    addrArray.Add(line)
                    Exit While
                ElseIf line.ToUpper.Contains("CERTIFICATE") Then
                    Do
                        line = fStream.ReadLine
                        lineArray.Add(line)
                    Loop Until line.Contains("SSN")

                    addrArray.Add(lineArray(lineArray.Count - 3))
                    addrArray.Add(lineArray(lineArray.Count - 2))
                    Exit While
                End If
            End While

            For i = addrArray.Count - 2 To addrArray.Count - 1
                tDes += addrArray(i) + " "
            Next
            tDes = Regex.Replace(tDes, "[,|.|\s+]AL\s+[\s\d-]*", "")
            If tDes.EndsWith(",") Then
                tDes = tDes.Substring(0, tDes.Length - 1)
            End If
            If tDes.EndsWith(".") Then
                tDes = tDes.Substring(0, tDes.Length - 1)
            End If
            tDes = ConvertToTitle(tDes)

            fStream.Close()
        Catch ex As Exception
            tDes = "Unreadable"
            fStream.Close()
        End Try

        tDes = Regex.Replace(tDes, "(\d{5}-\d{4})|(\d{5})^(\d{5}-\d{3})", "")
        Return FixSingleQuote(tDes)
    End Function
    Public Function GetALTaxAmount(ByVal instNo As String) As String
        Dim fStream As System.IO.StreamReader
        Dim line As String
        Dim tWords() As String

        Try
            If System.IO.File.Exists("c:\inetpub\wwwroot\scanneddocs\" + Trim(Str(instNo)) + "crp.tif.txt") Then
                fStream = New System.IO.StreamReader("c:\inetpub\wwwroot\scanneddocs\" + Trim(Str(instNo)) + "crp.tif.txt")
            Else
                fStream = New System.IO.StreamReader("c:\inetpub\wwwroot\scanneddocs\" + Trim(Str(instNo)) + ".tif.txt")
            End If

        Catch ex As Exception
            Return "0.0"
        End Try
        While Not fStream.EndOfStream
            line = fStream.ReadLine
            If line.ToUpper.Contains("AMOUNT OF LIEN") Then
                tWords = line.Split(":")
                Return tWords(UBound(tWords)).Replace("S", "$")
            End If
        End While
    End Function

    Public Function GetUSTaxAmount(ByVal instNo As String) As String
        Dim amtStr As String = "0.00"
        Try
            Dim fStream As System.IO.StreamReader
            fStream = New System.IO.StreamReader("c:\inetpub\wwwroot\scanneddocs\" + Trim(Str(instNo)) + ".tif.txt")
            Dim testAmt As Decimal = 0.0
            Dim line As String = fStream.ReadLine

            While Not fStream.EndOfStream
                line = fStream.ReadLine
                If line.ToUpper.Contains("MOBILE COUNTY TOTAL") Then
                    line = fStream.ReadLine
                    line = fStream.ReadLine
                    amtStr = Regex.Replace(line, "\D*", "")
                    amtStr = Regex.Replace(amtStr, ":", "")
                    If Not amtStr.Contains("$") Then
                        If amtStr.ToCharArray()(0) = "5" Then
                            amtStr = amtStr.Substring(1, amtStr.Length - 1)
                        End If
                    End If
                    amtStr = amtStr.Replace("$", "")
                    amtStr = Regex.Replace(amtStr, " ", "")
                    If amtStr.ToCharArray()(0) = "." Then
                        amtStr = amtStr.Substring(1, amtStr.Length - 1)
                    End If

                    If amtStr.EndsWith(".") Then
                        amtStr = amtStr.Substring(0, amtStr.Length - 1)
                    End If

                    If Not amtStr.Contains(".") Then
                        testAmt = CDec(amtStr)
                        testAmt = testAmt / 100
                        amtStr = Trim(Str(testAmt))
                    End If
                    Exit While
                End If
            End While
        Catch ex As Exception

        End Try

        Return amtStr
    End Function
    Public Function GetBCTaxAmount(ByVal instNo As String) As String
        Dim fStream As System.IO.StreamReader
        fStream = New System.IO.StreamReader("c:\inetpub\wwwroot\scanneddocs\" + Trim(Str(instNo)) + ".tif.txt")
        Dim amtStr As String = ""
        Dim i As Integer
        Dim gotMoney As Boolean = False
        Dim testAmt As Decimal = 0.0
        Dim tAmts As New ArrayList
        Dim line As String = fStream.ReadToEnd
        line = Regex.Replace(line, "\$", "!~!")
        Dim matches As MatchCollection
        matches = Regex.Matches(line, "!~!\d*.\d{2}")
        For i = 0 To matches.Count - 1
            tAmts.Add(Regex.Replace(matches(i).Value, "!~!", ""))
        Next
        Return GetLargestAmt(tAmts)
        Return "0.00"
    End Function

    Public Function GetMTTaxAmount(ByVal instNo As String) As String
        Dim fStream As System.IO.StreamReader
        fStream = New System.IO.StreamReader("c:\inetpub\wwwroot\scanneddocs\" + Trim(Str(instNo)) + ".tif.txt")
        Dim line As String
        Dim lineStrs() As String


        While Not fStream.EndOfStream
            line = fStream.ReadLine
            If line.Contains("AMOUNT OF LIEN:") Then
                lineStrs = line.Split(":")
                lineStrs(1).Replace("$", "")
                Return lineStrs(1).Replace("**", "")
            End If
        End While
        Return "ureadable"
    End Function
    Public Function GetHospAmount(ByVal instNo As String, ByVal HospName As String) As String
        Try

            Dim fStream As System.IO.StreamReader
            fStream = New System.IO.StreamReader("c:\inetpub\wwwroot\scanneddocs\" + Trim(Str(instNo)) + ".tif.txt")
            Dim line As String
            Dim lineStrs() As String
            If HospName = "University Of South Alabama Hospitals" Then
                While Not fStream.EndOfStream
                    line = fStream.ReadLine
                    If line.Contains("TOTAL CHARGES") Or line.Contains("DISCHARGE") Then
                        lineStrs = line.Split(":")
                        Return lineStrs(UBound(lineStrs))
                    End If
                End While
            ElseIf HospName = "Mobile Infirmary Medical Center" Then
                While Not fStream.EndOfStream
                    line = fStream.ReadLine
                    If line.Contains("Amount Due") Then
                        lineStrs = line.Split(":")
                        If lineStrs(UBound(lineStrs)).Contains("$") Then
                            Return lineStrs(UBound(lineStrs))
                        End If
                    End If

                    If line.Contains("Amount Due") And line.IndexOf("$") < 0 Then
                        While Not line.Contains("$")
                            line = fStream.ReadLine()
                            If line.Contains("$") Then
                                Return line
                            End If
                        End While
                    End If
                End While
            ElseIf HospName.Contains("Springhill Memorial Hospital") Then
                While Not fStream.EndOfStream
                    line = fStream.ReadLine
                    If line.Contains("claimed to be") Then
                        lineStrs = line.Split(":")
                        If lineStrs(UBound(lineStrs)).Contains("$") Then
                            Return lineStrs(UBound(lineStrs))
                        End If
                    End If

                    If line.Contains("claimed to be") And line.IndexOf("$") < 0 Then
                        While Not line.Contains("$")
                            line = fStream.ReadLine()
                            If line.Contains("$") Then
                                Return line
                            End If
                        End While
                    End If
                End While

            End If
        Catch ex As Exception
            Return "unreadable"
        End Try

        Return "ureadable"
    End Function
    Public Function GetLienAmount(ByVal instNo As String, ByVal GrantName As String) As String
        Dim fStream As System.IO.StreamReader
        fStream = New System.IO.StreamReader("c:\inetpub\wwwroot\scanneddocs\" + Trim(Str(instNo)) + ".tif.txt")
        Dim line As String
        Dim lineStrs() As String
        Select Case GrantName
            Case "Kings Branch Estates Property Owners Association", "Kings Branch Property Owners Association"
                While Not fStream.EndOfStream
                    line = fStream.ReadLine
                    If line.Contains("is the sum of") Then
                        lineStrs = line.Split(" ")
                        For i = 0 To UBound(lineStrs)
                            If lineStrs(i).Contains("$") Then
                                Return lineStrs(i)
                            End If
                        Next
                    End If
                End While
            Case "Augusta Property Owners Association Inc"
                While Not fStream.EndOfStream
                    line = fStream.ReadLine
                    If line.Contains("secure indebtedness") Then
                        lineStrs = line.Split(" ")
                        For i = 0 To UBound(lineStrs)
                            If lineStrs(i).Contains("$") Then
                                lineStrs(i) = lineStrs(i).Replace("(", "")
                                lineStrs(i) = lineStrs(i).Replace(")", "")
                                Return lineStrs(i)
                            End If
                        Next
                        Return lineStrs(UBound(lineStrs))
                    End If
                End While
            Case "Board Of Water And Sewer Commissioner Of The City Of Mobile"
                While Not fStream.EndOfStream
                    line = fStream.ReadLine
                    If line.Contains("connection fee of $") Then
                        lineStrs = line.Split(" ")
                        For i = 0 To UBound(lineStrs)
                            If lineStrs(i).Contains("$") Then
                                lineStrs(i) = lineStrs(i).Replace("(", "")
                                lineStrs(i) = lineStrs(i).Replace(")", "")
                                Return lineStrs(i)
                            End If
                        Next
                        Return lineStrs(UBound(lineStrs))
                    End If
                End While
        End Select


        Return "ureadable"
    End Function

    Public Function GetBCJudgeAmount(ByVal instNo As String) As String
        Dim fStream As System.IO.StreamReader
        fStream = New System.IO.StreamReader("c:\inetpub\wwwroot\scanneddocs\" + Trim(Str(instNo)) + ".tif.txt")
        Dim line As String
        Dim tStrings() As String
        Dim tAmts As New ArrayList


        While Not fStream.EndOfStream
            line = fStream.ReadLine
            If line.Contains("$") Then
                line = line.Replace("SUM OF", "")
                tStrings = line.Split(" ")
                For i = 0 To UBound(tStrings)
                    If tStrings(i).Contains("$") Then
                        tAmts.Add(tStrings(i))
                    End If
                Next
            End If
        End While
        Return GetLargestAmt(tAmts)
        Return "unreadable"
    End Function

    Public Function ReplaceFromTable(ByVal s As String) As String
        Dim searchArr As New ArrayList
        Dim replArr As New ArrayList
        Dim sIndex As Integer

        Dim dr As MySqlDataReader = fmMain.dg.GetDataReader("SELECT sString FROM searchstrings")
        While dr.Read
            searchArr.Add(dr(0).ToString)
        End While
        fmMain.dg.KillReader(dr)
        dr = fmMain.dg.GetDataReader("SELECT rString FROM searchstrings")
        While dr.Read
            replArr.Add(dr(0).ToString)
        End While
        fmMain.dg.KillReader(dr)

        For i = 0 To searchArr.Count - 1
            If s.Contains(searchArr(i)) Or searchArr(i).ToString.Contains(s) Then
                s = s.Replace(searchArr(i), replArr(i))
            End If
        Next

        Return s
    End Function


    Public Function GetBCJudgeCase(ByVal instNo As String) As String
        Dim fStream As System.IO.StreamReader
        fStream = New System.IO.StreamReader("c:\inetpub\wwwroot\scanneddocs\" + Trim(Str(instNo)) + ".tif.txt")
        Dim line As String
        Dim caseNo As String
        Dim startIndex As Integer


        Try

            While Not fStream.EndOfStream
                line = fStream.ReadLine
                If line.ToUpper.Contains("CV 20") Then
                    startIndex = line.IndexOf("CV ")
                    caseNo = line.Substring(startIndex, 17)
                    Return caseNo
                End If

                If line.ToUpper.Contains("DV 20") Then
                    startIndex = line.IndexOf("DV ")
                    caseNo = line.Substring(startIndex, 17)
                    Return caseNo
                End If

                If line.ToUpper.Contains("DR 20") Then
                    startIndex = line.IndexOf("DR ")
                    caseNo = line.Substring(startIndex, 17)
                    Return caseNo
                End If

                If line.ToUpper.Contains("SM 20") Then
                    startIndex = line.IndexOf("SM ")
                    caseNo = line.Substring(startIndex, 17)
                    Return caseNo
                End If

                If line.ToUpper.Contains("SN 20") Then
                    startIndex = line.IndexOf("SN ")
                    line.Replace("SN", "SM")
                    caseNo = line.Substring(startIndex, 17)
                    Return caseNo
                End If

                If line.ToUpper.Contains("OR 20") Then
                    startIndex = line.IndexOf("OR ")
                    line.Replace("OR", "DR")
                    caseNo = line.Substring(startIndex, 17)
                    Return caseNo
                End If
            End While
        Catch ex As Exception

        End Try

        Return "ureadable"
    End Function

    Public Function GetMobileCityAddr(ByVal instNo As String) As String
        Dim fStream As System.IO.StreamReader
        fStream = New System.IO.StreamReader("c:\inetpub\wwwroot\scanneddocs\" + Trim(Str(instNo)) + ".tif.txt")
        Dim line As String
        Dim yearStr As String = Now.Year.ToString


        While Not fStream.EndOfStream
            line = fStream.ReadLine
            If line.Contains("Alabama, to wit:") Then
                line = fStream.ReadLine
                Return Trim(line)
            End If
        End While
        Return "ureadable"
    End Function

    Public Function GetMobileCityAmount(ByVal instNo As String) As String
        Dim fStream As System.IO.StreamReader
        fStream = New System.IO.StreamReader("c:\inetpub\wwwroot\scanneddocs\" + Trim(Str(instNo)) + ".tif.txt")
        Dim line As String
        Dim lineStrs() As String


        While Not fStream.EndOfStream
            line = fStream.ReadLine
            If line.Contains("2. That the said lien") Then
                lineStrs = line.Split(" ")
                For i = 0 To UBound(lineStrs)
                    If lineStrs(i).Contains("$") Then
                        Return lineStrs(i)
                    End If
                Next
            End If
        End While
        Return "ureadable"
    End Function

    Public Function GetBCTaxAddress(ByVal instNo As String)
        Dim fStream As New System.IO.StreamReader("c:\inetpub\wwwroot\scanneddocs\" + Trim(Str(instNo)) + ".tif.txt")
        Dim tDes As String = ""
        Dim line As String = fStream.ReadLine
        Dim retStr As String = ""
        Dim i As Integer

        Try

            While Not fStream.EndOfStream
                line = fStream.ReadLine
                If line.Contains("PHYSICAL ADDRESS:") Then
                    retStr = fStream.ReadLine + " "
                    retStr += fStream.ReadLine
                    retStr = ConvertToTitle(retStr)
                    Return FixSingleQuote(retStr)
                End If
            End While
            fStream.Close()
        Catch ex As Exception
            tDes = "Unreadable"
            fStream.Close()
        End Try
    End Function

    Public Function GetMTTaxAddress(ByVal instNo As String, ByVal Grantee As String)
        Dim fStream As New System.IO.StreamReader("c:\inetpub\wwwroot\scanneddocs\" + Trim(Str(instNo)) + ".tif.txt")
        Dim tDes As String = ""
        Dim line As String = fStream.ReadLine
        Dim retStr As String = ""
        Dim i As Integer

        Try

            While Not fStream.EndOfStream
                line = fStream.ReadLine
                line = line.Replace("'", "")
                If line.Contains(Grantee.ToUpper) Or Grantee.ToUpper.Contains(line) Then
                    retStr = fStream.ReadLine + " "
                    retStr += fStream.ReadLine
                    Return FixSingleQuote(retStr)
                End If
            End While
            fStream.Close()
        Catch ex As Exception
            tDes = "Unreadable"
            fStream.Close()
        End Try
    End Function
    Public Function GetBCTaxAddressPP(ByVal instNo As String)
        Dim fStream As New System.IO.StreamReader("c:\inetpub\wwwroot\scanneddocs\" + Trim(Str(instNo)) + ".tif.txt")
        Dim tDes As String = ""
        Dim line As String = fStream.ReadLine
        Dim lineList As New ArrayList
        Dim i As Integer

        Try

            While Not fStream.EndOfStream
                While Not line.ToUpper.Contains("KNOW ALL PERSONS")
                    line = fStream.ReadLine
                    lineList.Add(line)
                End While
                For i = lineList.Count - 3 To lineList.Count - 2
                    tDes += lineList(i) + " "
                Next

                tDes = ConvertToTitle(tDes)
                Exit While
            End While
            fStream.Close()
        Catch ex As Exception
            tDes = "Unreadable"
            fStream.Close()
        End Try
        tDes = Regex.Replace(tDes, "(\d{5}-\d{4})|(\d{5})", "")
        Try
            If Not Regex.Match(tDes, "\d+").Success Then
                tDes = lineList(lineList.Count - 4) + " " + tDes
            End If
        Catch ex As Exception
            Return "unreadable"
        End Try
        tDes = Regex.Replace(tDes, "Al", "")
        Return FixSingleQuote(Trim(tDes))
    End Function


    Public Function CheckJRType(ByVal instNo As String) As Boolean
        Try

            Dim fStream As New System.IO.StreamReader("c:\inetpub\wwwroot\scanneddocs\" + Trim(Str(instNo)) + ".tif.txt")
            Dim tDes As String = ""
            Dim line As String = fStream.ReadLine
            Dim dontAdd As ArrayList
            Dim i As Integer

            dontAdd = getDontAdd("JR")
            While Not fStream.EndOfStream
                line = fStream.ReadLine
                For i = 0 To dontAdd.Count - 1
                    If line.Contains(dontAdd(i)) Then
                        fStream.Close()
                        Return True
                    End If
                Next

            End While
            fStream.Close()
            Return False
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Function GetUSTaxAddress(ByVal instNo As String)
        Dim fStream As New System.IO.StreamReader("c:\inetpub\wwwroot\scanneddocs\" + Trim(Str(instNo)) + ".tif.txt")
        Dim tDes As String = ""
        Dim line As String = fStream.ReadLine
        Dim gotZip As Boolean = False
        Dim addrArray() As String
        Dim addCount As Integer = 0


        Try

            While Not fStream.EndOfStream
                line = fStream.ReadLine
                If line.ToUpper.Contains("RESIDENCE") Then
                    While Not gotZip
                        addrArray = line.Split(" ")
                        For i = 0 To UBound(addrArray)
                            If addrArray(i).ToUpper.Contains("IMPORTANT") Then
                                Exit While
                            End If
                            tDes += addrArray(i) + " "
                            If (IsNumeric(addrArray(i)) And addrArray(i).Length = 5) Or (addrArray(i).Length = 10 And addrArray(i).Contains("-")) Then
                                gotZip = True
                                Exit While
                            End If
                            If addrArray(i).ToUpper = "AL" Then
                                addCount += 1
                            End If
                            If addCount > 0 Then
                                addCount += 1
                            End If
                            If addCount > 2 Then
                                Exit While
                            End If
                        Next
                        If Not gotZip Then
                            line = fStream.ReadLine
                        End If
                    End While
                    Exit While
                End If
            End While
            fStream.Close()
        Catch ex As Exception
            tDes = "Unreadable"
            fStream.Close()
        End Try

        tDes = tDes.Replace("RESIDENCE", "")
        tDes = tDes.Replace("Residence", "")
        Return FixSingleQuote(tDes)
    End Function

    Public Function GetCaseNo(ByVal instNo As String) As String
        If Not System.IO.File.Exists("c:\inetpub\wwwroot\scanneddocs\" + Trim(Str(instNo)) + ".tif.txt") Then Return "Unreadable"

        Dim fStream As New System.IO.StreamReader("c:\inetpub\wwwroot\scanneddocs\" + Trim(Str(instNo)) + ".tif.txt")
        Dim tDes As String = ""
        Dim line As String
        Try

            While Not fStream.EndOfStream
                line = fStream.ReadLine
                If line.ToUpper.Contains("CV") Or line.ToUpper.Contains("DV") Or line.ToUpper.Contains("SM") Then
                    Dim cArray() As Char
                    cArray = line.ToCharArray
                    Dim i As Integer
                    For i = 0 To UBound(cArray)
                        If (cArray(i) = "C" And cArray(i + 1) = "V") Or (cArray(i) = "D" And cArray(i + 1) = "V") Or (cArray(i) = "S" And cArray(i + 1) = "M") Then
                            tDes += cArray(i) + cArray(i + 1)
                            Dim j As Integer = i + 3
                            While Char.IsDigit(cArray(j)) Or Char.IsWhiteSpace(cArray(j))
                                tDes += cArray(j)
                                j += 1
                            End While
                            fStream.Close()
                            Return tDes
                        End If
                    Next
                    'line = fStream.ReadLine
                End If
            End While
        Catch ex As Exception
            Return "see original"
        End Try
    End Function


    Private Function GetLargestAmt(ByVal amtList As ArrayList, Optional ByVal isRegions As Boolean = False) As String
        Dim i As Integer
        Dim decStr As String
        Dim amtDec As Decimal = 0.0
        Dim lastAmt As Decimal = 0.0
        For i = 0 To amtList.Count - 1
            decStr = amtList(i).ToString.Replace("(", "")
            decStr = decStr.Replace(")", "")
            decStr = decStr.Replace("$", "")
            decStr = decStr.Replace(" ", "")
            decStr = decStr.Replace(",", "")
            If decStr.EndsWith(".") Then
                decStr = decStr.Substring(0, decStr.Length - 1)
            End If
            Try
                amtDec = CDec(decStr)
                If isRegions And amtDec = 250000 Then
                    amtDec = 0
                End If
                If amtDec > lastAmt And amtDec < 100000000 Then
                    lastAmt = amtDec
                End If
            Catch ex As Exception

            End Try
        Next

        Return FormatCurrency(lastAmt)
    End Function
    Public Function DoReplace2(ByVal sStr As String, ByVal rStr As String, ByVal sourceStr As String) As String
        Return Regex.Replace(sourceStr, sStr, rStr)
    End Function

    Public Sub DoReplace(ByVal sStr As String, ByVal rStr As String, ByVal instNo As String)

    End Sub
    Public Function FixSingleQuote(ByVal s As String) As String
        If s Is Nothing Then Return ""

        s = s.Replace("’", "''")
        s = s.Replace("'", "''")
        s = s.Replace("‘", "''")

        Return s
    End Function

    Public Function ConvertToTitle(ByVal s As String) As String
        If s.Length = 0 Then Return s
        s = s.ToLower
        Dim gotSpace As Boolean = False



        Dim arr() As Char = s.ToCharArray
        Dim i As Integer
        arr(0) = Char.ToUpper(arr(0))
        Try

            For i = 0 To UBound(arr)
                If gotSpace Then
                    arr(i) = Char.ToUpper(arr(i))
                End If
                If i = UBound(arr) - 2 And arr(UBound(arr) - 3) = " " Then
                    arr(i) = Char.ToUpper(arr(i))
                End If
                If arr(i) = " " Or arr(i) = "." Then
                    gotSpace = True
                Else
                    gotSpace = False
                End If
            Next

            s = arr
            If s.Length > 254 Then
                s = s.Substring(0, 254)
            End If
        Catch ex As Exception

        End Try

        Return s
    End Function

    Public Function getBusinessNameList() As ArrayList
        Dim retList As New ArrayList
        Dim dr As MySqlDataReader = fmMain.dg.GetDataReader("SELECT busNames FROM businessnames")
        While dr.Read
            retList.Add(Trim(dr(0)))
        End While
        fmMain.dg.KillReader(dr)
        Return retList
    End Function

    Public Function getDontAdd(ByVal tablename As String) As ArrayList

    End Function
    Public Function checkUpper(ByVal str As String) As Boolean
        If Strings.StrComp(str, Strings.UCase(str)) = 0 Then
            Return True
        Else
            Return False
        End If
    End Function

    Public Function isExcluded(ByVal name As String) As Boolean
        Dim exList As ArrayList = getBusinessNameList()
        Dim names() As String = name.Split(" ")
        Dim i As Integer
        For i = 0 To UBound(names)
            names(i) = Regex.Replace(names(i), ",", "")
            If exList.Contains(names(i)) Then
                Return True
            End If
        Next
        Return False
    End Function
    Public Function CheckCase(ByVal item As String) As String
        Dim newStr As String = ""
        Dim wordList() As String
        Dim i As Integer
        item = Regex.Replace(item, "&", " & ")
        wordList = item.Split(" ")
        For i = 0 To UBound(wordList)
            If wordList(i).Length < 3 Then
                If wordList(i).ToUpper <> "OF" And wordList(i).ToUpper <> "TO" Then
                    wordList(i) = wordList(i).ToUpper
                Else
                    wordList(i) = wordList(i).ToLower
                End If
            End If
            newStr += wordList(i) + " "
        Next
        Return newStr
    End Function


End Module
