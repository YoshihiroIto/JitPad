using System;
using System.IO;
using System.Reactive.Disposables;
using JitPad.Foundation;
using Reactive.Bindings.Extensions;
using System.Diagnostics;
using JitPad.Core.Processor;

namespace JitPad.Core
{
    public class AppCore : NotificationObject, IDisposable
    {
        public BuildingUnit BuildingUnit { get; }

        private readonly Config _config;
        private readonly CompositeDisposable _Trashes = new CompositeDisposable();

        public AppCore(Config config)
        {
            _config = config;

            var initialSourceCode = "";
            {
                try
                {
                    initialSourceCode = _config.LoadCodeTemplate();
                }
                catch
                {
                    _config.IsFileMonitoring = false;
                    _config.MonitoringFilePath = "";
                }
            }

            var compiler = new Compiler();
            var disassembler = new JitDisassembler("JitDasm/JitDasm.exe");
            BuildingUnit = new BuildingUnit(_config, compiler, disassembler) {SourceCode = initialSourceCode}
                .AddTo(_Trashes);

            var fileMonitor = new FileMonitor(_config).AddTo(_Trashes);
            fileMonitor.MonitoringFileChanged += (_, __) => LoadMonitoringFile();

            _config.ObserveProperty(x => x.MonitoringFilePath)
                .Subscribe(_ => LoadMonitoringFile())
                .AddTo(_Trashes);
        }

        public void Dispose()
        {
            _Trashes.Dispose();
        }

        public void OpenConfigFolder()
        {
            var dir = Path.GetDirectoryName(_config.FilePath);

            Process.Start("explorer", $"\"{dir}\"");
        }

        public void OpenAboutJitPad()
        {
            const string url = "https://github.com/YoshihiroIto/JitPad";

            Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") {CreateNoWindow = true});
        }

        public void ApplyTemplateFile()
        {
            _config.MonitoringFilePath = "";
            BuildingUnit.SourceCode = _config.LoadCodeTemplate();
        }

        public void LoadMonitoringFile()
        {
            if (File.Exists(_config.MonitoringFilePath))
                BuildingUnit.SourceCode = File.ReadAllText(_config.MonitoringFilePath);
            else
                ApplyTemplateFile();
        }
    }
}