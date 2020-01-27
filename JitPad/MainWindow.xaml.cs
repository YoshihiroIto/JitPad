using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace JitPad
{
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetData(DataFormats.FileDrop) is IEnumerable<string> files)
                ViewModel.MonitoringFilePath.Value = files.FirstOrDefault() ?? "";
        }

        private void Window_PreviewDragOver(object sender, DragEventArgs e)
        {
            e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop, true)
                ? DragDropEffects.Copy
                : DragDropEffects.None;
            e.Handled = true;
        }

        private MainWindowViewModel ViewModel => (MainWindowViewModel) DataContext;

        private void MainWindow_OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is MainWindowViewModel oldVm)
                oldVm.CsFileOpen = null;
            
            if (e.NewValue is MainWindowViewModel newVm)
                newVm.CsFileOpen = CsFileOpen;
        }

        private static string? CsFileOpen()
        {
            using var dialog = new CommonOpenFileDialog();

            var filter = new CommonFileDialogFilter {DisplayName = "C# file"};
            filter.Extensions.Add("cs");

            dialog.Filters.Add(filter);

            var window = Application.Current?.MainWindow;
            if (window == null)
                return null;

            return dialog.ShowDialog(window) == CommonFileDialogResult.Ok
                ? dialog.FileNames.FirstOrDefault()
                : null;
        }
    }
}