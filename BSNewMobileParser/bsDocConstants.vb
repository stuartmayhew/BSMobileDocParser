Public Module BSDocConstants
    Public AG As MasterDoc
    Public AI As MasterDoc
    Public AM As MasterDoc
    Public AO As MasterDoc
    Public Cdeed As MasterDoc
    Public FO As MasterDoc
    Public DE As MasterDoc
    Public DEED As MasterDoc
    Public MAP As MasterDoc
    Public MORT As MortgageDoc
    Public JUDGE As MasterDoc
    Public V As MasterDoc
    Public REL As RDoc
    Public TLien As TaxLienDoc
    Public LI As LIDoc
    Public LIS As MasterDoc
    Public DI As MasterDoc
    Public FB As MasterDoc
    Public FL As MasterDoc
    Public LE As MasterDoc
    Public ORD As MasterDoc
    Public DECL As MasterDoc
    Public ORDI As MasterDoc
    Public O As MasterDoc
    Public LIC As MasterDoc

    Public Const TBSNo As Integer = 9999

    Public Enum bsDocType As Integer
        bsDeltaDoc = 1
        bsMobileDoc = 2
    End Enum
    Public Enum bsLineType As Integer
        bsNotDate = 1
        bsGrantor = 2
        bsGrantee = 3
        bsDesc = 4
        bsDocImage = 5
        bsDeedBook = 6
        bsValue = 7
        bsDownPayment = 8
        bsLotBlockSub = 11
        bsMineralAcres = 12
        bsRecDate = 13
        bsRemarks = 14
    End Enum

    Public currInst As String
    Public currBSNO As String
    Public currBSDate As Date
    Public Enum bsDeedType As Integer
        bsNoDeed = 0
        bsDeed = 1
        bsCondoDeed = 2
    End Enum
    Public Enum bsReleaseType As Integer
        bsUSTax = 1
        bsStateTax = 2
        bsLien = 3
    End Enum
End Module
