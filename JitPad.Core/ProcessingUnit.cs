using JitPad.Foundation;

namespace JitPad.Core
{
    public class ProcessingUnit : NotificationObject
    {
        public SourceFile SourceFile { get; private set; } = new SourceFile();
        
        #region Result
        
        private string _Result = "";
        
        public string Result
        {
            get => _Result;
            set => SetProperty(ref _Result, value);
        }
        
        #endregion
    }
}