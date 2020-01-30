using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using JitPad.Core.Interface;

namespace JitPad.Core.Processor
{
    public class JitDisassembler : IDisassembler
    {
        private readonly string _jitDasmExe;

        public JitDisassembler(string jitDasmExe)
        {
            _jitDasmExe = jitDasmExe;
        }

        public DisassembleResult Run(string sourceCodePath, string sourceCode, byte[] assembleImage, bool isTieredJit)
        {
            var sourceCodeTempPath = sourceCodePath;
            var assemblyTempPath = Path.ChangeExtension(sourceCodeTempPath, ".dll");

            try
            {
                File.WriteAllText(sourceCodeTempPath, sourceCode);
                File.WriteAllBytes(assemblyTempPath, assembleImage);

                using var proc = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = _jitDasmExe,
                        Arguments = "--method-exclude .ctor --diffable -l " + assemblyTempPath,
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        RedirectStandardOutput = true
                    }
                };

                proc.StartInfo.Environment["COMPlus_TieredCompilation"] = isTieredJit ? "1" : "0";

                proc.Start();

                var stdout = new StringBuilder();

                proc.OutputDataReceived += (_, e) => stdout.AppendLine(e.Data);
                proc.BeginOutputReadLine();
                var exited = proc.WaitForExit(5 * 1000);
                proc.CancelOutputRead();

                return exited
                    ? new DisassembleResult(proc.ExitCode == 0, stdout.ToString())
                    : new DisassembleResult(false, "Timeout");
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