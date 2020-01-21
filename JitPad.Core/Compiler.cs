using System;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace JitPad.Core
{
    public class Compiler
    {
        public CompileResult Compile(string assemblyName, string sourceCode, bool isReleaseBuild)
        {
            using var asmImage = new MemoryStream();

            var result = GenerateCode(assemblyName, sourceCode, isReleaseBuild).Emit(asmImage);

            if (result.Success != false)
            {
                asmImage.Seek(0, SeekOrigin.Begin);

                return new CompileResult(asmImage.ToArray(), Array.Empty<string>());
            }
            else
            {
                var messages = result.Diagnostics
                    .OrderBy(x => x.Location.SourceSpan.Start)
                    .Select(x => x.ToString());

                return new CompileResult(Array.Empty<byte>(), messages.ToArray());
            }
        }

        private static CSharpCompilation GenerateCode(string assemblyName, string sourceCode, bool isReleaseBuild)
        {
            var sourceText = SourceText.From(sourceCode);
            var options = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Default);
            var syntaxTree = SyntaxFactory.ParseSyntaxTree(sourceText, options);
            var references = new MetadataReference[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            };

            return CSharpCompilation.Create(
                assemblyName,
                new[] {syntaxTree},
                references: references,
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,
                    optimizationLevel: isReleaseBuild ? OptimizationLevel.Release : OptimizationLevel.Debug,
                    assemblyIdentityComparer: DesktopAssemblyIdentityComparer.Default));
        }
    }
}