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

            var compileResult = Compiler.Run(sourceCode, true);
            
            var result = JitDisassembler.Run(sourceCode, compileResult.AssembleImage, true, "../../../../externals/JitDasm/JitDasm/bin/Release/netcoreapp3.0/JitDasm.exe");
            
            Assert.True(result.IsOk);
        }
    }
}