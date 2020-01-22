using System;
using System.IO;
using System.Runtime;
using System.Windows;

namespace JitPad
{
    public partial class App
    {
        [STAThread]
        public static void Main()
        {
            var configDir =
                Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "Jewelry Development/JitPad/"
                );

            Directory.CreateDirectory(configDir);
            ProfileOptimization.SetProfileRoot(configDir);
            ProfileOptimization.StartProfile("Startup.Profile");

            var app = new App();
            app.InitializeComponent();
            app.Run();
        }

        private readonly JitPad.Core.AppContext _appContext = new JitPad.Core.AppContext();

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Reactive.Bindings.UIDispatcherScheduler.Initialize();

            MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(_appContext)
            };

            MainWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            (MainWindow?.DataContext as IDisposable)?.Dispose();
            _appContext?.Dispose();
        }
    }
}