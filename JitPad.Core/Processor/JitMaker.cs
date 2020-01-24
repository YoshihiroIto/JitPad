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
        private readonly bool _IsTieredJit;

        public JitMaker(string sourceCode, bool isReleaseBuild, bool isTieredJit)
        {
            _sourceCode = sourceCode;
            _isReleaseBuild = isReleaseBuild;
            _IsTieredJit = isTieredJit;
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

                using var proc = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "JitDasm/JitDasm.exe",
                        Arguments = "--diffable -l " + assemblyTempPath,
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        RedirectStandardOutput = true
                    }
                };

                proc.StartInfo.Environment["COMPlus_TieredCompilation"] = _IsTieredJit ? "1" : "0";

                proc.Start();

                var stdout = new StringBuilder();

                proc.OutputDataReceived += (_, e) => stdout.AppendLine(e.Data);
                proc.BeginOutputReadLine();
                var r = proc.WaitForExit(5 * 1000);
                proc.CancelOutputRead();

                if (r == false)
                    return new DisassembleResult(false, "", new[] {"Timeout"});

                var output = stdout.ToString();

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
                    GC.WaitForPendingFinalizers();
                }
            }
        }
    }
}