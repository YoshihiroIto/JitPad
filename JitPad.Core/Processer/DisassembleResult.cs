namespace JitPad.Core.Processer
{
    public class DisassembleResult
    {
        public readonly bool IsOk;
        public readonly string Output;
        public readonly string[] Messages;

        public DisassembleResult(bool isOk, string output, string[] messages)
        {
            IsOk = isOk;
            Output = output;
            Messages = messages;
        }
    }
}