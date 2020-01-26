using System;
using System.Linq;
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

        private string _SourceCode = "";

        public string SourceCode
        {
            get => _SourceCode;
            set => SetProperty(ref _SourceCode, value);
        }

        #endregion

        #region BuildResult

        private string _BuildResult = "";

        public string BuildResult
        {
            get => _BuildResult;
            private set => SetProperty(ref _BuildResult, value);
        }

        #endregion

        #region BuildMessage

        private string _BuildMessage = "";

        public string BuildMessage
        {
            get => _BuildMessage;
            private set => SetProperty(ref _BuildMessage, value);
        }

        #endregion

        #region IsBuildOk

        private bool _IsBuildOk;

        public bool IsBuildOk
        {
            get => _IsBuildOk;
            private set => SetProperty(ref _IsBuildOk, value);
        }

        #endregion

        #region IsInProcessing

        private bool _IsInProcessing;

        public bool IsInProcessing
        {
            get => _IsInProcessing;
            private set => SetProperty(ref _IsInProcessing, value);
        }

        #endregion

        private readonly Config _config;
        private readonly CompositeDisposable _Trashes = new CompositeDisposable();

        public ProcessingUnit(Config config)
        {
            _config = config;
            
            this.ObserveProperty(x => x.SourceCode)
                .Throttle(TimeSpan.FromMilliseconds(500))
                .ObserveOn(ThreadPoolScheduler.Instance)
                .Subscribe(x => OnProcess())
                .AddTo(_Trashes);

            Observable
                .Merge(config.ObserveProperty(x => x.IsReleaseBuild))
                .Merge(config.ObserveProperty(x => x.IsTieredJit))
                .ObserveOn(ThreadPoolScheduler.Instance)
                .Subscribe(x => OnProcess())
                .AddTo(_Trashes);
        }

        private void OnProcess()
        {
            _ProcessedSourceCode = SourceCode;

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

        private string _ProcessedSourceCode = "";

        public void Dispose()
        {
            _Trashes.Dispose();
        }

        private (bool, string, string) DoProcess()
        {
            if (string.IsNullOrEmpty(_ProcessedSourceCode.Trim()))
                return (true, "", "");

            DisassembleResult result;

            string sourceCode;
            do
            {
                sourceCode = _ProcessedSourceCode;
                
                // compile
                var compileResult = Compiler.Run(_ProcessedSourceCode, _config.IsReleaseBuild);
                if (compileResult.IsOk == false)
                    return (false, "", string.Join("\n", compileResult.Messages.Select(x => x.ToString())));
                
                // jit disassemble
                result = JitDisassembler.Run(_ProcessedSourceCode, compileResult.AssembleImage, _config.IsTieredJit);
            } while (sourceCode != _ProcessedSourceCode);

            return (result.IsOk, result.Output, string.Join("\n", result.Messages));
        }
    }
}