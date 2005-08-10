Imports atcControls.atcGrid
Imports atcData

Public Class frmSpecifyComputation
  Inherits System.Windows.Forms.Form

#Region " Windows Form Designer generated code "

  Public Sub New()
    MyBase.New()

    'This call is required by the Windows Form Designer.
    InitializeComponent()

    'Add any initialization after the InitializeComponent() call

  End Sub

  'Form overrides dispose to clean up the component list.
  Protected Overloads Overrides Sub Dispose(ByVal disposing As Boolean)
    If disposing Then
      If Not (components Is Nothing) Then
        components.Dispose()
      End If
    End If
    MyBase.Dispose(disposing)
  End Sub

  'Required by the Windows Form Designer
  Private components As System.ComponentModel.IContainer

  'NOTE: The following procedure is required by the Windows Form Designer
  'It can be modified using the Windows Form Designer.  
  'Do not modify it using the code editor.
  Friend WithEvents pnlButtons As System.Windows.Forms.Panel
  Friend WithEvents btnCancel As System.Windows.Forms.Button
  Friend WithEvents btnOk As System.Windows.Forms.Button
  Friend WithEvents lblName As System.Windows.Forms.Label
  Friend WithEvents lblDescription As System.Windows.Forms.Label
  <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
    Me.pnlButtons = New System.Windows.Forms.Panel
    Me.btnCancel = New System.Windows.Forms.Button
    Me.btnOk = New System.Windows.Forms.Button
    Me.lblName = New System.Windows.Forms.Label
    Me.lblDescription = New System.Windows.Forms.Label
    Me.pnlButtons.SuspendLayout()
    Me.SuspendLayout()
    '
    'pnlButtons
    '
    Me.pnlButtons.Controls.Add(Me.btnCancel)
    Me.pnlButtons.Controls.Add(Me.btnOk)
    Me.pnlButtons.Dock = System.Windows.Forms.DockStyle.Bottom
    Me.pnlButtons.Location = New System.Drawing.Point(0, 205)
    Me.pnlButtons.Name = "pnlButtons"
    Me.pnlButtons.Size = New System.Drawing.Size(520, 40)
    Me.pnlButtons.TabIndex = 13
    '
    'btnCancel
    '
    Me.btnCancel.Location = New System.Drawing.Point(104, 8)
    Me.btnCancel.Name = "btnCancel"
    Me.btnCancel.Size = New System.Drawing.Size(80, 24)
    Me.btnCancel.TabIndex = 4
    Me.btnCancel.Text = "Cancel"
    '
    'btnOk
    '
    Me.btnOk.Location = New System.Drawing.Point(8, 8)
    Me.btnOk.Name = "btnOk"
    Me.btnOk.Size = New System.Drawing.Size(80, 24)
    Me.btnOk.TabIndex = 3
    Me.btnOk.Text = "Ok"
    '
    'lblName
    '
    Me.lblName.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
    Me.lblName.Location = New System.Drawing.Point(8, 8)
    Me.lblName.Name = "lblName"
    Me.lblName.Size = New System.Drawing.Size(504, 16)
    Me.lblName.TabIndex = 14
    '
    'lblDescription
    '
    Me.lblDescription.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
    Me.lblDescription.Location = New System.Drawing.Point(8, 32)
    Me.lblDescription.Name = "lblDescription"
    Me.lblDescription.Size = New System.Drawing.Size(504, 16)
    Me.lblDescription.TabIndex = 15
    '
    'frmSpecifyComputation
    '
    Me.AutoScaleBaseSize = New System.Drawing.Size(5, 13)
    Me.ClientSize = New System.Drawing.Size(520, 245)
    Me.Controls.Add(Me.lblDescription)
    Me.Controls.Add(Me.lblName)
    Me.Controls.Add(Me.pnlButtons)
    Me.Name = "frmSpecifyComputation"
    Me.Text = "Specify Computation"
    Me.pnlButtons.ResumeLayout(False)
    Me.ResumeLayout(False)

  End Sub

#End Region

  Private Const ARG_BUTTON_WIDTH = 80
  Private Const PADDING As Integer = 5

  Private pSelectedOK As Boolean
  Private pAttributes As atcData.atcDataAttributes
  Private pDataManager As atcDataManager

  Private pArgDefVal() As atcDefinedValue
  Private pArgLabel() As Windows.Forms.Label
  Private pArgText() As Windows.Forms.TextBox
  Private pArgButton() As Windows.Forms.Button

  'Private pArgSource As atcControls.atcGridSource  ' atcDataAttributesGridSource

  Public Function AskUser(ByVal aDataManager As atcDataManager, _
                          ByRef aArgs As atcData.atcDataAttributes) As Boolean
    pSelectedOK = False
    pAttributes = aArgs.Clone 'so we are not changing the original until Ok is clicked
    pDataManager = aDataManager

    Populate()
    'pArgSource = New atcDataAttributesGridSource(pArgs)
    'agdArguments.Initialize(pArgSource)

    Me.ShowDialog()
    If pSelectedOK Then
      aArgs = pAttributes
      Return True
    Else
      Return False
    End If
  End Function

  Private Sub Populate()
    ReDim pArgLabel(-1)
    ReDim pArgText(-1)
    ReDim pArgButton(-1)

    For Each lArg As atcDefinedValue In pAttributes
      AddArg(lArg)
    Next
  End Sub

  Private Sub AddArg(ByVal aDefVal As atcDefinedValue)
    Select Case aDefVal.Definition.Name
      Case "Name" : lblName.Text = aDefVal.Value
      Case "Description" : lblDescription.Text = aDefVal.Value
      Case Else
        Dim iArg As Integer = pArgLabel.GetUpperBound(0)
        iArg += 1

        ReDim Preserve pArgDefVal(iArg)
        pArgDefVal(iArg) = aDefVal

        ReDim Preserve pArgLabel(iArg)
        pArgLabel(iArg) = New Windows.Forms.Label
        With pArgLabel(iArg)
          If iArg = 0 Then
            .Top = lblDescription.Top + lblDescription.Height * 1.5
          Else
            .Top = pArgLabel(iArg - 1).Top + pArgLabel(iArg - 1).Height * 1.5
          End If
          .Left = lblDescription.Left
          .Text = aDefVal.Definition.Name
        End With
        Controls.Add(pArgLabel(iArg))

        ReDim Preserve pArgText(iArg)
        pArgText(iArg) = New Windows.Forms.TextBox
        With pArgText(iArg)
          .Top = pArgLabel(iArg).Top
          .Left = pArgLabel(iArg).Left + pArgLabel(iArg).Width + PADDING
          .Width = ClientRectangle.Width - pArgText(iArg).Left - ARG_BUTTON_WIDTH - PADDING * 2
          .Anchor = Windows.Forms.AnchorStyles.Right Or Windows.Forms.AnchorStyles.Left

          .Tag = iArg
          If aDefVal.Value Is Nothing Then
            .Text = ""
          Else
            .Text = ""
          End If
          AddHandler pArgText(iArg).TextChanged, AddressOf ArgText_TextChanged
        End With
        Controls.Add(pArgText(iArg))

        ReDim Preserve pArgButton(iArg)
        pArgButton(iArg) = New Windows.Forms.Button
        With pArgButton(iArg)
          .Top = pArgLabel(iArg).Top
          .Width = ARG_BUTTON_WIDTH
          .Left = ClientRectangle.Width - pArgButton(iArg).Width - PADDING

          .Tag = iArg
          .Text = "Select"
          .Anchor = Windows.Forms.AnchorStyles.Right

          Select Case aDefVal.Definition.TypeString
            Case "atcDataGroup" : .Visible = True
            Case "atcTimeseries" : .Visible = True
            Case Else : .Visible = False
          End Select
          AddHandler pArgButton(iArg).Click, AddressOf ArgButton_Click
        End With
        Controls.Add(pArgButton(iArg))

    End Select
  End Sub

  Private Sub btnOk_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnOk.Click
    pSelectedOK = True
    Close()
  End Sub

  Private Sub btnCancel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnCancel.Click
    Close()
  End Sub

  Private Sub ArgButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
    Dim iArg As Integer = CInt(sender.tag)
    'TODO: ask UserSelectData to only allow one selection if only one is wanted (atcTimeseries rather than atcDataGroup)
    Dim lSelected As atcDataGroup = pDataManager.UserSelectData("Select " _
                                                               & pArgLabel(iArg).Text _
                                                               & " for " & lblName.Text)
    pArgDefVal(iArg).Value = lSelected

    If lSelected Is Nothing OrElse lSelected.Count = 0 Then
      pArgText(iArg).Text = ""
    ElseIf lSelected.Count = 1 Then
      pArgText(iArg).Text = lSelected.Item(0).ToString
    Else
      pArgText(iArg).Text = lSelected.Count & " data sets"
    End If
  End Sub

  Private Sub ArgText_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs)
    Dim iArg As Integer = CInt(sender.tag)
    If Not pArgButton(iArg).Visible Then
      pArgDefVal(iArg).Value = sender.Text
    End If
  End Sub
End Class
