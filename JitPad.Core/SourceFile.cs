using JitPad.Foundation;

namespace JitPad.Core
{
    public class SourceFile : NotificationObject
    {
        #region Text
        
        private string _Text = "";
        
        public string Text
        {
            get => _Text;
            set => SetProperty(ref _Text, value);
        }
        
        #endregion
    }
}