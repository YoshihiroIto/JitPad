using System;
using System.Reactive.Disposables;
using System.Threading;

namespace JitPad.Foundation
{
    public class ViewModelBase : NotificationObject, IDisposable
    {
        private CompositeDisposable? _Trashes;

        public CompositeDisposable Trashes =>
            LazyInitializer.EnsureInitialized(ref _Trashes, () => new CompositeDisposable()) ??
            throw new NullReferenceException();

        private bool _disposed;

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
                _Trashes?.Dispose();

            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}