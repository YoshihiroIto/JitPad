using System;
using JitPad.Foundation;

namespace JitPad.Core
{
    public class AppContext : NotificationObject, IDisposable
    {
        public ProcessingUnit Primary { get; } = new ProcessingUnit();
        public ProcessingUnit Secondary { get; } = new ProcessingUnit();

        public void Dispose()
        {
            Primary.Dispose();
            Secondary.Dispose();
        }
    }
}