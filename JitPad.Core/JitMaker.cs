using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace JitPad.Core
{
    public class JitMaker : IDisassembler
    {
        private readonly string _sourceCode;
        private readonly bool _isReleaseBuild;

        public JitMaker(string sourceCode, bool isReleaseBuild)
        {
            _sourceCode = sourceCode;
            _isReleaseBuild = isReleaseBuild;
        }

        public DisassembleResult Run()
        {
            const string assemblyName = "compiled.dll";

            var compiler = new Compiler();

            var compileResult = compiler.Compile(assemblyName, _sourceCode, _isReleaseBuild);
            if (compileResult.IsOk == false)
                return new DisassembleResult(false, "", compileResult.Messages);

            var assemblyLoadContext = new UnloadableAssemblyLoadContext();

            try
            {
                var assembly = assemblyLoadContext.LoadFromStream(new MemoryStream(compileResult.AssembleImage));

                var args = new[]
                {
                    "-m", assemblyName + ", Version=0.0.0.0, Culture=neutral, PublicKeyToken=null",
                    "-p", Process.GetCurrentProcess().Id.ToString()
                };
                
                var output = new StringWriter();

                var retCode = JitDasm.Program.MainForJitPad(compileResult.AssembleImage, assembly, output, args);

                if (retCode == 0)
                {
                    return new DisassembleResult(true, output.ToString(), Array.Empty<string>());
                }
                else
                {
                    return new DisassembleResult(false, "", output.ToString().Split("\n"));
                }
            }
            finally
            {
                assemblyLoadContext.Unload();
            }
        }
    }
}