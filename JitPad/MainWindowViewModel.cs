using System;
using JitPad.Foundation;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using AppContext = JitPad.Core.AppContext;

namespace JitPad
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly AppContext _AppContext;
        public ReactiveProperty<bool> IeReleaseBuild { get; }
        public ReactiveProperty<bool> IsTieredJit { get; }
        public ReactiveProperty<bool> IsFileMonitoring { get; }
        public ReactiveProperty<string> MonitoringFilePath { get; }
        public ReadOnlyReactiveProperty<bool> IsInProcessing { get; }

        public ReactiveProperty<string> PrimarySourceText { get; }
        public ReadOnlyReactiveProperty<string> PrimaryResult { get; }
        public ReadOnlyReactiveProperty<string> PrimaryMessage { get; }
        public ReadOnlyReactiveProperty<bool> PrimaryIsOk { get; }

        public ReactiveCommand OpenMonitoringFileCommand { get; }
        
        public Func<string?>? CsFileOpen { get; set; }

        public MainWindowViewModel(AppContext appContext)
        {
            _AppContext = appContext;

            IeReleaseBuild = appContext.ToReactivePropertyAsSynchronized(x => x.IsReleaseBuild).AddTo(Trashes);
            IsTieredJit = appContext.ToReactivePropertyAsSynchronized(x => x.IsTieredJit).AddTo(Trashes);
            IsFileMonitoring = appContext.ToReactivePropertyAsSynchronized(x => x.IsFileMonitoring).AddTo(Trashes);
            MonitoringFilePath = appContext.ToReactivePropertyAsSynchronized(x => x.MonitoringFilePath).AddTo(Trashes);
            IsInProcessing = appContext.Primary.ObserveProperty(x => x.IsInProcessing).ToReadOnlyReactiveProperty().AddTo(Trashes);

            PrimarySourceText = appContext.Primary.ToReactivePropertyAsSynchronized(x => x.SourceText).AddTo(Trashes);
            PrimaryResult = appContext.Primary.ObserveProperty(x => x.Result).ToReadOnlyReactiveProperty().AddTo(Trashes);
            PrimaryMessage = appContext.Primary.ObserveProperty(x => x.Message).ToReadOnlyReactiveProperty().AddTo(Trashes);
            PrimaryIsOk = appContext.Primary.ObserveProperty(x => x.IsOk).ToReadOnlyReactiveProperty().AddTo(Trashes);

            OpenMonitoringFileCommand = new ReactiveCommand().AddTo(Trashes);
            OpenMonitoringFileCommand.Subscribe(_ =>
            {
                var selectedFile = CsFileOpen?.Invoke();

                if (selectedFile != null)
                {
                    _AppContext.MonitoringFilePath = selectedFile;
                    ReloadMonitoringFile();
                }
            }).AddTo(Trashes);
        }

        public void ReloadMonitoringFile()
        {
            _AppContext.ReloadMonitoringFile();
        }
    }
}