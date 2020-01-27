namespace JitPad.Core.Processor.Interface
{
    public interface IDisassembler
    {
        DisassembleResult Run(string sourceCode, byte[] assembleImage, bool isTieredJit);
    }
}