namespace JitPad.Core.Processor.Interface
{
    public interface ICompiler
    {
        CompileResult Run(string sourceCode, bool isReleaseBuild);
    }
}