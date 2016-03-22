<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class fmMain
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing AndAlso components IsNot Nothing Then
            components.Dispose()
        End If
        MyBase.Dispose(disposing)
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(fmMain))
        Me.ListBox1 = New System.Windows.Forms.ListBox()
        Me.tbStartRecp = New System.Windows.Forms.TextBox()
        Me.tbEndRecp = New System.Windows.Forms.TextBox()
        Me.btnMobile = New System.Windows.Forms.Button()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.Label5 = New System.Windows.Forms.Label()
        Me.Label6 = New System.Windows.Forms.Label()
        Me.dpTBSDate = New System.Windows.Forms.DateTimePicker()
        Me.btnClose = New System.Windows.Forms.Button()
        Me.Button2 = New System.Windows.Forms.Button()
        Me.Panel1 = New System.Windows.Forms.Panel()
        Me.WebBrowser1 = New System.Windows.Forms.WebBrowser()
        Me.CheckBox1 = New System.Windows.Forms.CheckBox()
        Me.Panel1.SuspendLayout()
        Me.SuspendLayout()
        '
        'ListBox1
        '
        Me.ListBox1.FormattingEnabled = True
        Me.ListBox1.Location = New System.Drawing.Point(26, 93)
        Me.ListBox1.Name = "ListBox1"
        Me.ListBox1.Size = New System.Drawing.Size(366, 381)
        Me.ListBox1.TabIndex = 0
        '
        'tbStartRecp
        '
        Me.tbStartRecp.Location = New System.Drawing.Point(64, 55)
        Me.tbStartRecp.Name = "tbStartRecp"
        Me.tbStartRecp.Size = New System.Drawing.Size(100, 20)
        Me.tbStartRecp.TabIndex = 1
        Me.tbStartRecp.Text = "2016012469"
        '
        'tbEndRecp
        '
        Me.tbEndRecp.Location = New System.Drawing.Point(197, 55)
        Me.tbEndRecp.Name = "tbEndRecp"
        Me.tbEndRecp.Size = New System.Drawing.Size(100, 20)
        Me.tbEndRecp.TabIndex = 2
        Me.tbEndRecp.Text = "2016012621"
        '
        'btnMobile
        '
        Me.btnMobile.Location = New System.Drawing.Point(332, 52)
        Me.btnMobile.Name = "btnMobile"
        Me.btnMobile.Size = New System.Drawing.Size(75, 21)
        Me.btnMobile.TabIndex = 3
        Me.btnMobile.Text = "Get Mobile"
        Me.btnMobile.UseVisualStyleBackColor = True
        '
        'Label4
        '
        Me.Label4.AutoSize = True
        Me.Label4.Location = New System.Drawing.Point(19, 57)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(33, 13)
        Me.Label4.TabIndex = 11
        Me.Label4.Text = "Recp"
        '
        'Label5
        '
        Me.Label5.AutoSize = True
        Me.Label5.Location = New System.Drawing.Point(176, 57)
        Me.Label5.Name = "Label5"
        Me.Label5.Size = New System.Drawing.Size(16, 13)
        Me.Label5.TabIndex = 12
        Me.Label5.Text = "to"
        '
        'Label6
        '
        Me.Label6.AutoSize = True
        Me.Label6.Location = New System.Drawing.Point(209, 17)
        Me.Label6.Name = "Label6"
        Me.Label6.Size = New System.Drawing.Size(54, 13)
        Me.Label6.TabIndex = 14
        Me.Label6.Text = "TBS Date"
        '
        'dpTBSDate
        '
        Me.dpTBSDate.Format = System.Windows.Forms.DateTimePickerFormat.[Short]
        Me.dpTBSDate.Location = New System.Drawing.Point(269, 12)
        Me.dpTBSDate.Name = "dpTBSDate"
        Me.dpTBSDate.Size = New System.Drawing.Size(97, 20)
        Me.dpTBSDate.TabIndex = 15
        '
        'btnClose
        '
        Me.btnClose.Location = New System.Drawing.Point(308, 499)
        Me.btnClose.Name = "btnClose"
        Me.btnClose.Size = New System.Drawing.Size(75, 23)
        Me.btnClose.TabIndex = 17
        Me.btnClose.Text = "Close"
        Me.btnClose.UseVisualStyleBackColor = True
        '
        'Button2
        '
        Me.Button2.BackColor = System.Drawing.Color.Red
        Me.Button2.Location = New System.Drawing.Point(17, 13)
        Me.Button2.Name = "Button2"
        Me.Button2.Size = New System.Drawing.Size(75, 23)
        Me.Button2.TabIndex = 18
        Me.Button2.Text = "Abort"
        Me.Button2.UseVisualStyleBackColor = False
        '
        'Panel1
        '
        Me.Panel1.Controls.Add(Me.WebBrowser1)
        Me.Panel1.Location = New System.Drawing.Point(439, 0)
        Me.Panel1.Name = "Panel1"
        Me.Panel1.Size = New System.Drawing.Size(410, 542)
        Me.Panel1.TabIndex = 19
        '
        'WebBrowser1
        '
        Me.WebBrowser1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.WebBrowser1.Location = New System.Drawing.Point(0, 0)
        Me.WebBrowser1.MinimumSize = New System.Drawing.Size(20, 20)
        Me.WebBrowser1.Name = "WebBrowser1"
        Me.WebBrowser1.Size = New System.Drawing.Size(410, 542)
        Me.WebBrowser1.TabIndex = 0
        '
        'CheckBox1
        '
        Me.CheckBox1.AutoSize = True
        Me.CheckBox1.Checked = True
        Me.CheckBox1.CheckState = System.Windows.Forms.CheckState.Checked
        Me.CheckBox1.Location = New System.Drawing.Point(26, 499)
        Me.CheckBox1.Name = "CheckBox1"
        Me.CheckBox1.Size = New System.Drawing.Size(98, 17)
        Me.CheckBox1.TabIndex = 20
        Me.CheckBox1.Text = "Scan Doc Text"
        Me.CheckBox1.UseVisualStyleBackColor = True
        '
        'fmMain
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(438, 538)
        Me.ControlBox = False
        Me.Controls.Add(Me.CheckBox1)
        Me.Controls.Add(Me.Panel1)
        Me.Controls.Add(Me.Button2)
        Me.Controls.Add(Me.btnClose)
        Me.Controls.Add(Me.dpTBSDate)
        Me.Controls.Add(Me.Label6)
        Me.Controls.Add(Me.Label5)
        Me.Controls.Add(Me.Label4)
        Me.Controls.Add(Me.btnMobile)
        Me.Controls.Add(Me.tbEndRecp)
        Me.Controls.Add(Me.tbStartRecp)
        Me.Controls.Add(Me.ListBox1)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MaximizeBox = False
        Me.Name = "fmMain"
        Me.Text = "Parser"
        Me.Panel1.ResumeLayout(False)
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents ListBox1 As System.Windows.Forms.ListBox
    Friend WithEvents tbStartRecp As System.Windows.Forms.TextBox
    Friend WithEvents tbEndRecp As System.Windows.Forms.TextBox
    Friend WithEvents btnMobile As System.Windows.Forms.Button
    Friend WithEvents Label4 As System.Windows.Forms.Label
    Friend WithEvents Label5 As System.Windows.Forms.Label
    Friend WithEvents Label6 As System.Windows.Forms.Label
    Friend WithEvents btnClose As System.Windows.Forms.Button
    Friend WithEvents Button2 As System.Windows.Forms.Button
    Protected WithEvents dpTBSDate As System.Windows.Forms.DateTimePicker
    Friend WithEvents Panel1 As System.Windows.Forms.Panel
    Friend WithEvents WebBrowser1 As System.Windows.Forms.WebBrowser
    Friend WithEvents CheckBox1 As System.Windows.Forms.CheckBox
End Class
