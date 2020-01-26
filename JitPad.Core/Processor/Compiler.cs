using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Text;

namespace JitPad.Core.Processor
{
    public static class Compiler
    {
        public static CompileResult Run(string sourceCode, bool isReleaseBuild)
        {
            using var asmImage = new MemoryStream();

            var symbolsName = Path.ChangeExtension("compiled.dll", ".pdb");

            var buffer = Encoding.UTF8.GetBytes(sourceCode);
            var sourceText = SourceText.From(buffer, buffer.Length, Encoding.UTF8, canBeEmbedded: true);

            var compilation = GenerateCode("compiled.dll", sourceText, "source.cs", isReleaseBuild);

            var emitOptions = new EmitOptions(
                debugInformationFormat: DebugInformationFormat.Embedded,
                pdbFilePath: symbolsName
            );

            var result = compilation.Emit(
                asmImage,
                embeddedTexts: new[] {EmbeddedText.FromSource("source.cs", sourceText)},
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
                    .Select(x =>
                    {
                        var lineSpan = x.Location.GetMappedLineSpan().Span;

                        var line = lineSpan.Start.Line + 1;
                        var character = lineSpan.Start.Character + 1;
                        var severity = x.Severity.ToString().ToLower();

                        return $"({line},{character}) {severity} {x.Id}: {x.GetMessage()}";
                    });

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

                if (fileName.IndexOf("Native", StringComparison.Ordinal) == -1 &&
                    (fileName.StartsWith("System.") || fileName.StartsWith("Microsoft.")))
                    references.Add(MetadataReference.CreateFromFile(x));
            }

            compilation = compilation.AddReferences(references);

            return compilation;
        }
    }
}