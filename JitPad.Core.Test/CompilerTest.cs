using System.Linq;
using JitPad.Core.Processor;
using Xunit;

namespace JitPad.Core.Test
{
    public class CompilerTest
    {
        [Fact]
        public void Smoke()
        {
            // ReSharper disable once ObjectCreationAsStatement
            new Compiler();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CompileOk(bool isReleaseBuild)
        {
            var sourceCode = @"namespace TestNameSpace
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

            var result = compiler.Compile("test.dll", sourceCode, null, isReleaseBuild);

            Assert.True(result.IsOk);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CompileNg(bool isReleaseBuild)
        {
            var sourceCode = @"namespace TestNameSpace
{
public class TestClass
{
    public int TestMethod(int a, int b, int c)
    {
 err       return (a + b) * c;
    }
}
}";

            var compiler = new Compiler();

            var result = compiler.Compile("test.dll", sourceCode, null, isReleaseBuild);

            Assert.False(result.IsOk);
            Assert.True(result.Messages.Any());
        }
    }
}