using JitPad.Core.Processor;
using Xunit;

namespace JitPad.Core.Test
{
    public class JitDisAssemblerTest
    {
        [Fact]
        public void Ok()
        {
            const string sourceText = @"
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

            var compileResult = Compiler.Run(sourceText, true);
            
            var result = JitDisassembler.Run(sourceText, compileResult.AssembleImage, true, "../../../../externals/JitDasm/JitDasm/bin/Release/netcoreapp3.0/JitDasm.exe");
            
            Assert.True(result.IsOk);
        }
    }
}