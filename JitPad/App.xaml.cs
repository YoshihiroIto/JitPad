using System;
using System.Windows;
using System.Windows.Threading;

namespace JitPad
{
    public partial class App
    {
        [STAThread]
        public static void Main()
        {
            App app = new App();
            app.InitializeComponent();
            app.Run();
        }
        
        public App()
        {
            InitializeComponent();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Reactive.Bindings.UIDispatcherScheduler.Initialize();

            MainWindow = new MainWindow();
            MainWindow?.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
        }
    }
}