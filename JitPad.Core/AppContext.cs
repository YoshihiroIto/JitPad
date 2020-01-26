using System;
using System.IO;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using JitPad.Foundation;
using Reactive.Bindings.Extensions;

namespace JitPad.Core
{
    public class AppContext : NotificationObject, IDisposable
    {
        #region IsReleaseBuild

        private bool _IsReleaseBuild = true;

        public bool IsReleaseBuild
        {
            get => _IsReleaseBuild;
            set
            {
                if (SetProperty(ref _IsReleaseBuild, value))
                    ProcessingUnit.IsReleaseBuild = value;
            }
        }

        #endregion
        
        #region IsTieredJit

        private bool _IsTieredJit;

        public bool IsTieredJit
        {
            get => _IsTieredJit;
            set
            {
                if (SetProperty(ref _IsTieredJit, value))
                    ProcessingUnit.IsTieredJit = value;
            }
        }

        #endregion

        #region IsFileMonitoring

        private bool _IsFileMonitoring;

        public bool IsFileMonitoring
        {
            get => _IsFileMonitoring;
            set => SetProperty(ref _IsFileMonitoring, value);
        }

        #endregion

        #region MonitoringFilePath

        private string _MonitoringFilePath = "";

        public string MonitoringFilePath
        {
            get => _MonitoringFilePath;
            set => SetProperty(ref _MonitoringFilePath, value);
        }

        #endregion

        public ProcessingUnit ProcessingUnit { get; }

        private readonly CompositeDisposable _Trashes = new CompositeDisposable();

        public AppContext()
        {
            ProcessingUnit = new ProcessingUnit
            {
                IsReleaseBuild = IsReleaseBuild,
                IsTieredJit = IsTieredJit
            };

            SetupFileMonitoring();
        }

        public void Dispose()
        {
            ReleaseFileMonitor();

            _Trashes.Dispose();
            ProcessingUnit.Dispose();
        }
        
        #region file monitoring

        private FileMonitor? _fileMonitor;
        private IDisposable? _fileMonitorChanged;
        
        public void ReloadMonitoringFile()
        {
            LoadMonitoringFile();
        }

        private void SetupFileMonitoring()
        {
            Observable
                .Merge(this.ObserveProperty(x => IsFileMonitoring).ToUnit())
                .Merge(this.ObserveProperty(x => MonitoringFilePath).ToUnit())
                .Subscribe(_ => UpdateFileMonitoring())
                .AddTo(_Trashes);
        }

        private void UpdateFileMonitoring()
        {
            ReleaseFileMonitor();

            if (IsFileMonitoring && File.Exists(MonitoringFilePath))
            {
                _fileMonitor = new FileMonitor(MonitoringFilePath);
                _fileMonitorChanged = _fileMonitor.Changed
                    .Subscribe(x => LoadMonitoringFile());
            }
        }

        private void LoadMonitoringFile()
        {
            ProcessingUnit.SourceText =
                File.Exists(MonitoringFilePath)
                    ? File.ReadAllText(MonitoringFilePath)
                    : "";
        }

        private void ReleaseFileMonitor()
        {
            _fileMonitorChanged?.Dispose();
            _fileMonitor?.Dispose();

            _fileMonitor = null;
            _fileMonitorChanged = null;
        }

        #endregion
    }
}