using JitPad.Core.Processor;
using Xunit;

namespace JitPad.Core.Test
{
    public class JitDisAssemblerTest
    {
        [Fact]
        public void Ok()
        {
            var sourceCode = @"
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

            var jitMaker = new JitDisassembler( sourceCode, true, true);

            var result = jitMaker.Run();
        }
    }
}