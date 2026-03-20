using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Klarity.Core.Quality;

/// <summary>
/// Analizador de calidad de código para C#.
/// Detecta: Catch vacíos, anidamiento profundo, concatenación en bucles y convenciones de nombres.
/// </summary>
public class QualityAnalyzer : CSharpSyntaxWalker
{
    public List<AnalysisResult> Issues { get; } = new();

    public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
    {
        // 1. Long methods check
        var lineSpan = node.GetLocation().GetLineSpan();
        int lines = lineSpan.EndLinePosition.Line - lineSpan.StartLinePosition.Line;
        if (lines > 40)
        {
            Issues.Add(new AnalysisResult(
                $"Método '{node.Identifier.Text}' muy largo ({lines} líneas).",
                $"Línea {lineSpan.StartLinePosition.Line + 1}",
                "Refactorice el método en funciones más pequeñas.",
                "Medium",
                lineSpan.StartLinePosition.Line + 1));
        }

        base.VisitMethodDeclaration(node);
    }

    public override void VisitCatchClause(CatchClauseSyntax node)
    {
        // 2. Empty catch block
        if (node.Block.Statements.Count == 0)
        {
            var lineSpan = node.GetLocation().GetLineSpan();
            Issues.Add(new AnalysisResult(
                "Bloque catch vacío detectado.",
                $"Línea {lineSpan.StartLinePosition.Line + 1}",
                "No ignore las excepciones. Registre el error o reláncelo.",
                "High",
                lineSpan.StartLinePosition.Line + 1));
        }

        base.VisitCatchClause(node);
    }

    public override void VisitClassDeclaration(ClassDeclarationSyntax node)
    {
        // 3. PascalCase for classes
        var className = node.Identifier.Text;
        if (char.IsLower(className[0]))
        {
            var lineSpan = node.GetLocation().GetLineSpan();
            Issues.Add(new AnalysisResult(
                $"Nombre de clase '{className}' no cumple PascalCase.",
                $"Línea {lineSpan.StartLinePosition.Line + 1}",
                $"Renombre la clase a '{char.ToUpper(className[0])}{className.Substring(1)}'.",
                "Low",
                lineSpan.StartLinePosition.Line + 1));
        }

        base.VisitClassDeclaration(node);
    }

    public override void VisitAssignmentExpression(AssignmentExpressionSyntax node)
    {
        // 4. Strings in loops (+ or +=)
        if (node.OperatorToken.IsKind(SyntaxKind.PlusEqualsToken))
        {
            if (IsInsideLoop(node))
            {
                var lineSpan = node.GetLocation().GetLineSpan();
                Issues.Add(new AnalysisResult(
                    "Concatenación de strings en bucle.",
                    $"Línea {lineSpan.StartLinePosition.Line + 1}",
                    "Use StringBuilder para optimizar el rendimiento.",
                    "Medium",
                    lineSpan.StartLinePosition.Line + 1));
            }
        }
        base.VisitAssignmentExpression(node);
    }

    public override void VisitIfStatement(IfStatementSyntax node) { CheckNesting(node); base.VisitIfStatement(node); }
    public override void VisitForStatement(ForStatementSyntax node) { CheckNesting(node); base.VisitForStatement(node); }
    public override void VisitForEachStatement(ForEachStatementSyntax node) { CheckNesting(node); base.VisitForEachStatement(node); }
    public override void VisitWhileStatement(WhileStatementSyntax node) { CheckNesting(node); base.VisitWhileStatement(node); }

    private void CheckNesting(SyntaxNode node)
    {
        int depth = 0;
        var parent = node.Parent;
        while (parent != null)
        {
            if (parent is IfStatementSyntax || parent is ForStatementSyntax || 
                parent is ForEachStatementSyntax || parent is WhileStatementSyntax)
            {
                depth++;
            }
            parent = parent.Parent;
        }

        if (depth >= 3)
        {
            var lineSpan = node.GetLocation().GetLineSpan();
            Issues.Add(new AnalysisResult(
                "Nivel de anidamiento excesivo (> 3).",
                $"Línea {lineSpan.StartLinePosition.Line + 1}",
                "Refactorice el código para reducir la complejidad cognitiva.",
                "Medium",
                lineSpan.StartLinePosition.Line + 1));
        }
    }

    private bool IsInsideLoop(SyntaxNode node)
    {
        var parent = node.Parent;
        while (parent != null)
        {
            if (parent is ForStatementSyntax || parent is ForEachStatementSyntax || parent is WhileStatementSyntax)
                return true;
            parent = parent.Parent;
        }
        return false;
    }
}
