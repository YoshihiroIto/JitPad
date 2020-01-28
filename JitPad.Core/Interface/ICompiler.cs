namespace JitPad.Core.Interface
{
    public interface ICompiler
    {
        CompileResult Run(string sourceCode, bool isReleaseBuild);
    }
    
    public class CompileResult
    {
        public readonly byte[] AssembleImage;
        public readonly Message[] Messages;

        public bool IsOk => AssembleImage.Length > 0;

        public CompileResult(byte[] assembleImage, Message[] messages)
        {
            AssembleImage = assembleImage;
            Messages = messages;
        }

        public class Message
        {
            public readonly int StartLine;
            public readonly int StartCharacter;
            public readonly int EndLine;
            public readonly int EndCharacter;
            
            public readonly string Severity;
            public readonly string Id;
            public readonly string Text;

            public override string ToString()
                => $"({StartLine + 1},{StartCharacter + 1}) {Severity} {Id}: {Text}";

            public Message(int startLine, int startCharacter, int endLine, int endCharacter, string severity, string id, string text)
            {
                StartLine = startLine;
                StartCharacter = startCharacter;
                EndLine = endLine;
                EndCharacter = endCharacter;
                Severity = severity;
                Id = id;
                Text = text;
            }
        }
    }
    
}