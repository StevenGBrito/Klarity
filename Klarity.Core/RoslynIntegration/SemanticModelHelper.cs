using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Klarity.Core.RoslynIntegration;

public class SemanticModelHelper
{
    private readonly Compilation _compilation;

    public SemanticModelHelper(Compilation compilation)
    {
        _compilation = compilation;
    }

    public IMethodSymbol? GetMethodSymbol(MethodDeclarationSyntax methodSyntax)
    {
        var model = _compilation.GetSemanticModel(methodSyntax.SyntaxTree);
        return model.GetDeclaredSymbol(methodSyntax) as IMethodSymbol;
    }

    public ITypeSymbol? GetTypeSymbol(ExpressionSyntax expression)
    {
        var model = _compilation.GetSemanticModel(expression.SyntaxTree);
        return model.GetTypeInfo(expression).Type;
    }
    
    public SemanticModel GetSemanticModel(SyntaxTree tree)
    {
        return _compilation.GetSemanticModel(tree);
    }
}
