using System.IO;
using System.Xml;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;

namespace JitPad
{
    public partial class MainWindow
    {
        public MainWindow()
        {
            using var reader = new XmlTextReader(new MemoryStream(Properties.Resources.CSharp_Mode));
            var highlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);

            HighlightingManager.Instance.RegisterHighlighting("C#", new[] {".cs"}, highlighting);

            InitializeComponent();
        }
    }
}