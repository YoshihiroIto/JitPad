using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using JitPad.Core.Interface;

namespace JitPad.Core.Processor
{
    public class JitDisassembler : IDisassembler
    {
        public DisassembleResult Run(string sourceCodePath, string sourceCode, byte[] assembleImage, bool isTieredJit)
        {
            var sourceCodeTempPath = sourceCodePath;
            var assemblyTempPath = Path.ChangeExtension(sourceCodeTempPath, ".dll");

            try
            {
                File.WriteAllText(sourceCodeTempPath, sourceCode);
                File.WriteAllBytes(assemblyTempPath, assembleImage);

                // todo:
                //Environment.SetEnvironmentVariable("COMPlus_TieredCompilation", isTieredJit ? "1" : "0");

                var output = new StringBuilder();
                var exitCode = JitDasm.Program.Main(output, new[] {"--method-exclude", ".ctor", "--diffable", "-l", assemblyTempPath});

                return new DisassembleResult(exitCode == 0, output.ToString());
            }
            finally
            {
                SafeFileDelete(sourceCodeTempPath);
                SafeFileDelete(assemblyTempPath);
            }
        }

        private static void SafeFileDelete(string filePath)
        {
            for (var i = 0; i != 4; ++i)
            {
                try
                {
                    File.Delete(filePath);
                    break;
                }
                catch
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }
            }
        }
    }
}