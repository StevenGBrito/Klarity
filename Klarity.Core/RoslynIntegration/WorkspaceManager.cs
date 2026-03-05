using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Klarity.Core.RoslynIntegration;

public class WorkspaceManager
{
    public Compilation? Compilation { get; private set; }
    public List<SyntaxTree> SyntaxTrees { get; private set; } = new();

    public void LoadSourceCode(string sourceCode)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(SourceText.From(sourceCode));
        SyntaxTrees.Add(syntaxTree);
        UpdateCompilation();
    }

    public void LoadTraverseDirectory(string directoryPath)
    {
        var files = Directory.GetFiles(directoryPath, "*.cs", SearchOption.AllDirectories);
        foreach (var file in files)
        {
            var text = File.ReadAllText(file);
            var syntaxTree = CSharpSyntaxTree.ParseText(SourceText.From(text));
            SyntaxTrees.Add(syntaxTree);
        }
        UpdateCompilation();
    }

    private void UpdateCompilation()
    {
        Compilation = CSharpCompilation.Create("KlarityAnalysis")
            .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
            .AddSyntaxTrees(SyntaxTrees);
    }
}
