using System;
using System.Linq;
using JitPad.Core;
using JitPad.Core.Interface;
using JitPad.Foundation;
using Livet.Messaging;
using Livet.Messaging.IO;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace JitPad
{
    public class MainWindowViewModel : ViewModelBase
    {
        public ReactiveProperty<bool> IeReleaseBuild { get; }
        public ReactiveProperty<bool> IsFileMonitoring { get; }
        public ReactiveProperty<string> MonitoringFilePath { get; }
        public ReadOnlyReactiveProperty<bool> IsInBuilding { get; }

        public ReactiveProperty<bool> IsTieredCompilation { get; }
        public ReactiveProperty<bool> IsTcQuickJit { get; }
        public ReactiveProperty<bool> IsTcQuickJitForLoops { get; }

        public ReactiveProperty<string> SourceCode { get; }
        public ReadOnlyReactiveProperty<string?> BuildResult { get; }
        public ReadOnlyReactiveProperty<string?> BuildMessage { get; }
        public ReadOnlyReactiveProperty<CompileResult.Message[]?> BuildDetailMessage { get; }
        public ReadOnlyReactiveProperty<bool> IsBuildOk { get; }

        public ReactiveCommand<OpeningFileSelectionMessage> OpenMonitoringFileCommand { get; }
        public ReactiveCommand ApplyTemplateFileCommand { get; }
        public ReactiveCommand OpenConfigFolderCommand { get; }
        public AsyncReactiveCommand OpenAboutDialogCommand { get; }

        public InteractionMessenger Messenger { get; } = new InteractionMessenger();

        // for TextEditorCompletionBehavior
        public ICompiler Compiler { get; }

        public MainWindowViewModel(AppCore appCore, Config config)
        {
            Compiler = appCore.Compiler;
            
            IeReleaseBuild = config.ToReactivePropertyAsSynchronized(x => x.IsReleaseBuild).AddTo(Trashes);

            IsFileMonitoring = config.ToReactivePropertyAsSynchronized(x => x.IsFileMonitoring).AddTo(Trashes);
            MonitoringFilePath = config.ToReactivePropertyAsSynchronized(x => x.MonitoringFilePath).AddTo(Trashes);
            IsInBuilding = appCore.BuildingUnit.ObserveProperty(x => x.IsInBuilding).ToReadOnlyReactiveProperty().AddTo(Trashes);

            IsTieredCompilation = config.ToReactivePropertyAsSynchronized(
                x => x.JitFlags,
                x => x.HasFlag(JitFlags.TieredCompilation),
                x => x
                    ? config.JitFlags | JitFlags.TieredCompilation
                    : config.JitFlags & ~JitFlags.TieredCompilation).AddTo(Trashes);
            
            IsTcQuickJit = config.ToReactivePropertyAsSynchronized(
                x => x.JitFlags,
                x => x.HasFlag(JitFlags.TC_QuickJit),
                x => x
                    ? config.JitFlags | JitFlags.TC_QuickJit
                    : config.JitFlags & ~JitFlags.TC_QuickJit).AddTo(Trashes);

            IsTcQuickJitForLoops = config.ToReactivePropertyAsSynchronized(
                x => x.JitFlags,
                x => x.HasFlag(JitFlags.TC_QuickJitForLoops),
                x => x
                    ? config.JitFlags | JitFlags.TC_QuickJitForLoops
                    : config.JitFlags & ~JitFlags.TC_QuickJitForLoops).AddTo(Trashes);

            SourceCode = appCore.BuildingUnit.ToReactivePropertyAsSynchronized(x => x.SourceCode).AddTo(Trashes);
            BuildResult = appCore.BuildingUnit.ObserveProperty(x => x.BuildResult).ToReadOnlyReactiveProperty().AddTo(Trashes);
            BuildMessage = appCore.BuildingUnit.ObserveProperty(x => x.BuildMessage).ToReadOnlyReactiveProperty().AddTo(Trashes);
            BuildDetailMessage = appCore.BuildingUnit.ObserveProperty(x => x.BuildDetailMessages).ToReadOnlyReactiveProperty().AddTo(Trashes);
            IsBuildOk = appCore.BuildingUnit.ObserveProperty(x => x.IsBuildOk).ToReadOnlyReactiveProperty().AddTo(Trashes);

            OpenMonitoringFileCommand = new ReactiveCommand<OpeningFileSelectionMessage>().AddTo(Trashes);
            OpenMonitoringFileCommand.Subscribe(x =>
            {
                var selectedFile = x?.Response?.FirstOrDefault();
                if (string.IsNullOrEmpty(selectedFile) == false)
                    config.MonitoringFilePath = selectedFile!;
            }).AddTo(Trashes);

            ApplyTemplateFileCommand = new ReactiveCommand().AddTo(Trashes);
            ApplyTemplateFileCommand.Subscribe(_ => appCore.ApplyTemplateFile()).AddTo(Trashes);

            OpenConfigFolderCommand = new ReactiveCommand().AddTo(Trashes);
            OpenConfigFolderCommand.Subscribe(_ => appCore.OpenConfigFolder()).AddTo(Trashes);

            OpenAboutDialogCommand = new AsyncReactiveCommand().AddTo(Trashes);
            OpenAboutDialogCommand.Subscribe(async _ =>
            {
                using var aboutDialogViewModel = new AboutDialogViewModel(appCore);

                // ReSharper disable once AsyncConverter.AsyncAwaitMayBeElidedHighlighting
                await Messenger.RaiseAsync(new TransitionMessage(aboutDialogViewModel, "OpenAboutDialog")).ConfigureAwait(false);
            }).AddTo(Trashes);
        }
    }
}