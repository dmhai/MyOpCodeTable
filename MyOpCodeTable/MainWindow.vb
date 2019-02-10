Imports System.ComponentModel
Imports System.IO
Imports System.Reflection.Emit
Imports System.Threading.Tasks
Imports Newtonsoft.Json
Imports Telerik.WinControls


Public Class MainWindow

    Private CachePath As String = Path.GetDirectoryName(Application.ExecutablePath) & "\ListofOpCodes.json"
    Private DictofOpCodes As New Dictionary(Of OpCode, String)
    Private ListofOpCodes As New List(Of OpCodesList.MyOpcode)
    Public Sub New()
        Dim worker As BackgroundWorker = New BackgroundWorker()
        AddHandler worker.DoWork, AddressOf LoadThemeComponents
        worker.RunWorkerAsync()
        InitializeComponent()
        AddHandler RadGridView.CreateRow, AddressOf radGridView_CreateRow
    End Sub
    Private Sub LoadThemeComponents()
        Dim FluentDarkTheme As New Themes.FluentDarkTheme
        Dim FluentTheme As New Themes.FluentTheme
        ThemeResolutionService.ApplicationThemeName = "FluentDark"
    End Sub
    Private Sub MainWindow_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Cursor = Cursors.WaitCursor
        Icon = My.Resources.My_Icons
        If Not File.Exists(CachePath) Then
            DictofOpCodes = OpCodesList.GetAll
            LoadtoDataGrid(0)
        Else
            ListofOpCodes = (Function() JsonConvert.DeserializeObject(Of List(Of OpCodesList.MyOpcode))(File.ReadAllText(CachePath))).Invoke
            LoadtoDataGrid(1)
            RadGridView.Columns("Name").Sort(UI.RadSortOrder.Ascending, False)
        End If
        Cursor = Cursors.Default
        RadGridView.Rows(0).IsCurrent = True
    End Sub
    Private Sub LoadtoDataGrid(input As Integer)
        If input = 0 Then
            RadGridView.BeginUpdate()
            For Each item In DictofOpCodes
                RadGridView.Rows.Add(item.Key.Name.ToString, Conversion.Hex(item.Key.Value), item.Key.Size.ToString, item.Value,
                                      item.Key.OpCodeType.ToString, item.Key.FlowControl.ToString, item.Key.OperandType.ToString, item.Key.StackBehaviourPush.ToString, item.Key.StackBehaviourPop.ToString)
            Next
            RadGridView.EndUpdate()
        ElseIf input = 1 Then
            RadGridView.BeginUpdate()
            For Each item As OpCodesList.MyOpcode In ListofOpCodes
                RadGridView.Rows.Add(item.Name, item.Value, item.Size, item.Info, item.OpCodeType, item.FlowControl, item.OperandType, item.StackBehaviourPush, item.StackBehaviourPop)
            Next
            RadGridView.EndUpdate()
        End If
        For Each cell In RadGridView.Rows
            Dim rowi As UI.GridViewRowInfo = RadGridView.Rows(cell.Index)
            Parallel.For(0, 8, Sub(i)
                                   SyncLock cell
                                       If i <> 3 Then
                                           rowi.Cells(i).ReadOnly = True
                                       End If
                                   End SyncLock
                               End Sub)
        Next
    End Sub
    Private Overloads Sub radGridView_CreateRow(ByVal sender As Object, ByVal e As Telerik.WinControls.UI.GridViewCreateRowEventArgs)
        e.RowInfo.MinHeight = 28
    End Sub
    Private Sub saveToolStripMenuItem_Click(ByVal sender As Object, ByVal e As EventArgs)
        Cursor = Cursors.WaitCursor
        ListofOpCodes.Clear()
        Clear(CachePath)
        Parallel.ForEach(RadGridView.Rows, Sub(cell)
                                               SyncLock ListofOpCodes
                                                   Dim rowi As UI.GridViewRowInfo = RadGridView.Rows(cell.Index)
                                                   Dim temp As New OpCodesList.MyOpcode With {.Name = rowi.Cells(0).Value.ToString(), .Value = rowi.Cells(1).Value.ToString().ToString,
                                                                                       .Size = rowi.Cells(2).Value.ToString(), .Info = rowi.Cells(3).Value.ToString(), .OpCodeType = rowi.Cells(4).Value.ToString(),
                                                                                       .FlowControl = rowi.Cells(5).Value.ToString(), .OperandType = rowi.Cells(6).Value.ToString(), .StackBehaviourPush = rowi.Cells(7).Value.ToString(),
                                                                                       .StackBehaviourPop = rowi.Cells(8).Value.ToString()}
                                                   ListofOpCodes.Add(temp)
                                               End SyncLock
                                           End Sub)
        Parallel.Invoke(Sub() File.WriteAllText(CachePath, JsonConvert.SerializeObject(ListofOpCodes, Formatting.Indented)))
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
        DictofOpCodes.Clear()
        DictofOpCodes = OpCodesList.GetAll
        Clear(CachePath)
        RadGridView.Rows.Clear()
        LoadtoDataGrid(0)
        Cursor = Cursors.Default
    End Sub
    Private Sub RadGridView_ContextMenuOpening(ByVal sender As Object, ByVal e As Telerik.WinControls.UI.ContextMenuOpeningEventArgs) Handles RadGridView.ContextMenuOpening

        Dim LoadDefaultsMenuItem As New UI.RadMenuItem("Load Defaults")
        AddHandler LoadDefaultsMenuItem.Click, AddressOf LoadDefaultsMenuItem_Click
        e.ContextMenu.Items.Add(LoadDefaultsMenuItem)

        Dim SaveAllMenuItem As New UI.RadMenuItem("Save All")
        AddHandler SaveAllMenuItem.Click, AddressOf saveToolStripMenuItem_Click
        e.ContextMenu.Items.Add(SaveAllMenuItem)
        e.ContextMenu.Items.Add(New UI.RadMenuSeparatorItem())

        If ThemeResolutionService.ApplicationThemeName.Contains("FluentDark") Then
            Dim LightMenuItem As New UI.RadMenuItem("Light Theme")
            e.ContextMenu.Items.Add(LightMenuItem)
            AddHandler LightMenuItem.Click, AddressOf LightToolStripMenuItem_Click
        Else
            Dim DarkMenuItem As New UI.RadMenuItem("Dark Theme")
            e.ContextMenu.Items.Add(DarkMenuItem)
            AddHandler DarkMenuItem.Click, AddressOf DarkToolStripMenuItem_Click
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
