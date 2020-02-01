using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using JitPad.Core.Interface;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Text;

namespace JitPad.Core.Processor
{
    public class Compiler : ICompiler
    {
        public CompileResult Run(string sourceCodePath, string sourceCode, bool isReleaseBuild)
        {
            using var asmImage = new MemoryStream();

            var symbolsName = Path.ChangeExtension("compiled.dll", ".pdb");

            var buffer = Encoding.UTF8.GetBytes(sourceCode);
            var sourceText = SourceText.From(buffer, buffer.Length, Encoding.UTF8, canBeEmbedded: true);

            var compilation = GenerateCode("compiled.dll", sourceText, sourceCodePath, isReleaseBuild);

            var emitOptions = new EmitOptions(
                debugInformationFormat: DebugInformationFormat.Embedded,
                pdbFilePath: symbolsName
            );

            var result = compilation.Emit(
                asmImage,
                embeddedTexts: new[] {EmbeddedText.FromSource(sourceCodePath, sourceText)},
                options: emitOptions);

            if (result.Success)
            {
                asmImage.Seek(0, SeekOrigin.Begin);

                return new CompileResult(asmImage.ToArray(), Array.Empty<CompileResult.Message>());
            }
            else
            {
                var messages = result.Diagnostics
                    .Where(x =>
                        x.IsWarningAsError ||
                        x.Severity == DiagnosticSeverity.Error)
                    .OrderBy(x => x.Location.SourceSpan.Start);

                return new CompileResult(Array.Empty<byte>(),
                    messages.Select(x =>
                    {
                        var lineSpan = x.Location.GetMappedLineSpan().Span;

                        var startLine = lineSpan.Start.Line;
                        var startCharacter = lineSpan.Start.Character;
                        var endLine = lineSpan.End.Line;
                        var endCharacter = lineSpan.End.Character;
                        var severity = x.Severity.ToString().ToLower();

                        return new CompileResult.Message(
                            startLine, startCharacter,
                            endLine, endCharacter,
                            severity, x.Id, x.GetMessage()
                        );
                    }).ToArray());
            }
        }

        private CSharpCompilation GenerateCode(string assemblyName, SourceText sourceText, string sourceCodePath, bool isReleaseBuild)
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
                    .WithPlatform(Platform.AnyCpu)
                    .WithAllowUnsafe(true));

            compilation = compilation.AddReferences(MetadataReferences);

            return compilation;
        }

        public MetadataReference[] MetadataReferences { get; } = EnumMetadataReferences().ToArray();

        private static IEnumerable<MetadataReference> EnumMetadataReferences()
        {
            var assemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location);

            foreach (var x in Directory.EnumerateFiles(assemblyPath, "*.dll"))
            {
                var fileName = Path.GetFileName(x);

                if (fileName.IndexOf("Native", StringComparison.Ordinal) == -1 &&
                    (fileName.StartsWith("System.") || fileName.StartsWith("Microsoft.")))
                    yield return MetadataReference.CreateFromFile(x);
            }
        }
    }
}