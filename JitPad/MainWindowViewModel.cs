using System;
using JitPad.Core;
using JitPad.Foundation;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using AppContext = JitPad.Core.AppContext;

namespace JitPad
{
    public class MainWindowViewModel : ViewModelBase
    {
        public ReactiveProperty<bool> IeReleaseBuild { get; }
        public ReactiveProperty<bool> IsTieredJit { get; }
        public ReactiveProperty<bool> IsFileMonitoring { get; }
        public ReactiveProperty<string> MonitoringFilePath { get; }
        public ReadOnlyReactiveProperty<bool> IsInProcessing { get; }

        public ReactiveProperty<string> SourceCode { get; }
        public ReadOnlyReactiveProperty<string> BuildResult { get; }
        public ReadOnlyReactiveProperty<string> BuildMessage { get; }
        public ReadOnlyReactiveProperty<bool> IsBuildOk { get; }

        public ReactiveCommand OpenMonitoringFileCommand { get; }
        
        public Func<string?>? CsFileOpen { get; set; }
        
        private readonly AppContext _appContext;

        public MainWindowViewModel(AppContext appContext, Config config)
        {
            _appContext = appContext;

            IeReleaseBuild = config.ToReactivePropertyAsSynchronized(x => x.IsReleaseBuild).AddTo(Trashes);
            IsTieredJit = config.ToReactivePropertyAsSynchronized(x => x.IsTieredJit).AddTo(Trashes);
            IsFileMonitoring = config.ToReactivePropertyAsSynchronized(x => x.IsFileMonitoring).AddTo(Trashes);
            MonitoringFilePath = config.ToReactivePropertyAsSynchronized(x => x.MonitoringFilePath).AddTo(Trashes);
            IsInProcessing = appContext.ProcessingUnit.ObserveProperty(x => x.IsInProcessing).ToReadOnlyReactiveProperty().AddTo(Trashes);

            SourceCode = appContext.ProcessingUnit.ToReactivePropertyAsSynchronized(x => x.SourceCode).AddTo(Trashes);
            BuildResult = appContext.ProcessingUnit.ObserveProperty(x => x.BuildResult).ToReadOnlyReactiveProperty().AddTo(Trashes);
            BuildMessage = appContext.ProcessingUnit.ObserveProperty(x => x.BuildMessage).ToReadOnlyReactiveProperty().AddTo(Trashes);
            IsBuildOk = appContext.ProcessingUnit.ObserveProperty(x => x.IsBuildOk).ToReadOnlyReactiveProperty().AddTo(Trashes);

            OpenMonitoringFileCommand = new ReactiveCommand().AddTo(Trashes);
            OpenMonitoringFileCommand.Subscribe(_ =>
            {
                var selectedFile = CsFileOpen?.Invoke();

                if (selectedFile != null)
                {
                    config.MonitoringFilePath = selectedFile;
                    ReloadMonitoringFile();
                }
            }).AddTo(Trashes);
        }

        public void ReloadMonitoringFile()
        {
            _appContext.ReloadMonitoringFile();
        }
    }
}