namespace JitPad.Core.Interface
{
    public interface IDisassembler
    {
        DisassembleResult Run(string sourceCodePath, string sourceCode, byte[] assembleImage, bool isTieredJit);
    }
    
    public class DisassembleResult
    {
        public readonly bool IsOk;
        public readonly string Output;

        public DisassembleResult(bool isOk, string output)
        {
            IsOk = isOk;
            Output = output;
        }
    }
}