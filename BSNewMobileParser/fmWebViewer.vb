Imports System.IO
Imports System.Windows.Forms
Imports System.Net
Imports System
Imports System.Collections.Generic
Imports System.ComponentModel
Imports System.Data
Imports System.Collections
Imports System.Runtime.InteropServices
Imports System.Security.AccessControl

Public Class fmWebViewer
    Public currInstStr As String
    Public currSessionString As String = ""
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
            'Dim url As String = "https://roam.probate.mobilecountyal.gov/ailis/search.do?indexName=mobimages&lq=Instrument%3A" + Trim(Str(currInst))
            'Dim request As HttpWebRequest = HttpWebRequest.Create(url)
            'Dim response As HttpWebResponse = request.GetResponse()
            'Dim sessionStr As String = response.ResponseUri.LocalPath.Split(";")(1)
            'currSessionString = sessionStr.Split("=")(1)
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
            Dim fName As String = "c:\inetpub\wwwroot\scanneddocs\" + Trim(Str(currInst)) + ".pdf"
            Dim path As String = Environment.GetFolderPath(Environment.SpecialFolder.InternetCache) + "\Content.IE5"
            Dim fInfo, fInfos() As FileInfo
            Dim dInfo, dInfos() As DirectoryInfo
            dInfo = New DirectoryInfo(path)
            Dim FolderAcl As New DirectorySecurity
            FolderAcl.AddAccessRule(New FileSystemAccessRule("Everyone", FileSystemRights.Modify, InheritanceFlags.ContainerInherit Or InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow))
            FolderAcl.SetAccessRuleProtection(True, False) 'uncomment to remove existing permissions
            dInfo.SetAccessControl(FolderAcl)
            dInfos = dInfo.GetDirectories()
            Dim hasFile As Boolean = False
            For i = 0 To dInfos.Count - 1
                fInfos = dInfos(i).GetFiles("*")
                For Each fInfo In fInfos
                    If fInfo.Name.Contains("document") And fInfo.Extension = ".pdf" Then
                        File.Copy(fInfo.FullName, fName, True)
                        File.Delete(fInfo.FullName)
                        hasFile = True
                        Exit For
                    End If
                Next
                If hasFile Then
                    Exit For
                End If
            Next
            If Not hasFile Then
                ShowStatus("Failed to get PDF for " + Str(currInst))
            End If
            DelIECache()
            WebBrowser1.Navigate("about:blank")
            Me.DialogResult = Windows.Forms.DialogResult.Cancel
            Close()
        End If
    End Sub

    Private Sub WebBrowser1_Navigating(sender As Object, e As WebBrowserNavigatingEventArgs) Handles WebBrowser1.Navigating
        Dim x = e.Url
    End Sub

    Public Sub DelIECache()

        Dim path As String = Environment.GetFolderPath(Environment.SpecialFolder.InternetCache) + "\Content.IE5"
        Dim dInfo, dInfos() As DirectoryInfo
        dInfo = New DirectoryInfo(path)
        Dim FolderAcl As New DirectorySecurity
        FolderAcl.AddAccessRule(New FileSystemAccessRule("Everyone", FileSystemRights.Modify, InheritanceFlags.ContainerInherit Or InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow))
        FolderAcl.SetAccessRuleProtection(True, False) 'uncomment to remove existing permissions
        dInfo.SetAccessControl(FolderAcl)
        dInfos = dInfo.GetDirectories()
        For i = 0 To dInfos.Count - 1
            Try
                dInfos(i).Delete(True)
            Catch ex As Exception

            End Try

        Next
    End Sub
End Class