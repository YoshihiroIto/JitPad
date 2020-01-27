using JitPad.Core.Processor;
using Xunit;

namespace JitPad.Core.Test
{
    public class JitDisAssemblerTest
    {
        [Fact]
        public void Ok()
        {
            const string sourceCode = @"
namespace TestNameSpace
{
public class TestClass
{
    public int TestMethod(int a, int b, int c)
    {
        return (a + b) * c;
    }
}
}";

            var compiler = new Compiler();
            var compileResult = compiler.Run(sourceCode, true);
            
            var disassembler = new JitDisassembler("../../../../externals/JitDasm/JitDasm/bin/Release/netcoreapp3.0/JitDasm.exe");
            var result = disassembler.Run(sourceCode, compileResult.AssembleImage, true);
            
            Assert.True(result.IsOk);
        }
    }
}