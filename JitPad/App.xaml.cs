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
        private Core.AppContext? _appContext;

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
            _appContext = new Core.AppContext(_config);

            MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(_appContext, _config)
            };

            MainWindow.Show();

            UiDispatcher = MainWindow?.Dispatcher ?? throw new NullReferenceException();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            (MainWindow?.DataContext as IDisposable)?.Dispose();
            _appContext?.Dispose();
            _config?.Save();
        }

        private static void SetupTextEditor()
        {
            {
                using var reader = new XmlTextReader(new MemoryStream(JitPad.Properties.Resources.CSharp_Mode));
                var highlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
                HighlightingManager.Instance.RegisterHighlighting("C#", new[] {".cs"}, highlighting);
            }

            {
                using var reader = new XmlTextReader(new MemoryStream(JitPad.Properties.Resources.Asm_Mode));
                var highlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
                HighlightingManager.Instance.RegisterHighlighting("Asm", new[] {".asm"}, highlighting);
            }
            
            {
                using var reader = new XmlTextReader(new MemoryStream(JitPad.Properties.Resources.BuildMessage_Mode));
                var highlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
                HighlightingManager.Instance.RegisterHighlighting("BUILD_MSG", new[] {".BUILD_MSG"}, highlighting);
            }
        }
    }
}