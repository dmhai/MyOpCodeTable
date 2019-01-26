Imports System.IO
Imports Newtonsoft.Json
Imports Telerik.WinControls

Public Class MainWindow

    Private CachePath As String = Path.GetDirectoryName(Application.ExecutablePath) & "\ListofOpCodes.json"
    Private ListofOpCodes As List(Of OpCodesList.MyOpcode)
    Public Sub New()
        InitializeComponent()
        AddHandler RadGridView.CreateRow, AddressOf radGridView_CreateRow
    End Sub
    Private Sub MainWindow_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Cursor = Cursors.WaitCursor
        ThemeResolutionService.ApplicationThemeName = "FluentDark"
        Icon = My.Resources.My_Icons
        Try
            ListofOpCodes = JsonConvert.DeserializeObject(Of List(Of OpCodesList.MyOpcode))(File.ReadAllText(CachePath))
        Catch ex As Exception
            ListofOpCodes = OpCodesList.GetAll
            Clear(CachePath)
            File.WriteAllText(CachePath, JsonConvert.SerializeObject(ListofOpCodes, Formatting.Indented))
        End Try
        LoadtoDataGrid()
        Cursor = Cursors.Default
    End Sub
    Private Sub LoadtoDataGrid()
        For Each item As OpCodesList.MyOpcode In ListofOpCodes
            RadGridView.Rows.Add(item.Name, item.Value, item.Size, item.Info, item.OpCodeType, item.FlowControl, item.OperandType, item.StackBehaviourPush, item.StackBehaviourPop)
        Next
        For Each cell As UI.GridViewRowInfo In RadGridView.Rows
            Dim rowi As UI.GridViewRowInfo = RadGridView.Rows(cell.Index)
            For i = 0 To 8
                If i <> 3 Then
                    rowi.Cells(i).ReadOnly = True
                End If
            Next
        Next
        RadGridView.Rows(0).IsCurrent = True
    End Sub
    Private Overloads Sub radGridView_CreateRow(ByVal sender As Object, ByVal e As Telerik.WinControls.UI.GridViewCreateRowEventArgs)
        e.RowInfo.MinHeight = 28
    End Sub
    Private Sub saveToolStripMenuItem_Click(ByVal sender As Object, ByVal e As EventArgs)
        Cursor = Cursors.WaitCursor
        ListofOpCodes.Clear()
        Clear(CachePath)
        For Each cell As UI.GridViewRowInfo In RadGridView.Rows
            Dim rowi As UI.GridViewRowInfo = RadGridView.Rows(cell.Index)
            Dim temp As New OpCodesList.MyOpcode With {.Name = rowi.Cells(0).Value.ToString(), .Value = rowi.Cells(1).Value.ToString().ToString,
            .Size = rowi.Cells(2).Value.ToString(), .Info = rowi.Cells(3).Value.ToString(), .OpCodeType = rowi.Cells(4).Value.ToString(),
            .FlowControl = rowi.Cells(5).Value.ToString(), .OperandType = rowi.Cells(6).Value.ToString(), .StackBehaviourPush = rowi.Cells(7).Value.ToString(),
            .StackBehaviourPop = rowi.Cells(8).Value.ToString()}
            ListofOpCodes.Add(temp)
            File.WriteAllText(CachePath, JsonConvert.SerializeObject(ListofOpCodes, Formatting.Indented))
        Next cell
        Cursor = Cursors.Default
    End Sub
    Private Sub LightToolStripMenuItem_Click(ByVal sender As Object, ByVal e As EventArgs)
        ThemeResolutionService.ApplicationThemeName = "Fluent"
    End Sub

    Private Sub DarkToolStripMenuItem_Click(ByVal sender As Object, ByVal e As EventArgs)
        ThemeResolutionService.ApplicationThemeName = "FluentDark"
    End Sub

    Private Sub LoadDefaultsMenuItem_Click(ByVal sender As Object, ByVal e As EventArgs)
        Cursor = Cursors.WaitCursor
        ListofOpCodes.Clear()
        ListofOpCodes = OpCodesList.GetAll
        Clear(CachePath)
        File.WriteAllText(CachePath, JsonConvert.SerializeObject(ListofOpCodes, Formatting.Indented))
        RadGridView.Rows.Clear()
        LoadtoDataGrid()
        Cursor = Cursors.Default
    End Sub
    Private Sub RadGridView_ContextMenuOpening(ByVal sender As Object, ByVal e As Telerik.WinControls.UI.ContextMenuOpeningEventArgs) Handles RadGridView.ContextMenuOpening
        e.ContextMenu.Items.Add(New UI.RadMenuSeparatorItem())
        Dim LoadDefaultsMenuItem As New UI.RadMenuItem("Load Defaults")
        AddHandler LoadDefaultsMenuItem.Click, AddressOf LoadDefaultsMenuItem_Click
        e.ContextMenu.Items.Add(LoadDefaultsMenuItem)

        Dim SaveAllMenuItem As New UI.RadMenuItem("Save All")
        AddHandler SaveAllMenuItem.Click, AddressOf saveToolStripMenuItem_Click
        e.ContextMenu.Items.Add(SaveAllMenuItem)

        Dim LightMenuItem As New UI.RadMenuItem("Light Theme")
        AddHandler LightMenuItem.Click, AddressOf LightToolStripMenuItem_Click
        Dim DarkMenuItem As New UI.RadMenuItem("Dark Theme")
        AddHandler DarkMenuItem.Click, AddressOf DarkToolStripMenuItem_Click

        e.ContextMenu.Items.Add(New UI.RadMenuSeparatorItem())
        If ThemeResolutionService.ApplicationThemeName.Contains("FluentDark") Then
            e.ContextMenu.Items.Add(LightMenuItem)
        Else
            e.ContextMenu.Items.Add(DarkMenuItem)
        End If
    End Sub
    Private Sub Clear(Path As String)
        If File.Exists(Path) Then
            Try
                File.Delete(Path)
            Catch ex As Exception
                Throw
            End Try
        End If
    End Sub
End Class
