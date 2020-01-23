using System.Linq;
using System.Threading.Tasks;
using JitPad.Core.Processor;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Recommendations;
using Microsoft.CodeAnalysis.Text;

namespace JitPad.Core
{
    public class CodeCompleter
    {
        public async Task<CompleteItem[]> CompleteAsync(string sourceCode, int position)
        {
            var compiler = new Compiler();
            var semanticModel = compiler.MakeSemanticModel(sourceCode);

            var workspace = new AdhocWorkspace();
            var projectName = "Project";
            var projectId = ProjectId.CreateNewId();
            var versionStamp = VersionStamp.Create();
            var projectInfo = ProjectInfo.Create(projectId, versionStamp, projectName, projectName, LanguageNames.CSharp);
            var project = workspace.AddProject(projectInfo);

            workspace.AddDocument(project.Id, "dummy.cs", SourceText.From(sourceCode));

            var items = await Recommender.GetRecommendedSymbolsAtPositionAsync(semanticModel, position, workspace)
                .ConfigureAwait(false);

            return
                items
                    .Distinct(x => x.Name)
                    .OrderBy(x => x.Name)
                    .Select(x =>
                        new CompleteItem(
                            x.Name,
                            x.Name,
                            null
                        )).ToArray();
        }
    }

    public class CompleteItem
    {
        public readonly string Content;
        public readonly string Text;
        public readonly string Description;

        public CompleteItem(string content, string text, string description)
        {
            Content = content;
            Text = text;
            Description = description;
        }
    }
}