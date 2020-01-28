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
        
        public DisassembleResult Run(string sourceCode, byte[] assembleImage, bool isTieredJit)
        {
            var sourceCodeTempPath = Path.GetTempFileName() + ".cs";
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
                        Arguments = "--diffable -l " + assemblyTempPath,
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
                var r = proc.WaitForExit(5 * 1000);
                proc.CancelOutputRead();

                if (r == false)
                    return new DisassembleResult(false, "", "Timeout");

                var output = stdout.ToString();

                return proc.ExitCode == 0
                    ? new DisassembleResult(true, output, "")
                    : new DisassembleResult(false, "", output);
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