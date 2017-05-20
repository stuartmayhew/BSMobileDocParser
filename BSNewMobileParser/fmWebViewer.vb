Imports System.IO
Imports System.Windows.Forms
Imports System.Net
Imports System
Imports System.Collections.Generic
Imports System.ComponentModel
Imports System.Data
Imports System.Collections
Imports System.Runtime.InteropServices


Public Class fmWebViewer
    Public currInstStr As String
    Dim logCount As Integer = 0

    Private Sub fmWebViewer_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        WebBrowser1.Navigate("https://roam.probate.mobilecountyal.gov/ailis/publicLogin.admin")
    End Sub

    Private Sub WebBrowser1_DocumentCompleted(ByVal sender As Object, ByVal e As System.Windows.Forms.WebBrowserDocumentCompletedEventArgs) Handles WebBrowser1.DocumentCompleted
        Dim element As HtmlElement
        If WebBrowser1.ReadyState <> WebBrowserReadyState.Complete Then Exit Sub
        'tbURL.Text = WebBrowser1.Url.AbsoluteUri
        If WebBrowser1.Url.AbsoluteUri.Contains("publicLogin") Then
            logCount += 1
            If logCount > 2 Then
                WebBrowser1.Navigate("about:blank")
                Me.DialogResult = Windows.Forms.DialogResult.Cancel
                Close()
            End If
            element = WebBrowser1.Document.GetElementById("j_username")
            element.InnerText = "stumay111@gmail.com"
            element = WebBrowser1.Document.GetElementById("j_password")
            element.InnerText = "shadow111"
            element = WebBrowser1.Document.GetElementById("btnLogin")
            element.InvokeMember("click")
        ElseIf WebBrowser1.Url.AbsoluteUri.Contains("checkSubscription") Then
            If logCount = 2 Then
                WebBrowser1.Navigate("https://roam.probate.mobilecountyal.gov/ailis/search.do?indexName=mobimages&lq=Instrument%3A" + Trim(Str(currInst)))
            End If
            If WebBrowser1.DocumentText.Contains("Advanced Search") Then
                Dim wc As New WebClient()
                Dim urlStr As String = "https://roam.probate.mobilecountyal.gov/ailis/search.do?indexName=details&templateName=default&lq=Instrument:" + currInstStr
                WebBrowser1.Navigate(urlStr)
            End If
        ElseIf WebBrowser1.Url.AbsoluteUri.Contains("https://roam.probate.mobilecountyal.gov/ailis/search.do?indexName=details") Then
            Dim sourceCode As String = WebBrowser1.DocumentText
            Dim someFile As String = "c:\inetpub\wwwroot\scanneddocs\mPage" + Trim(currInstStr) + ".htm"
            Dim sw As New StreamWriter(someFile)
            sw.WriteLine(sourceCode)
            sw.Close()
            sw.Dispose()
            sw = Nothing
            WebBrowser1.Navigate("https://roam.probate.mobilecountyal.gov/ailis/search.do?indexName=mobimages&lq=Instrument%3A" + Trim(Str(currInst)))

        ElseIf WebBrowser1.Document.Body.InnerHtml.Contains("expired") Then
            WebBrowser1.Navigate("about:blank")
            Me.DialogResult = Windows.Forms.DialogResult.Cancel
            Close()
        ElseIf WebBrowser1.Document.Body.InnerHtml.Contains("<b>1</b> - <b>0</b> of <b>0</b> for <b></b>") Then
            WebBrowser1.Navigate("about:blank")
            Me.DialogResult = Windows.Forms.DialogResult.Cancel
            Close()
        End If
    End Sub


    Private Sub WebBrowser1_Navigated(ByVal sender As Object, ByVal e As System.Windows.Forms.WebBrowserNavigatedEventArgs) Handles WebBrowser1.Navigated
        If WebBrowser1.Url.AbsoluteUri.Contains("indexName=mobimages") Then
            WebBrowser1.Navigate("about:blank")
            Me.DialogResult = Windows.Forms.DialogResult.Cancel
            Close()
        End If
    End Sub
End Class