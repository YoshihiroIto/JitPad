using System;
using System.IO;
using System.Reactive.Linq;

namespace JitPad.Foundation
{
    public class FileMonitor : IDisposable
    {
        public IObservable<FileSystemEventArgs> Changed { get; }
        
        private readonly FileSystemWatcher _watcher;

        public FileMonitor(string filePath)
        {
            var path = Path.GetDirectoryName(filePath);

            _watcher = new FileSystemWatcher
            {
                Path = path,
                NotifyFilter = NotifyFilters.LastWrite,
                IncludeSubdirectories = false,
                EnableRaisingEvents = true
            };

            Changed = Observable
                .FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(h => _watcher.Changed += h, h => _watcher.Changed -= h)
                .Select(x => x.EventArgs)
                .Where(x => x.FullPath == filePath)
                .Throttle(TimeSpan.FromMilliseconds(200));
        }

        public void Dispose()
        {
            _watcher.Dispose();
            (Changed as IDisposable)?.Dispose();
        }
    }
}