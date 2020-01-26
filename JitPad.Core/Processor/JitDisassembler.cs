using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace JitPad.Core.Processor
{
    public static class JitDisassembler
    {
        public static DisassembleResult Run(string sourceCode, byte[] assembleImage, bool isTieredJit, string? jitDasmExe = null)
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
                        FileName = jitDasmExe ?? "JitDasm/JitDasm.exe",
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