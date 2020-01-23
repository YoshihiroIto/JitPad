using JitPad.Foundation;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using AppContext = JitPad.Core.AppContext;

namespace JitPad
{
    public class MainWindowViewModel : ViewModelBase
    {
        public ReactiveProperty<string> PrimarySourceText { get; }
        public ReadOnlyReactiveProperty<string> PrimaryResult { get; }

        public MainWindowViewModel(AppContext appContext)
        {
            PrimarySourceText = appContext.Primary.SourceFile.ToReactivePropertyAsSynchronized(x => x.Text).AddTo(Trashes);
            PrimaryResult = appContext.Primary.ObserveProperty(x => x.Result).ToReadOnlyReactiveProperty().AddTo(Trashes);
        }
    }
}