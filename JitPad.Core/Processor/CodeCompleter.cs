using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JitPad.Core.Interface;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Text;

namespace JitPad.Core.Processor
{
    // ref: RoslynPad -- https://github.com/aelij/RoslynPad

    public class CodeCompleter
    {
        private readonly ICompiler _compiler;
        private readonly MefHostServices _Host = MefHostServices.Create(MefHostServices.DefaultAssemblies);

        public CodeCompleter(ICompiler compiler)
        {
            _compiler = compiler;
            if (_isInitialized)
                return;

            Task.Run(async () =>
            {
                if (_isInitialized)
                    return;

                _isInitialized = true;

                // load Roslyn
                await CompleteAsync("", 0, null)
                    .ConfigureAwait(false);
            });
        }

        private static bool _isInitialized;

        private CancellationTokenSource? _CancellationTokenSource;

        public async Task<CompletionResult> CompleteAsync(string sourceCode, int position, char? triggerChar)
        {
            _CancellationTokenSource?.Cancel();
            _CancellationTokenSource?.Dispose();
            _CancellationTokenSource = new CancellationTokenSource();

            try
            {
                var workspace = new AdhocWorkspace(_Host);

                var projectInfo = ProjectInfo
                    .Create(ProjectId.CreateNewId(), VersionStamp.Create(), "Project", "Project", LanguageNames.CSharp)
                    .WithMetadataReferences(_compiler.MetadataReferences);
                var project = workspace.AddProject(projectInfo);
                var document = workspace.AddDocument(project.Id, "File.cs", SourceText.From(sourceCode));

                var completionService = CompletionService.GetService(document);
                var completionTrigger = GetCompletionTrigger(triggerChar);
                var data = await completionService.GetCompletionsAsync(document, position, completionTrigger, null, null, _CancellationTokenSource.Token)
                    .ConfigureAwait(false);

                if (data == null || data.Items == null)
                    return new CompletionResult(Array.Empty<CompleteData>());

                var helper = CompletionHelper.GetHelper(document);
                var text = await document.GetTextAsync(_CancellationTokenSource.Token).ConfigureAwait(false);
                var textSpanToText = new Dictionary<TextSpan, string>();

                var items =
                    data.Items
                        .Where(item => MatchesFilterText(helper, item, text, textSpanToText))
                        .OrderBy(x => x.DisplayText)
                        .Distinct(x => x.DisplayText)
                        .Select(x =>
                            new CompleteData(
                                x,
                                completionService,
                                document)
                        ).ToArray();

                return new CompletionResult(items);
            }
            catch (OperationCanceledException)
            {
                return new CompletionResult(Array.Empty<CompleteData>());
            }
        }

        private static CompletionTrigger GetCompletionTrigger(char? triggerChar)
            => triggerChar != null
                ? CompletionTrigger.CreateInsertionTrigger(triggerChar.Value)
                : CompletionTrigger.Invoke;

        private static bool MatchesFilterText(CompletionHelper helper, CompletionItem item, SourceText text, Dictionary<TextSpan, string> textSpanToText)
        {
            var filterText = GetFilterText(item, text, textSpanToText);

            return string.IsNullOrEmpty(filterText) || helper.MatchesPattern(item.FilterText, filterText, CultureInfo.InvariantCulture);
        }

        private static string GetFilterText(CompletionItem item, SourceText text, Dictionary<TextSpan, string> textSpanToText)
        {
            var textSpan = item.Span;
            if (!textSpanToText.TryGetValue(textSpan, out var filterText))
            {
                filterText = text.GetSubText(textSpan).ToString();
                textSpanToText[textSpan] = filterText;
            }

            return filterText;
        }
    }
    
    public class CompletionResult
    {
        public readonly CompleteData[] CompletionData;

        public CompletionResult(CompleteData[] completionData)
        {
            CompletionData = completionData;
        }
    }
    
    public class CompleteData
    {
        public readonly CompletionItem Item;
        public readonly CompletionService CompletionService;
        public readonly Document Document;

        public CompleteData(
            CompletionItem item,
            CompletionService completionService,
            Document document)
        {
            Item = item;
            CompletionService = completionService;
            Document = document;
        }
    }
    
}