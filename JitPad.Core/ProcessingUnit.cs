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
        public SourceFile SourceFile { get; } = new SourceFile();

        #region Result

        private string _Result = "";

        public string Result
        {
            get => _Result;
            set => SetProperty(ref _Result, value);
        }

        #endregion

        #region Message

        private string _Message = "";
        
        public string Message
        {
            get => _Message;
            set => SetProperty(ref _Message, value);
        }
        
        #endregion

        #region IsOk
        
        private bool _IsOk;
        
        public bool IsOk
        {
            get => _IsOk;
            set => SetProperty(ref _IsOk, value);
        }
        
        #endregion

        private readonly CompositeDisposable _Trashes = new CompositeDisposable();

        public ProcessingUnit()
        {
            SourceFile.ObserveProperty(x => x.Text)
                .Throttle(TimeSpan.FromMilliseconds(500))
                .ObserveOn(ThreadPoolScheduler.Instance)
                .Subscribe(x =>
                {
                    _ProcessedSource = SourceFile.Text;

                    if (_isInProcessing)
                        return;

                    _isInProcessing = true;

                    try
                    {
                        var (isOk, result, message) = DoProcess();

                        if (isOk)
                            Result = result;
                        else
                            Message = message;

                        IsOk = isOk;
                    }
                    finally
                    {
                        _isInProcessing = false;
                    }
                })
                .AddTo(_Trashes);
        }

        private string _ProcessedSource = "";

        private bool _isInProcessing;

        public void Dispose()
        {
            _Trashes.Dispose();
        }

        private (bool, string, string) DoProcess()
        {
            if (string.IsNullOrEmpty(_ProcessedSource))
                return (true, "", "");

            DisassembleResult result;

            string sourceText;
            do
            {
                sourceText = _ProcessedSource;
                
                var jitMaker = new JitMaker(_ProcessedSource, true);

                result = jitMaker.Run();
            } while (sourceText != _ProcessedSource);

            return (result.IsOk, result.Output, string.Join("\n", result.Messages));
        }
    }
}