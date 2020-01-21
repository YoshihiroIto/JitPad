using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Text;

namespace JitPad.Core
{
    public class Compiler
    {
        public CompileResult Compile(string assemblyName, string sourceCode, string? sourceCodePath,
            bool isReleaseBuild)
        {
            using var asmImage = new MemoryStream();

            sourceCodePath ??= Path.ChangeExtension(assemblyName, ".cs");

            var symbolsName = Path.ChangeExtension(assemblyName, ".pdb");

            var buffer = Encoding.UTF8.GetBytes(sourceCode);
            var sourceText = SourceText.From(buffer, buffer.Length, Encoding.UTF8, canBeEmbedded: true);

            var compilation = GenerateCode(assemblyName, sourceText, sourceCodePath, isReleaseBuild);

            var emitOptions = new EmitOptions(
                debugInformationFormat: DebugInformationFormat.Embedded,
                pdbFilePath: symbolsName
            );

            var result = compilation.Emit(
                peStream: asmImage,
                embeddedTexts: new[] {EmbeddedText.FromSource(sourceCodePath, sourceText)},
                options: emitOptions);

            if (result.Success)
            {
                asmImage.Seek(0, SeekOrigin.Begin);

                return new CompileResult(asmImage.ToArray(), Array.Empty<string>());
            }
            else
            {
                var messages = result.Diagnostics
                    .Where(x =>
                        x.IsWarningAsError ||
                        x.Severity == DiagnosticSeverity.Error)
                    .OrderBy(x => x.Location.SourceSpan.Start)
                    .Select(x => x.ToString());

                return new CompileResult(Array.Empty<byte>(), messages.ToArray());
            }
        }

        private static CSharpCompilation GenerateCode(string assemblyName, SourceText sourceText, string sourceCodePath,
            bool isReleaseBuild)
        {
            var options = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Default);
            var syntaxTree = CSharpSyntaxTree.ParseText(sourceText, options, path: sourceCodePath);

            var syntaxRootNode = syntaxTree.GetRoot() as CSharpSyntaxNode;
            var encoded = CSharpSyntaxTree.Create(syntaxRootNode, null, sourceCodePath, Encoding.UTF8);

            var compilation = CSharpCompilation.Create(
                assemblyName,
                new[] {encoded},
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                    .WithOptimizationLevel(isReleaseBuild ? OptimizationLevel.Release : OptimizationLevel.Debug)
                    .WithPlatform(Platform.AnyCpu));

            var references = new List<MetadataReference>();
            var assemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location);

            foreach (var x in Directory.EnumerateFiles(assemblyPath, "*.dll"))
            {
                var fileName = Path.GetFileName(x);
                
                if (fileName.IndexOf("Native", StringComparison.Ordinal) == -1  &&
                    (fileName.StartsWith("System.") || fileName.StartsWith("Microsoft.")))
                    references.Add(MetadataReference.CreateFromFile(x));
            }

            compilation = compilation.AddReferences(references);

            return compilation;
        }
    }
}