using System.Windows.Input;

namespace JitPad
{
    public partial class AboutDialog
    {
        public AboutDialog()
        {
            InitializeComponent();
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                Close();
        }
    }
}