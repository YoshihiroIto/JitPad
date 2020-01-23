using System.Collections.Generic;
using System.Linq;
using System.Windows;

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
            {
                ((MainWindowViewModel) DataContext).MonitoringFilePath.Value = files.FirstOrDefault() ?? "";
                ((MainWindowViewModel) DataContext).ReloadMonitoringFile();
            }
        }

        private void Window_PreviewDragOver(object sender, DragEventArgs e)
        {
            e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop, true)
                ? DragDropEffects.Copy
                : DragDropEffects.None;
            e.Handled = true;
        }
    }
}