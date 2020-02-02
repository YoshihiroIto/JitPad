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
    public class CodeCompleter : IDisposable
    {
        public CodeCompleter(ICompiler compiler)
        {
            _workspace = new AdhocWorkspace(MefHostServices.Create(MefHostServices.DefaultAssemblies));

            var projectInfo = ProjectInfo
                .Create(ProjectId.CreateNewId(), VersionStamp.Create(), "Project", "Project", LanguageNames.CSharp)
                .WithMetadataReferences(compiler.MetadataReferences);

            _project = _workspace.AddProject(projectInfo);

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

        public void Dispose()
        {
            _CancellationTokenSource?.Cancel();
            _CancellationTokenSource?.Dispose();
            _CancellationTokenSource = new CancellationTokenSource();

            _workspace.Dispose();
        }

        private readonly AdhocWorkspace _workspace;
        private Project _project;

        private CancellationTokenSource? _CancellationTokenSource;

        public async Task<CompleteData[]> CompleteAsync(string sourceCode, int position, char? triggerChar)
        {
            _CancellationTokenSource?.Cancel();
            _CancellationTokenSource?.Dispose();
            _CancellationTokenSource = new CancellationTokenSource();

            var sourceText = SourceText.From(sourceCode);
            var document = _project.AddDocument("File.cs", sourceText);

            _project = document.Project;

            try
            {
                var completionService = CompletionService.GetService(document);
                var completionTrigger = GetCompletionTrigger(triggerChar);

                var data = await completionService
                    .GetCompletionsAsync(document, position, completionTrigger, null, null, _CancellationTokenSource.Token)
                    .ConfigureAwait(false);

                if (data == null || data.Items == null)
                    return Array.Empty<CompleteData>();

                var helper = CompletionHelper.GetHelper(document);
                var textSpanToText = new Dictionary<TextSpan, string>();

                return data.Items
                    .Where(item => MatchesFilterText(helper, item, sourceText, textSpanToText))
                    .Distinct(x => x.DisplayText)
                    .Select(x => new CompleteData(x, completionService, document))
                    .ToArray();
            }
            catch (OperationCanceledException)
            {
                return Array.Empty<CompleteData>();
            }
            finally
            {
                _project = _project.RemoveDocument(document.Id);
            }
        }

        private static CompletionTrigger GetCompletionTrigger(char? triggerChar)
            => triggerChar != null
                ? CompletionTrigger.CreateInsertionTrigger(triggerChar.Value)
                : CompletionTrigger.Invoke;

        // ref: RoslynPad -- https://github.com/aelij/RoslynPad
        private static bool MatchesFilterText(CompletionHelper helper, CompletionItem item, SourceText text, Dictionary<TextSpan, string> textSpanToText)
        {
            var filterText = GetFilterText(item, text, textSpanToText);

            return string.IsNullOrEmpty(filterText) || helper.MatchesPattern(item.FilterText, filterText, CultureInfo.InvariantCulture);
        }

        private static string? GetFilterText(CompletionItem item, SourceText text, Dictionary<TextSpan, string> textSpanToText)
        {
            var textSpan = item.Span;

            if (textSpanToText.TryGetValue(textSpan, out var filterText) == false)
            {
                filterText = text.GetSubText(textSpan).ToString();
                textSpanToText[textSpan] = filterText;
            }

            return filterText;
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