
Imports MySql.Data.MySqlClient

Public Class clsDataGetter
    Dim conn As MySqlConnection
    Dim cnStr As String


    Public Sub New(ByVal connStr As String)
        conn = New MySqlConnection(connStr)
        cnStr = connStr
    End Sub

    Public Function GetDataReader(ByVal sql As String) As MySqlDataReader
        Dim dr As MySqlDataReader
        Dim cmd As New MySqlCommand(sql, conn)
        If Not conn.State = Data.ConnectionState.Open Then
            conn.Open()
        End If
        Try
            dr = cmd.ExecuteReader

        Catch ex As Exception
            conn.Close()
        End Try
        Return dr
    End Function

    Public Function HasData(ByVal sql As String) As Boolean
        Dim dr As MySqlDataReader
        Dim conn2 As New MySqlConnection(cnStr)

        Dim cmd As New MySqlCommand(sql, conn2)
        If Not conn2.State = Data.ConnectionState.Open Then
            conn2.Open()
        End If
        Try
            dr = cmd.ExecuteReader
            If dr.Read Then
                dr.Close()
                Return True
            End If
        Catch ex As Exception
            conn2.Close()
            If Not dr.IsClosed Then dr.Close()
            Return False
        End Try
        conn2.Close()
        dr.Close()
        Return False
    End Function

    Public Function GetScalarInteger(ByVal sql As String) As Integer
        Dim tConn As New MySql.Data.MySqlClient.MySqlConnection(cnStr)
        Dim x As Integer
        tConn.Open()

        Dim cmd As New MySqlCommand(sql, tConn)
        x = cmd.ExecuteScalar
        tConn.Close()
        tConn.Dispose()
        Return x
    End Function

    Public Function GetScalarString(ByVal sql As String) As String
        Dim tConn As New MySql.Data.MySqlClient.MySqlConnection(cnStr)
        Dim x As String
        tConn.Open()

        Dim cmd As New MySqlCommand(sql, tConn)
        x = cmd.ExecuteScalar
        tConn.Close()
        tConn.Dispose()
        Return x
    End Function


    Public Sub RunCommand(ByVal sql As String)
        Dim conn2 As New MySqlConnection(cnStr)
        conn2.Open()

        Dim cmd As New MySqlCommand(sql, conn2)
        Try
            cmd.ExecuteNonQuery()
        Catch ex As Exception
            ' MsgBox(ex.Message)
            conn2.Close()
        End Try
        conn2.Close()
    End Sub

    Public Sub RunCommand(ByVal sql As String, ByVal paramList As ArrayList)
        Dim conn2 As New MySqlConnection(cnStr)
        Dim i As Integer
        Dim cmd As New MySqlCommand(sql, conn2)
        cmd.CommandType = Data.CommandType.StoredProcedure

        conn2.Open()
        While i <= paramList.Count - 1
            cmd.Parameters.AddWithValue(paramList(i), paramList(i + 1))
            i = i + 2
        End While

        Try
            cmd.ExecuteNonQuery()
        Catch ex As Exception
            conn2.Close()
            Throw ex
        End Try
        conn2.Close()
    End Sub
    Public Sub KillReader(ByRef rdr As MySqlDataReader)
        rdr.Close()
    End Sub

    Public Sub Close()
        conn.Close()
    End Sub
End Class
