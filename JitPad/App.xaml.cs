using System;
using System.IO;
using System.Runtime;
using System.Windows;
using System.Windows.Threading;
using System.Xml;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using JitPad.Core;

namespace JitPad
{
    public partial class App
    {
        [STAThread]
        public static void Main()
        {
            var configDir = Path.GetDirectoryName(Config.DefaultFilePath) ?? throw new NullReferenceException();

            Directory.CreateDirectory(configDir);
            ProfileOptimization.SetProfileRoot(configDir);
            ProfileOptimization.StartProfile("Startup.Profile");

            var app = new App();
            app.InitializeComponent();
            app.Run();
        }

        private Config? _config;
        private AppCore? _appCore;

        public static Dispatcher UiDispatcher
        {
            get => _uiDispatcher ?? throw new NullReferenceException();
            private set => _uiDispatcher = value;
        }
        private static Dispatcher? _uiDispatcher;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Reactive.Bindings.UIDispatcherScheduler.Initialize();

            SetupTextEditor();
            
            _config = Config.Load();
            _appCore = new AppCore(_config);

            MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(_appCore, _config)
            };

            MainWindow.Show();

            UiDispatcher = MainWindow?.Dispatcher ?? throw new NullReferenceException();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            (MainWindow?.DataContext as IDisposable)?.Dispose();
            _appCore?.Dispose();
            _config?.Save();
        }

        private static void SetupTextEditor()
        {
            var asm = typeof(App).Assembly;
            
            {
                using var stream = asm.GetManifestResourceStream("JitPad.Resources.Xshd.CSharp-Mode.xshd") ?? throw new NullReferenceException();
                using var reader = new XmlTextReader(stream);
                
                var highlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
                HighlightingManager.Instance.RegisterHighlighting("C#", new[] {".cs"}, highlighting);
            }

            {
                using var stream = asm.GetManifestResourceStream("JitPad.Resources.Xshd.Asm-Mode.xshd") ?? throw new NullReferenceException();
                using var reader = new XmlTextReader(stream);
                
                var highlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
                HighlightingManager.Instance.RegisterHighlighting("Asm", new[] {".asm"}, highlighting);
            }
            
            {
                using var stream = asm.GetManifestResourceStream("JitPad.Resources.Xshd.BuildMessage-Mode.xshd") ?? throw new NullReferenceException();
                using var reader = new XmlTextReader(stream);
                
                var highlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
                HighlightingManager.Instance.RegisterHighlighting("BUILD_MSG", new[] {".BUILD_MSG"}, highlighting);
            }
        }
    }
}