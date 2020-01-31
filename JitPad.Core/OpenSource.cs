using System.Text.Json;
using JitPad.Core.Properties;

namespace JitPad.Core
{
    public class OpenSource
    {
        #region OpenSources

        private OpenSourceMetadata[]? _OpenSources;

        public OpenSourceMetadata[] OpenSources => _OpenSources ??= ReadOpenSources();

        #endregion

        private static OpenSourceMetadata[] ReadOpenSources()
            => JsonSerializer.Deserialize<OpenSourceMetadata[]>(Resources.OssList);
    }
    
    public class OpenSourceMetadata
    {
        public string? Name { get; set; }
        public string? Author { get; set; }
        public string? Summary { get; set; }
        public string? Url { get; set; }
    }
}