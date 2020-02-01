using JitPad.Foundation;

namespace JitPad.Core
{
    public class OpenSource
    {
        public static readonly OpenSourceMetadata[] OpenSources =
            EmbeddedResourceReader.Read<OpenSourceMetadata[]>(
                typeof(OpenSource).Assembly, "JitPad.Core.Resources.OssList.json");
    }

    public class OpenSourceMetadata
    {
        public string? Name { get; set; }
        public string? Author { get; set; }
        public string? Summary { get; set; }
        public string? Url { get; set; }
    }
}