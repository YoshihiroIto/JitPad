using JitPad.Foundation;
using AppContext = JitPad.Core.AppContext;

namespace JitPad
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly AppContext _appContext;

        public MainWindowViewModel(AppContext appContext)
        {
            _appContext = appContext;
        }
    }
}