using System;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using JitPad.Core.Processor;
using JitPad.Foundation;
using Reactive.Bindings.Extensions;

namespace JitPad.Core
{
    public class ProcessingUnit : NotificationObject, IDisposable
    {
        #region SourceCode
        
        private string _SourceText = "";
        
        public string SourceText
        {
            get => _SourceText;
            set => SetProperty(ref _SourceText, value);
        }
        
        #endregion

        #region BuildResult

        private string _BuildResult = "";

        public string BuildResult
        {
            get => _BuildResult;
            set => SetProperty(ref _BuildResult, value);
        }

        #endregion

        #region BuildMessage

        private string _BuildMessage = "";

        public string BuildMessage
        {
            get => _BuildMessage;
            set => SetProperty(ref _BuildMessage, value);
        }

        #endregion

        #region IsBuildOk

        private bool _IsBuildOk;

        public bool IsBuildOk
        {
            get => _IsBuildOk;
            set => SetProperty(ref _IsBuildOk, value);
        }

        #endregion

        #region IsReleaseBuild

        private bool _IsReleaseBuild;

        public bool IsReleaseBuild
        {
            get => _IsReleaseBuild;
            set => SetProperty(ref _IsReleaseBuild, value);
        }

        #endregion

        #region IsTieredJit

        private bool _IsTieredJit;

        public bool IsTieredJit
        {
            get => _IsTieredJit;
            set => SetProperty(ref _IsTieredJit, value);
        }

        #endregion

        #region IsInProcessing

        private bool _IsInProcessing;

        public bool IsInProcessing
        {
            get => _IsInProcessing;
            set => SetProperty(ref _IsInProcessing, value);
        }

        #endregion

        private readonly CompositeDisposable _Trashes = new CompositeDisposable();

        public ProcessingUnit()
        {
            this.ObserveProperty(x => x.SourceText)
                .Throttle(TimeSpan.FromMilliseconds(500))
                .ObserveOn(ThreadPoolScheduler.Instance)
                .Subscribe(x => OnProcess())
                .AddTo(_Trashes);

            Observable
                .Merge(this.ObserveProperty(x => x.IsReleaseBuild))
                .Merge(this.ObserveProperty(x => x.IsTieredJit))
                .ObserveOn(ThreadPoolScheduler.Instance)
                .Subscribe(x => OnProcess())
                .AddTo(_Trashes);
        }

        private void OnProcess()
        {
            _ProcessedSourceText = SourceText;

            if (IsInProcessing)
                return;

            IsInProcessing = true;

            try
            {
                var (isOk, result, message) = DoProcess();

                if (isOk)
                    BuildResult = result;
                else
                    BuildMessage = message;

                IsBuildOk = isOk;
            }
            finally
            {
                IsInProcessing = false;
            }
        }

        private string _ProcessedSourceText = "";

        public void Dispose()
        {
            _Trashes.Dispose();
        }

        private (bool, string, string) DoProcess()
        {
            if (string.IsNullOrEmpty(_ProcessedSourceText))
                return (true, "", "");

            DisassembleResult result;

            string sourceText;
            do
            {
                sourceText = _ProcessedSourceText;

                var jitMaker = new JitDisassembler(_ProcessedSourceText, IsReleaseBuild, IsTieredJit);

                result = jitMaker.Run();
            } while (sourceText != _ProcessedSourceText);

            return (result.IsOk, result.Output, string.Join("\n", result.Messages));
        }
    }
}