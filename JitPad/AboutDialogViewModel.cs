using System;
using JitPad.Core;
using JitPad.Foundation;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace JitPad
{
    public class AboutDialogViewModel : ViewModelBase
    {
        public OpenSourceMetadata[] OpenSources => _appCore.OpenSources;
        public string Version => _appCore.Version;
        public string Copyright => _appCore.Copyright;
        public string JitPadWebSiteUrl => _appCore.JitPadWebSiteUrl;
        
        public ReactiveCommand<OpenSourceMetadata> OpenJitPadWebSiteCommand { get; }
        public ReactiveCommand<OpenSourceMetadata> OpenOpenSourceCommand { get; }
        
        private readonly AppCore _appCore;

        public AboutDialogViewModel(AppCore appCore)
        {
            _appCore = appCore;
            
            OpenJitPadWebSiteCommand = new ReactiveCommand<OpenSourceMetadata>().AddTo(Trashes);
            OpenJitPadWebSiteCommand.Subscribe(_ => appCore.OpenJitPadWebSite())
                .AddTo(Trashes);
               
            OpenOpenSourceCommand = new ReactiveCommand<OpenSourceMetadata>().AddTo(Trashes);
            OpenOpenSourceCommand.Subscribe(x => appCore.OpenWeb(x.Url!))
                .AddTo(Trashes);
        }
    }
}