Imports System.IO
Public Class RDoc
    Inherits MasterDoc
    Private isMortgageRelease As Boolean = False


    Public Sub New(ByVal dType As bsDocType, ByVal currInst As String, ByVal dName As String)
        MyBase.New(dType, currInst, dName)
        DocType = dName
    End Sub
    Public Overloads Sub ProcessDocument()
        MyBase.ProcessDocument()
        If Grantor.ToUpper.Contains("BANK") Or Grantor.ToUpper.Contains("MORTGAGE") Or Grantor.ToUpper.Contains("MORT") Or Grantor.ToUpper.Contains("FINANCE") Or Grantor.ToUpper.Contains("MEDICAL") Then
            isMortgageRelease = True
            Exit Sub
        End If

        If Grantor.ToUpper.Contains("DEPARTMENT OF THE TREASURY") Or Grantor.ToUpper.Contains("STATE OF ALABAMA") Or Grantor.ToUpper.Contains("REVENUE COMMISSIONER") Then
            Dim tGrant As String = Grantee
            Grantee = Grantor
            Grantor = tGrant
        End If






        If Grantee.ToUpper.Contains("UNITED STATES") Then
            TableName = "UR"
            Grantee = Grantor
        ElseIf Grantee.ToUpper.Contains("DEPARTMENT OF THE TREASURY") Then
            TableName = "UR"
            Grantee = Grantor
        ElseIf Grantee.ToUpper.Contains("STATE OF ALABAMA") Then
            TableName = "SR"
            Grantee = Grantor
        ElseIf Grantee.ToUpper.Contains("REVENUE COMMISSIONER") Then
            TableName = "MR"
            Grantee = Grantor
        Else
            TableName = "JR"
        End If
    End Sub
    Public Sub AddToDatabaseRel()

        If isMortgageRelease Then Exit Sub

        Desc = Grantee + " " + Value + " "
        Dim cmdStr As String = "INSERT INTO instrumentmasterflat("
        cmdStr = cmdStr + "InstrumentNo, County_ID, DocType,"
        cmdStr = cmdStr + "TableName, Grantor, Grantee, "
        cmdStr = cmdStr + "AuxTableType, NotaryDate, Remarks,"
        cmdStr = cmdStr + "DocFileName, Description, TBSNo,"
        cmdStr = cmdStr + "TBSDate,"
        cmdStr = cmdStr + "Address, DownPayment, Amount,PrevInst)"

        cmdStr = cmdStr + "VALUES('"
        cmdStr = cmdStr + CStr(currInst) + "'," + CStr(CountyID) + ",'" + DocType + "','"
        cmdStr = cmdStr + TableName + "','" + Grantor + "','" + Grantee + "',"
        cmdStr = cmdStr
        cmdStr = cmdStr
        cmdStr = cmdStr + "3,'" + NotaryDate + "','" + Remarks + "','"
        cmdStr = cmdStr + DocFile + "','" + Desc + "'," + CStr(TBSNo) + ",'"
        cmdStr = cmdStr + TBSDate + "','"
        cmdStr = cmdStr + "','" + DownPayment + "','" + Value + "','" + PrevInst + "')"

        Try
            fmMain.dg.RunCommand(cmdStr)
        Catch ex As Exception
            LogError(ex.Message)
        End Try
    End Sub

End Class
