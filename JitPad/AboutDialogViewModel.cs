using System;
using JitPad.Core;
using JitPad.Foundation;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace JitPad
{
    public class AboutDialogViewModel : ViewModelBase
    {
        public OpenSourceMetadata[] OpenSources { get; }
        
        public string Version { get; }
        
        public ReactiveCommand<OpenSourceMetadata> OpenJitPadWebSiteCommand { get; }
        public ReactiveCommand<OpenSourceMetadata> OpenOpenSourceCommand { get; }

        public AboutDialogViewModel(AppCore appCore)
        {
            OpenSources = appCore.OpenSources;
            Version = appCore.Version;
            
            OpenJitPadWebSiteCommand = new ReactiveCommand<OpenSourceMetadata>().AddTo(Trashes);
            OpenJitPadWebSiteCommand.Subscribe(_ => appCore.OpenJitPadWebSite())
                .AddTo(Trashes);
               
            OpenOpenSourceCommand = new ReactiveCommand<OpenSourceMetadata>().AddTo(Trashes);
            OpenOpenSourceCommand.Subscribe(x => appCore.OpenWeb(x.Url!))
                .AddTo(Trashes);
        }
    }
}