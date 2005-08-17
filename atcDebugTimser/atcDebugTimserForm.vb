Imports System.Windows.Forms
Imports atcData
Imports atcUtility

Public Class atcDebugTimserForm
  Inherits System.Windows.Forms.Form

#Region " Windows Form Designer generated code "

  Public Sub New(ByVal aDataManager As atcData.atcDataManager, _
        Optional ByVal aDataGroup As atcData.atcDataGroup = Nothing)
    MyBase.New()
    pDataManager = aDataManager
    If aDataGroup Is Nothing Then
      pDataGroup = New atcDataGroup
    Else
      pDataGroup = aDataGroup
    End If
    InitializeComponent() 'required by Windows Form Designer

    Dim DisplayPlugins As ICollection = pDataManager.GetPlugins(GetType(atcDataDisplay))
    For Each ldisp As atcDataDisplay In DisplayPlugins
      mnuAnalysis.MenuItems.Add(ldisp.Name, New EventHandler(AddressOf mnuAnalysis_Click))
    Next

    If pDataGroup.Count = 0 Then 'ask user to specify some Data
      mnuFileAdd_Click(Nothing, Nothing)
    End If

    If pDataGroup.Count > 0 Then
      PopulateTree()
    Else 'user declined to specify Data
      Me.Close()
    End If
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
  Friend WithEvents mnuFile As System.Windows.Forms.MenuItem
  Friend WithEvents mnuFileAdd As System.Windows.Forms.MenuItem
  Friend WithEvents mnuFileSave As System.Windows.Forms.MenuItem
  Friend WithEvents mnuView As System.Windows.Forms.MenuItem
  Friend WithEvents mnuExpand As System.Windows.Forms.MenuItem
  Friend WithEvents mnuCollapse As System.Windows.Forms.MenuItem
  Friend WithEvents mnuDefault As System.Windows.Forms.MenuItem
  Friend WithEvents mnuAnalysis As System.Windows.Forms.MenuItem
  Friend WithEvents mnuMain As System.Windows.Forms.MainMenu
  Friend WithEvents mnuCopyClipboard As System.Windows.Forms.MenuItem
  <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
    Dim resources As System.Resources.ResourceManager = New System.Resources.ResourceManager(GetType(atcDebugTimserForm))
    Me.mnuFile = New System.Windows.Forms.MenuItem
    Me.mnuFileAdd = New System.Windows.Forms.MenuItem
    Me.mnuFileSave = New System.Windows.Forms.MenuItem
    Me.mnuView = New System.Windows.Forms.MenuItem
    Me.mnuExpand = New System.Windows.Forms.MenuItem
    Me.mnuCollapse = New System.Windows.Forms.MenuItem
    Me.mnuDefault = New System.Windows.Forms.MenuItem
    Me.mnuAnalysis = New System.Windows.Forms.MenuItem
    Me.mnuMain = New System.Windows.Forms.MainMenu
    Me.mnuCopyClipboard = New System.Windows.Forms.MenuItem
    '
    'mnuFile
    '
    Me.mnuFile.Index = 0
    Me.mnuFile.MenuItems.AddRange(New System.Windows.Forms.MenuItem() {Me.mnuFileAdd, Me.mnuCopyClipboard, Me.mnuFileSave})
    Me.mnuFile.Text = "&File"
    '
    'mnuFileAdd
    '
    Me.mnuFileAdd.Index = 0
    Me.mnuFileAdd.Text = "&Add Data"
    '
    'mnuFileSave
    '
    Me.mnuFileSave.Index = 2
    Me.mnuFileSave.Text = "&Save"
    '
    'mnuView
    '
    Me.mnuView.Index = 1
    Me.mnuView.MenuItems.AddRange(New System.Windows.Forms.MenuItem() {Me.mnuExpand, Me.mnuCollapse, Me.mnuDefault})
    Me.mnuView.Text = "&View"
    '
    'mnuExpand
    '
    Me.mnuExpand.Index = 0
    Me.mnuExpand.Text = "&Expand"
    '
    'mnuCollapse
    '
    Me.mnuCollapse.Index = 1
    Me.mnuCollapse.Text = "&Collapse"
    '
    'mnuDefault
    '
    Me.mnuDefault.Index = 2
    Me.mnuDefault.Text = "&Default"
    '
    'mnuAnalysis
    '
    Me.mnuAnalysis.Index = 2
    Me.mnuAnalysis.Text = "&Analysis"
    '
    'mnuMain
    '
    Me.mnuMain.MenuItems.AddRange(New System.Windows.Forms.MenuItem() {Me.mnuFile, Me.mnuView, Me.mnuAnalysis})
    '
    'mnuCopyClipboard
    '
    Me.mnuCopyClipboard.Index = 1
    Me.mnuCopyClipboard.Text = "Copy to Clipboard"
    '
    'atcDebugTimserForm
    '
    Me.AutoScaleBaseSize = New System.Drawing.Size(6, 15)
    Me.ClientSize = New System.Drawing.Size(633, 628)
    Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
    Me.Menu = Me.mnuMain
    Me.Name = "atcDebugTimserForm"
    Me.Text = "Data Debug"

  End Sub

#End Region

  Private pDataManager As atcDataManager
  Private WithEvents atrMain As TreeView   'tree control
  Private WithEvents pDataGroup As atcDataGroup   'group of atcData displayed
  Private pNumValuesShow As Integer = 8 'make number of values to display editable

  Private Sub PopulateTree()
    Dim lAttributeName As String
    Dim lAttributeValue As String
    Dim lNumValues As Integer
    Dim lNumValuesNow As Integer
    Dim lValueStart As Integer
    Dim lDateString(3) As String

    If Not atrMain Is Nothing Then
      Me.Controls.Remove(atrMain)
    End If

    atrMain = New TreeView
    With atrMain
      .Visible = False
      '.Font = System.Drawing.Font.  try for courier or other not proportional font here
      .Location = New System.Drawing.Point(0, 0)
      .Name = "atrMain"
      .Size = Me.ClientSize
      .TabIndex = 14
      .Anchor = AnchorStyles.Top _
             Or AnchorStyles.Bottom _
             Or AnchorStyles.Left _
             Or AnchorStyles.Right
      Me.Controls.Add(atrMain)
      .Refresh()
      For Each lData As atcTimeseries In pDataGroup
        lData.Attributes.CalculateAll() 'be sure to get everything
        Dim lNode As New TreeNode
        lNode = .Nodes.Add(lData.ToString)

        Dim lAttributeNode As TreeNode = lNode.Nodes.Add("Attributes")
        Dim lComputedNode As TreeNode = lNode.Nodes.Add("Computed")
        Dim lAttributes As SortedList = lData.Attributes.ValuesSortedByName
        For i As Integer = 0 To lAttributes.Count - 1
          lAttributeName = lAttributes.GetKey(i)
          lAttributeValue = lData.Attributes.GetFormattedValue(lAttributeName)
          If lData.Attributes.GetDefinition(lAttributeName).Calculated Then
            lComputedNode.Nodes.Add(lAttributeName & " : " & lAttributeValue)
          Else
            lAttributeNode.Nodes.Add(lAttributeName & " : " & lAttributeValue)
          End If
        Next
        lAttributeNode.ExpandAll()
        lAttributeNode.EnsureVisible()
        lComputedNode.ExpandAll()

        Dim lInternalNode As New TreeNode
        lInternalNode = lNode.Nodes.Add("Internal")
        lNumValues = lData.numValues
        lInternalNode.Nodes.Add("NumValues : " & lNumValues)
        lInternalNode.ExpandAll()
        lInternalNode.EnsureVisible()

        Dim lDataNode As New TreeNode
        lDataNode = lNode.Nodes.Add("Data")
        If lNumValues > pNumValuesShow Then
          lNumValuesNow = pNumValuesShow
        Else
          lNumValuesNow = lNumValues + 1
        End If
        For j As Integer = 1 To lNumValuesNow - 1
          'data starts at 1, date display is from prev value which is start of interval
          lDateString = DumpDate(lData.Dates.Value(j - 1)).Split(" ")
          lDataNode.Nodes.Add(lDateString(2) & " " & lDateString(3) & " : " & _
                              Format(lData.Value(j), "#,##0.#####") & " : " & _
                              lDateString(0))
        Next
        If lNumValues > pNumValuesShow Then  'some from end too
          If lNumValues - pNumValuesShow > pNumValuesShow Then
            lDataNode.Nodes.Add("  <" & lNumValues - (2 * pNumValuesShow) & " values skipped>")
            lValueStart = lNumValues - pNumValuesShow
          Else
            lValueStart = lNumValuesNow
          End If
          For j As Integer = lValueStart To lData.numValues
            lDateString = DumpDate(lData.Dates.Value(j - 1)).Split(" ")
            lDataNode.Nodes.Add(lDateString(2) & " " & lDateString(3) & " : " & _
                                Format(lData.Value(j), "#,##0.#####") & " : " & _
                                lDateString(0))
          Next
        End If
      Next
      .Visible = True
    End With
  End Sub

  Public Sub Save(ByVal aFileName As String)
    Dim lFileName As String = aFileName

    If Len(lFileName) = 0 Then 'prompt user
      Dim cdlg As New Windows.Forms.SaveFileDialog
      With cdlg
        .Title = "Select File to Save Into"
        .Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*"
        .FilterIndex = 1
        .DefaultExt = "txt"
        If .ShowDialog() = Windows.Forms.DialogResult.OK Then
          lFileName = AbsolutePath(.FileName, CurDir)
        Else 'Return empty string if user clicked Cancel
          lFileName = ""
        End If
      End With
    End If

    If Len(lFileName) > 0 Then
      SaveFileString(lFileName, TreeAsString)
    End If
  End Sub

  Private Function TreeAsString() As String
    Dim s As String
    Dim t As String
    Dim ta(3) As String
    For i As Integer = 0 To atrMain.GetNodeCount(False) - 1
      With atrMain.Nodes(i)
        s &= .Text
        If Not .IsExpanded And .GetNodeCount(False) > 0 Then
          s &= " ..." & vbCrLf
        Else
          s &= vbCrLf
          For j As Integer = 0 To .GetNodeCount(False) - 1
            With .Nodes(j)
              s &= vbTab & .Text
              If Not .IsExpanded And .GetNodeCount(False) > 0 Then
                s &= " ..." & vbCrLf
              Else
                s &= vbCrLf
                For k As Integer = 0 To .GetNodeCount(False) - 1
                  t = .Nodes(k).Text.Replace(" : ", vbTab)
                  If t.IndexOf(vbTab) > 0 Then
                    ta = t.Split(vbTab)
                    s &= vbTab & vbTab & ta(0).PadRight(24) & vbTab & ta(1).PadRight(16)
                    If UBound(ta) > 1 Then s &= vbTab & ta(2)
                    s &= vbCrLf
                  Else
                    s &= vbTab & vbTab & .Nodes(k).Text & vbCrLf
                  End If
                Next
              End If
            End With
          Next
        End If
      End With
    Next
    Return s
  End Function

  Public Sub TreeAction(ByVal ParamArray aAction() As String)
    For Each lAction As String In aAction
      Dim lCurAction() As String = lAction.Split(" ")
      Select Case lCurAction(0)
        Case "Expand"
          With atrMain
            .Visible = False
            .ExpandAll()
            .Visible = True
          End With
        Case "Collapse"
          With atrMain
            .Visible = False
            .CollapseAll()
            .Visible = True
          End With
        Case "Default" : PopulateTree()
        Case "Display"
          If IsNumeric(lCurAction(1)) Then
            pNumValuesShow = lCurAction(1)
            PopulateTree()
          End If
      End Select
    Next
  End Sub

  Private Function GetIndex(ByVal aName As String) As Integer
    Return CInt(Mid(aName, InStr(aName, "#") + 1))
  End Function

  Private Sub mnuAnalysis_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuAnalysis.Click
    Dim newDisplay As atcDataDisplay
    Dim DisplayPlugins As ICollection = pDataManager.GetPlugins(GetType(atcDataDisplay))

    For Each ldisp As atcDataDisplay In DisplayPlugins
      If ldisp.Name = sender.Text Then
        Dim typ As System.Type = ldisp.GetType()
        Dim asm As System.Reflection.Assembly = System.Reflection.Assembly.GetAssembly(typ)
        newDisplay = asm.CreateInstance(typ.FullName)
        newDisplay.Show(pDataManager, pDataGroup)
        Exit Sub
      End If
    Next
  End Sub

  Private Sub mnuFileAdd_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuFileAdd.Click
    pDataManager.UserSelectData(, pDataGroup)
  End Sub

  Private Sub pDataGroup_Added(ByVal aAdded As atcCollection) Handles pDataGroup.Added
    PopulateTree()
    'TODO: could efficiently insert newly added item(s)
  End Sub

  Private Sub pDataGroup_Removed(ByVal aRemoved As atcCollection) Handles pDataGroup.Removed
    PopulateTree()
    'TODO: could efficiently remove by serial number
  End Sub

  Private Sub mnuFileSave_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuFileSave.Click
    Save("")
  End Sub

  Private Sub mnuCollapse_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuCollapse.Click
    TreeAction("Collapse")
  End Sub

  Private Sub mnuExpand_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuExpand.Click
    TreeAction("Expand")
  End Sub

  Private Sub mnuDefault_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles mnuDefault.Click
    TreeAction("Default")
  End Sub

  Private Sub mnuCopyClipboard_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles mnuCopyClipboard.Click
    Clipboard.SetDataObject(TreeAsString)
  End Sub
End Class