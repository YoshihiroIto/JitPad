namespace JitPad.Core.Interface
{
    public interface IDisassembler
    {
        DisassembleResult Run(string sourceCode, byte[] assembleImage, bool isTieredJit);
    }
    
    public class DisassembleResult
    {
        public readonly bool IsOk;
        public readonly string Output;
        public readonly string Message;

        public DisassembleResult(bool isOk, string output, string messages)
        {
            IsOk = isOk;
            Output = output;
            Message = messages;
        }
    }
}