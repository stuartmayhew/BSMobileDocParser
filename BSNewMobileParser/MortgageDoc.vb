Imports System.IO
Imports MySql.Data.MySqlClient

Public Class MortgageDoc
    Inherits MasterDoc

    Dim tLot As String
    Dim tSubdiv As String

    '  Public InstMasterTableAdapter As New BSDocParserConsole.BlueSheetsLocalDataSet1TableAdapters.InstrumentMasterTableAdapter
    '  Public AuxTable2Adapter As New BSDocParserConsole.BlueSheetsLocalDataSet1TableAdapters.AuxTable2TableAdapter


    Public Sub New(ByVal dType As bsDocType, ByVal currInst As String, ByVal dName As String)
        MyBase.New(dType, currInst, dName)
        TableName = "M"
        DocType = dName
    End Sub
    Public Overloads Sub ProcessDocument()
        MyBase.ProcessDocument()
        If Desc.Contains("&nbsp") Or Desc = "" Then
            Desc = Subdivision
        End If
    End Sub

    Public Sub RunCommandString(ByVal DeedType As String)
        Dim cmdStr As String = "INSERT INTO instrumentmasterflat("
        cmdStr = cmdStr + "InstrumentNo, County_ID, DocType,"
        cmdStr = cmdStr + "TableName, Grantor, Grantee, "
        cmdStr = cmdStr + "AuxTableType, NotaryDate, Remarks,"
        cmdStr = cmdStr + "DocFileName, Description, TBSNo,"
        cmdStr = cmdStr + "TBSDate,"
        cmdStr = cmdStr + "Address, DownPayment, Amount,"
        cmdStr = cmdStr + "Sect, TownRange, Subdivision,"
        cmdStr = cmdStr + "Lot, DeedType)"
        'City, State, Zip, KindTax, 
        'CaseNo

        cmdStr = cmdStr + "VALUES('"
        cmdStr = cmdStr + CStr(currInst) + "'," + CStr(CountyID) + ",'" + DocType + "','"
        cmdStr = cmdStr + "M" + "','" + Grantor + "','" + Grantee + "',"
        cmdStr = cmdStr + "2,'" + NotaryDate + "','" + Remarks + "','"
        cmdStr = cmdStr + DocFile + "','" + Desc + "'," + CStr(TBSNo) + ",'"
        cmdStr = cmdStr + TBSDate + "','"
        cmdStr = cmdStr + "" + "','" + DownPayment + "','" + Value + "','"
        cmdStr = cmdStr + Section + "','" + TownRange + "','" + Subdivision + "','"
        cmdStr = cmdStr + Lot + "','" + DeedType + "')"
        'Address, , , , 
        'DeedTax, , , , 
        ', , LienInstr, PrevInst, 
        'City, State, Zip, KindTax, 
        'CaseNo

        '        Try
        fmMain.dg.RunCommand(cmdStr)
        'Catch ex As Exception
        'LogError(ex.Message)
        'End Try
    End Sub
    Public Sub AddToDatabaseMort()
        Try
            If hasDeed() = bsDeedType.bsDeed Then
                RunCommandString("D")
                WriteDeedData()
            ElseIf hasDeed() = bsDeedType.bsCondoDeed Then
                RunCommandString("C")
                WriteCondoDeedData()
            ElseIf hasDeed() = bsDeedType.bsNoDeed Then
                RunCommandString("N")
            Else
                PendingDeedInstrument = 0
            End If


        Catch ex As Exception
            RunCommandString("N")
            'LogError(ex.Message)
        End Try
    End Sub

    Private Function hasDeed() As bsDeedType
        If PendingDeedInstrument = 0 Then
            PendingDeedInstrument = CInt(currInst) - 1
        End If

        Dim dr As MySqlDataReader
        dr = fmMain.dg.GetDataReader("SELECT * FROM instrumentmasterflat WHERE InstrumentNo = " + CStr(PendingDeedInstrument))

        If dr.Read Then
            If Trim(dr("DocType")) = "D" Then
                If SameName(Trim(dr("Grantee")), Grantor) Then
                    hasDeed = bsDeedType.bsDeed
                Else
                    hasDeed = bsDeedType.bsNoDeed
                End If
            ElseIf Trim(dr("DocType")) = "C" Then
                If SameName(Trim(dr("Grantee")), Grantor) Then
                    hasDeed = bsDeedType.bsCondoDeed
                Else
                    hasDeed = bsDeedType.bsNoDeed
                End If
            End If
        Else
            hasDeed = bsDeedType.bsNoDeed
        End If
        fmMain.dg.KillReader(dr)
    End Function

    Private Sub WriteDeedData()

        Dim amount As Double
        Dim amountStr As String
        Dim cmdString As String
        Try
            amount = GetDeedMortAmnt(PendingDeedInstrument)
            amount = amount + CDbl(Value)
            amountStr = CStr(amount)
        Catch ex As Exception
            amountStr = ""
        End Try
        cmdString = "UPDATE instrumentmasterflat SET Amount = '" + amountStr + "',DeedType='D' WHERE InstrumentNo=" + CStr(PendingDeedInstrument)
        fmMain.dg.RunCommand(cmdString)
    End Sub

    Private Sub WriteCondoDeedData()
        Dim amount As Double
        Dim amountStr As String
        Dim cmdString As String

        Try
            amount = GetDeedMortAmnt(PendingDeedInstrument)
            amount = amount + CDbl(Value)
            amountStr = CStr(amount)

        Catch ex As Exception
            amountStr = ""
        End Try
        cmdString = "UPDATE instrumentmasterflat SET Amount = '" + amountStr + "',DeedType='C' WHERE " + CStr(PendingDeedInstrument)
        fmMain.dg.RunCommand(cmdString)
    End Sub
    Private Function SameName(ByVal Name1 As String, ByVal Name2 As String) As Boolean
        Dim Name1array() As String
        Name1.Replace("and", "~")
        Name1array = Name1.Split("~")

        If Name1array(0).Contains(Trim(Name2)) Or Name2.Contains(Name1array(0)) Then
            Return True
        End If

        Return False
    End Function

    Private Function GetDeedMortAmnt(ByVal instNo As String) As Double
        Dim dr As MySqlDataReader
        dr = fmMain.dg.GetDataReader("SELECT AMOUNT FROM instrumentmasterflat WHERE InstrumentNo='" + instNo + "'")
        If dr.Read Then
            Return (dr(0))
        End If
    End Function
End Class

