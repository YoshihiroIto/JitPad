using System.Linq;
using JitPad.Core.Processor;
using Xunit;

namespace JitPad.Core.Test
{
    public class CompilerTest
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CompileOk(bool isReleaseBuild)
        {
            const string sourceCode = @"namespace TestNameSpace
{
public class TestClass
{
    public int TestMethod(int a, int b, int c)
    {
        return (a + b) * c;
    }
}
}";

            var result = Compiler.Run(sourceCode, isReleaseBuild);

            Assert.True(result.IsOk);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CompileNg(bool isReleaseBuild)
        {
            const string sourceCode = @"namespace TestNameSpace
{
public class TestClass
{
    public int TestMethod(int a, int b, int c)
    {
 err       return (a + b) * c;
    }
}
}";

            var result = Compiler.Run(sourceCode, isReleaseBuild);

            Assert.False(result.IsOk);
            Assert.True(result.Messages.Any());
        }
    }
}