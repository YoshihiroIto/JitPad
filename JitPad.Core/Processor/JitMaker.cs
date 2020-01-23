using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace JitPad.Core.Processor
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
            var sourceCodeTempPath = Path.GetTempFileName() + ".cs";
            var assemblyTempPath = Path.ChangeExtension(sourceCodeTempPath, ".dll");

            try
            {
                File.WriteAllText(sourceCodeTempPath, _sourceCode, Encoding.UTF8);

                var compiler = new Compiler();

                var compileResult = compiler.Compile("compiled.dll", _sourceCode, sourceCodeTempPath, _isReleaseBuild);
                if (compileResult.IsOk == false)
                    return new DisassembleResult(false, "", compileResult.Messages);

                File.WriteAllBytes(assemblyTempPath, compileResult.AssembleImage);

                var procInfo = new ProcessStartInfo
                {
                    FileName = "JitDasm",
                    Arguments = "--diffable -l " + assemblyTempPath,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true
                };

                var proc = Process.Start(procInfo);
                if (proc == null)
                    return new DisassembleResult(false, "", Array.Empty<string>());

                var output = proc.StandardOutput.ReadToEnd().Replace("\r\r\n", "\n");

                return proc.ExitCode == 0
                    ? new DisassembleResult(true, output, Array.Empty<string>())
                    : new DisassembleResult(false, "", output.Split("\n"));
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
                    GC.WaitForFullGCComplete();
                }
            }
        }
    }
}