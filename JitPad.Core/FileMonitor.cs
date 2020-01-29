using System;
using System.IO;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using JitPad.Foundation;
using Reactive.Bindings.Extensions;

namespace JitPad.Core
{
    public class FileMonitor : IDisposable
    {
        public event EventHandler MonitoringFileChanged;
        
        private ObservableFileSystem? _monitoringFileObservable;
        private IDisposable? _fileMonitorChanged;

        private readonly Config _config;
        private readonly CompositeDisposable _Trashes = new CompositeDisposable();

        public FileMonitor(Config config)
        {
            _config = config;

            Observable
                .Merge(_config.ObserveProperty(x => x.IsFileMonitoring).ToUnit())
                .Merge(_config.ObserveProperty(x => x.MonitoringFilePath).ToUnit())
                .Subscribe(_ => UpdateFileMonitoring())
                .AddTo(_Trashes);
        }

        private void UpdateFileMonitoring()
        {
            ReleaseFileMonitor();

            if (_config.IsFileMonitoring && File.Exists(_config.MonitoringFilePath))
            {
                _monitoringFileObservable = new ObservableFileSystem(_config.MonitoringFilePath);
                _fileMonitorChanged = _monitoringFileObservable.Changed
                    .Subscribe(x => MonitoringFileChanged?.Invoke(this, EventArgs.Empty));
            }
        }

        private void ReleaseFileMonitor()
        {
            _fileMonitorChanged?.Dispose();
            _monitoringFileObservable?.Dispose();

            _fileMonitorChanged = null;
            _monitoringFileObservable = null;
        }

        public void Dispose()
        {
            ReleaseFileMonitor();
            _Trashes.Dispose();
        }
    }
}