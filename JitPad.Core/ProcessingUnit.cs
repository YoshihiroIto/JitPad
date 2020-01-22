using System;
using System.Diagnostics;
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

        private readonly CompositeDisposable _Trashes = new CompositeDisposable();

        public ProcessingUnit()
        {
            this.SourceFile.ObserveProperty(x => x.Text)
                .Throttle(TimeSpan.FromMilliseconds(50))
                .Subscribe(x => Result = DoProcess(SourceFile.Text))
                .AddTo(_Trashes);
        }

        public void Dispose()
        {
            _Trashes.Dispose();
        }

        private static string DoProcess(string sourceText)
        {
            if (string.IsNullOrEmpty(sourceText))
                return "";

            var jitMaker = new JitMaker(sourceText, true);

            var result = jitMaker.Run();

            return result.IsOk ? result.Output : string.Join("\n", result.Messages);
        }
    }
}