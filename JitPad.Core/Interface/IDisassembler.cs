using System;

namespace JitPad.Core.Interface
{
    public interface IDisassembler
    {
        DisassembleResult Run(string sourceCodePath, string sourceCode, byte[] assembleImage, JitFlags flags);
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

    [Flags]
    public enum JitFlags
    {
        // ReSharper disable InconsistentNaming
        None = 0,
        TieredCompilation = 1 << 0,
        TC_QuickJit = 1 << 1,
        TC_QuickJitForLoops = 1 << 2
        // ReSharper restore InconsistentNaming
    }
}