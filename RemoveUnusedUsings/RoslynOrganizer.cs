using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;

namespace RemoveUnusedUsings
{
    public class RoslynOrganizer : IOrganizer
    {
        private readonly ILogger logger;

        public RoslynOrganizer(ILogger logger)
        {
            this.logger = logger;
        }

        public void OrganizeSolution(string solutionPath)
        {
            var workspace = MSBuildWorkspace.Create();

            var originalSolution = workspace.OpenSolutionAsync(solutionPath).Result;
            var modifiedSolution = originalSolution;

            foreach (var projectId in modifiedSolution.ProjectIds)
            {
                var project = modifiedSolution.GetProject(projectId);
                foreach (var documentId in project.DocumentIds)
                {
                    while (true)
                    {
                        var document = modifiedSolution.GetDocument(documentId);

                        var root = document.GetSyntaxRootAsync().Result;
                        var usings = root.DescendantNodes().OfType<UsingDirectiveSyntax>().ToArray();

                        var semanticModel = document.GetSemanticModelAsync().Result;
                        var diagnostics = semanticModel.GetDiagnostics();
                        var unusedUsingsCount = diagnostics.Count(IsUnusedUsing);
                        if (unusedUsingsCount > 0)
                        {
                            logger.Log($"Document: {document.FilePath}. Usings count: {unusedUsingsCount}");
                        }
                        var unusedUsingError = diagnostics.FirstOrDefault(IsUnusedUsing);
                        if (unusedUsingError == null)
                        {
                            break;
                        }

                        var @using = usings.FirstOrDefault(u => u.Span == unusedUsingError.Location.SourceSpan);

                        modifiedSolution = document.WithSyntaxRoot(root.RemoveNode(@using, SyntaxRemoveOptions.KeepDirectives)).Project.Solution;
                    }
                }
            }

            workspace.TryApplyChanges(modifiedSolution);
        }

        private static bool IsUnusedUsing(Diagnostic d)
        {
            return string.Equals(d.Id, "CS8019", StringComparison.InvariantCultureIgnoreCase);
        }
    }
}