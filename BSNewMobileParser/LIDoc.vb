Imports System.Text.RegularExpressions

Imports System.IO

Public Class LIDoc
    Inherits MasterDoc
    Public isHospitalLien
    Dim reason1 As String = "for unpaid dues, assessments, etc."
    Dim reason2 As String = "for materials, labor, etc"

    '  Public InstTableAdapter As New BSDocParserConsole.BlueSheetsLocalDataSet1TableAdapters.InstrumentMasterTableAdapter
    '  Public AuxTable3Adapter As New BlueSheetsLocalDataSet1TableAdapters.AuxTable3TableAdapter

    Public Sub New(ByVal dType As bsDocType, ByVal dName As String, Optional ByVal mdType As String = "")
        MyBase.New(dType, dName, mdType)
        dType = dType
        DocType = dName
    End Sub
    Public Overloads Sub ProcessDocument()
        MyBase.ProcessDocument()

        If Grantor.ToUpper.Contains("MEDICAL") Or Grantor.ToUpper.Contains("HOSPITAL") Or Grantor.ToUpper.Contains("CLINIC") Or Grantor.ToUpper.Contains("INFIRMARY") Or Grantor.ToUpper.Contains("UNIVERSITY OF SOUTH ALABAMA HO") Or isHospitalLien Then
            TableName = "H"
            Grantee = Regex.Replace(Grantee, "and\s* \w*\s*\w*\s*\w*\s*\w*\s*\w*", "")
            Desc = Grantor + " claims a lien for medical care received by  " + Grantee
        ElseIf Grantor.ToUpper.Contains("REVENUE COMMISSIONER") Then
            TableName = "MT"
            Grantor = "MOBILE COUNTY"
            'SMTODO
            ConvertFile(currInst, "MT", False, True)
            Value = GetMTTaxAmount(currInst)
            KindTax = GetMTKindTax(currInst)
            'Address = GetMTTaxAddress(currInst, Grantee)
        ElseIf Grantor.ToUpper.Contains("STATE OF ALABAMA") Then
            TableName = "ST"
        ElseIf DocType = "VLD" Then
            TableName = "V"
            AuxTable = 2
        ElseIf Grantor = "MOBILE CITY OF" Then
            ConvertFile(currInst, "LI", False, True)
            Address = GetMobileCityAddr(currInst)
            Value = GetMobileCityAmount(currInst)
            TableName = "LI"
            Desc = "THE CITY OF MOBILE CLAIMS A LIEN AGAINST " + Grantee + " OF " + Value + " ON PROPERTY LOCATED AT " + Address
        Else
            If Address <> "" Then
                Address = Address
            Else
                Address = " see original for address "
            End If
            TableName = "LI"
            Desc = Grantor + " claims a Lien against property owned by " + Grantee
        End If

    End Sub
    Public Sub AddToDatabaseLien()

        Dim cmdStr As String = "INSERT INTO instrumentmasterflat("
        cmdStr = cmdStr + "InstrumentNo, County_ID, DocType,"
        cmdStr = cmdStr + "TableName, Grantor, Grantee, "
        cmdStr = cmdStr + "AuxTableType, NotaryDate, Remarks,"
        cmdStr = cmdStr + "DocFileName, Description, TBSNo,"
        cmdStr = cmdStr + "TBSDate,"
        cmdStr = cmdStr + "Address, DownPayment, Amount)"
        'City, State, Zip, KindTax, 
        'CaseNo

        cmdStr = cmdStr + "VALUES('"
        cmdStr = cmdStr + CStr(currInst) + "'," + CStr(CountyID) + ",'" + DocType + "','"
        cmdStr = cmdStr + TableName + "','" + Grantor + "','" + Grantee + "',"
        cmdStr = cmdStr
        cmdStr = cmdStr
        cmdStr = cmdStr + "3,'" + NotaryDate + "','" + Remarks + "','"
        cmdStr = cmdStr + DocFile + "','" + Desc + "'," + CStr(TBSNo) + ",'"
        cmdStr = cmdStr + TBSDate + "','"
        cmdStr = cmdStr + Address + "','" + DownPayment + "','" + Value + "')"

        'Address, , , , 
        'DeedTax, , , , 
        ', , LienInstr, PrevInst, 
        'City, State, Zip, KindTax, 
        'CaseNo

        Try
            fmMain.dg.RunCommand(cmdStr)
        Catch ex As Exception
            LogError(ex.Message)
        End Try
    End Sub

End Class
