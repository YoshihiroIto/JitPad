using System;
using JitPad.Foundation;

namespace JitPad.Core
{
    public class AppContext : NotificationObject, IDisposable
    {
        #region IsReleaseBuild

        private bool _IsReleaseBuild = true;

        public bool IsReleaseBuild
        {
            get => _IsReleaseBuild;
            set
            {
                if (SetProperty(ref _IsReleaseBuild, value))
                    Primary.IsReleaseBuild = value;
            }
        }

        #endregion

        public ProcessingUnit Primary { get; }

        public AppContext()
        {
            Primary = new ProcessingUnit
            {
                IsReleaseBuild = IsReleaseBuild
            };
        }

        public void Dispose()
        {
            Primary.Dispose();
        }
    }
}