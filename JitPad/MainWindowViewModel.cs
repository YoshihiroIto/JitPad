using System;
using JitPad.Core;
using JitPad.Core.Processor;
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
        public ReadOnlyReactiveProperty<bool> IsInBuilding { get; }

        public ReactiveProperty<string> SourceCode { get; }
        public ReadOnlyReactiveProperty<string?> BuildResult { get; }
        public ReadOnlyReactiveProperty<string?> BuildMessage { get; }
        public ReadOnlyReactiveProperty<CompileResult.Message[]?> BuildDetailMessage { get; }
        public ReadOnlyReactiveProperty<bool> IsBuildOk { get; }

        public ReactiveCommand OpenMonitoringFileCommand { get; }
        public ReactiveCommand ApplyTemplateFileCommand { get; }
        public ReactiveCommand OpenConfigFolderCommand { get; }
        public ReactiveCommand OpenAboutJitPadCommand { get; }

        public Func<string?>? CsFileOpen { get; set; }

        public MainWindowViewModel(AppContext appContext, Config config)
        {
            IeReleaseBuild = config.ToReactivePropertyAsSynchronized(x => x.IsReleaseBuild).AddTo(Trashes);
            IsTieredJit = config.ToReactivePropertyAsSynchronized(x => x.IsTieredJit).AddTo(Trashes);
            IsFileMonitoring = config.ToReactivePropertyAsSynchronized(x => x.IsFileMonitoring).AddTo(Trashes);
            MonitoringFilePath = config.ToReactivePropertyAsSynchronized(x => x.MonitoringFilePath).AddTo(Trashes);
            IsInBuilding = appContext.BuildingUnit.ObserveProperty(x => x.IsInBuilding).ToReadOnlyReactiveProperty().AddTo(Trashes);

            SourceCode = appContext.BuildingUnit.ToReactivePropertyAsSynchronized(x => x.SourceCode).AddTo(Trashes);
            BuildResult = appContext.BuildingUnit.ObserveProperty(x => x.BuildResult).ToReadOnlyReactiveProperty().AddTo(Trashes);
            BuildMessage = appContext.BuildingUnit.ObserveProperty(x => x.BuildMessage).ToReadOnlyReactiveProperty().AddTo(Trashes);
            BuildDetailMessage = appContext.BuildingUnit.ObserveProperty(x => x.BuildDetailMessages).ToReadOnlyReactiveProperty().AddTo(Trashes);
            IsBuildOk = appContext.BuildingUnit.ObserveProperty(x => x.IsBuildOk).ToReadOnlyReactiveProperty().AddTo(Trashes);

            OpenMonitoringFileCommand = new ReactiveCommand().AddTo(Trashes);
            OpenMonitoringFileCommand.Subscribe(_ =>
            {
                var selectedFile = CsFileOpen?.Invoke();

                if (selectedFile != null)
                    config.MonitoringFilePath = selectedFile;
            }).AddTo(Trashes);

            ApplyTemplateFileCommand = new ReactiveCommand().AddTo(Trashes);
            ApplyTemplateFileCommand.Subscribe(_ => appContext.ApplyTemplateFile()).AddTo(Trashes);

            OpenConfigFolderCommand = new ReactiveCommand().AddTo(Trashes);
            OpenConfigFolderCommand.Subscribe(_ => appContext.OpenConfigFolder()).AddTo(Trashes);

            OpenAboutJitPadCommand = new ReactiveCommand().AddTo(Trashes);
            OpenAboutJitPadCommand.Subscribe(_ => appContext.OpenAboutJitPad()).AddTo(Trashes);
        }
    }
}