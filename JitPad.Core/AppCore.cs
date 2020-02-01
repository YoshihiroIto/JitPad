using System;
using System.IO;
using System.Reactive.Disposables;
using JitPad.Foundation;
using Reactive.Bindings.Extensions;
using System.Diagnostics;
using System.Reflection;
using JitPad.Core.Interface;
using JitPad.Core.Processor;

namespace JitPad.Core
{
    public class AppCore : NotificationObject, IDisposable
    {
        public BuildingUnit BuildingUnit { get; }

        private readonly Config _config;
        private readonly CompositeDisposable _Trashes = new CompositeDisposable();

        public OpenSourceMetadata[] OpenSources => OpenSource.OpenSources;

        public string Version => Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? throw new NullReferenceException();

        public readonly ICompiler Compiler;
        public readonly IDisassembler Disassembler;
        
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

            Compiler = new Compiler();
            Disassembler = new JitDisassembler("JitDasm/JitDasm.exe");
            
            BuildingUnit = new BuildingUnit(_config, Compiler, Disassembler) {SourceCode = initialSourceCode}
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

        public void OpenJitPadWebSite()
        {
            const string url = "https://github.com/YoshihiroIto/JitPad";
            
            OpenWeb(url);
        }
        
        public void OpenWeb(string url)
        {
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