using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Klarity.Core.DFA;

namespace Klarity.Core.Taint;

/// <summary>
/// Fuentes de datos no confiables / Sources
/// </summary>
file static class Sources
{
    public static readonly string[] Methods =
    [
        "Console.ReadLine()",
        "Request.QueryString",
        "Request.Form",
        "Request.Params",
        "Request.Headers",
        "Request.Cookies",
        "ReadAsync",
        "args",
        "Environment.GetEnvironmentVariable"
    ];
}

/// <summary>
/// Métodos que limpian los datos / Sanitizers
/// </summary>
file static class Sanitizers
{
    public static readonly string[] Methods =
    [
        "HtmlEncoder.Encode",
        "HtmlEncode",
        "UrlEncoder.Encode",
        "UrlEncode",
        "GetFileName",
        "int.Parse",
        "long.Parse",
        "double.Parse",
        "bool.Parse",
        "Guid.Parse"
    ];
}

/// <summary>
/// Sumideros peligrosos con su tipo de vulnerabilidad / Sinks
/// </summary>
file static class Sinks
{
    public static readonly (string Method, string VulnType)[] Methods =
    [
        // SQL Injection
        ("ExecuteSql",           "SQL"),
        ("ExecuteQuery",         "SQL"),
        ("SqlCommand",           "SQL"),
        ("ExecuteNonQuery",      "SQL"),
        ("ExecuteScalar",        "SQL"),
        ("ExecuteReader",        "SQL"),
        
        // Path Traversal
        ("File.ReadAllText",     "PathTraversal"),
        ("File.WriteAllText",    "PathTraversal"),
        ("File.Open",            "PathTraversal"),
        ("File.ReadAllBytes",    "PathTraversal"),
        ("Directory.GetFiles",   "PathTraversal"),
        ("StreamReader",         "PathTraversal"),
        ("StreamWriter",         "PathTraversal"),
        
        // XSS
        ("Response.Write",       "XSS"),
        ("HttpResponse.Write",   "XSS"),
        ("Console.Write",        "XSS"),

        // Command Injection
        ("Process.Start",        "CommandInjection"),
        ("ShellExecute",         "CommandInjection")
    ];
}

public class TaintAnalysis : IDataFlowAnalysis<TaintState>
{
    public TaintState InitialState => new TaintState();
    public List<AnalysisResult> Vulnerabilities { get; } = new();

    public TaintState Merge(TaintState s1, TaintState s2)
    {
        return TaintState.Merge(s1, s2);
    }

    public TaintState Transfer(TaintState inputState, SyntaxNode node)
    {
        var newState = new TaintState(inputState.TaintedVariables);

        if (node is LocalDeclarationStatementSyntax localDecl)
        {
            foreach (var variable in localDecl.Declaration.Variables)
            {
                var varName = variable.Identifier.Text;
                if (variable.Initializer != null)
                {
                    if (IsSource(variable.Initializer.Value) || IsTainted(variable.Initializer.Value, inputState))
                        newState.TaintedVariables.Add(varName);
                    else
                        newState.TaintedVariables.Remove(varName);
                }
            }
        }
        else if (node is ExpressionStatementSyntax exprStmt)
        {
            if (exprStmt.Expression is AssignmentExpressionSyntax assignment)
            {
                var targetName = assignment.Left.ToString();
                if (IsSource(assignment.Right) || IsTainted(assignment.Right, inputState))
                    newState.TaintedVariables.Add(targetName);
                else
                    newState.TaintedVariables.Remove(targetName);
            }

            // Check for Sinks
            var (isSink, vulnType) = FindSink(exprStmt.Expression, inputState);
            if (isSink)
            {
                var location = node.GetLocation().GetLineSpan();
                var result = BuildResult(vulnType, location);
                if (!Vulnerabilities.Any(v => v.LineNumber == result.LineNumber && v.Message == result.Message))
                {
                    Vulnerabilities.Add(result);
                }
            }
        }

        return newState;
    }

    // ----- Private helpers -----

    private bool IsSource(ExpressionSyntax expr)
    {
        var text = expr.ToString();
        return Sources.Methods.Any(s => text.Contains(s));
    }

    private bool IsSanitized(ExpressionSyntax expr)
    {
        var text = expr.ToString();
        return Sanitizers.Methods.Any(s => text.Contains(s));
    }

    private (bool found, string type) FindSink(ExpressionSyntax expr, TaintState state)
    {
        var text = expr.ToString();
        foreach (var (method, vulnType) in Sinks.Methods)
        {
            if (!text.Contains(method)) continue;

            if (expr is InvocationExpressionSyntax invocation)
            {
                foreach (var arg in invocation.ArgumentList.Arguments)
                {
                    if (IsTainted(arg.Expression, state))
                        return (true, vulnType);
                }
            }
            // ObjectCreationExpression: new SqlCommand(query, conn)
            else if (expr is ObjectCreationExpressionSyntax objCreation
                     && objCreation.ArgumentList != null)
            {
                foreach (var arg in objCreation.ArgumentList.Arguments)
                {
                    if (IsTainted(arg.Expression, state))
                        return (true, vulnType);
                }
            }
        }
        return (false, string.Empty);
    }

    private bool IsTainted(ExpressionSyntax expr, TaintState state)
    {
        if (IsSanitized(expr)) return false;

        return expr switch
        {
            IdentifierNameSyntax id          => state.TaintedVariables.Contains(id.Identifier.Text),
            BinaryExpressionSyntax binary    => IsTainted(binary.Left, state) || IsTainted(binary.Right, state),
            ParenthesizedExpressionSyntax p  => IsTainted(p.Expression, state),
            InterpolatedStringExpressionSyntax interp =>
                interp.Contents.OfType<InterpolationSyntax>().Any(i => IsTainted(i.Expression, state)),
            MemberAccessExpressionSyntax member => IsTainted(member.Expression, state),
            _                                => false
        };
    }

    private static AnalysisResult BuildResult(string vulnType, FileLinePositionSpan loc)
    {
        var line = loc.StartLinePosition.Line + 1;
        var lineStr = $"Línea {line}";
        return vulnType switch
        {
            "SQL"           => new("Posible Inyección SQL: datos no confiables en consulta.",
                                   lineStr, "Use SqlCommand.Parameters.AddWithValue().", "High", line),
            "PathTraversal" => new("Posible Path Traversal: ruta construida con input del usuario.",
                                   lineStr, "Use Path.GetFullPath() y verifique el directorio base.", "High", line),
            "XSS"           => new("Posible XSS: datos del usuario escritos sin codificar.",
                                   lineStr, "Codifique con HtmlEncoder.Encode() antes de escribir.", "Medium", line),
            "CommandInjection" => new("Posible Command Injection: ejecución de proceso con input externo.",
                                   lineStr, "Valide estrictamente los argumentos antes de ejecutar.", "High", line),
            _               => new($"Vulnerabilidad '{vulnType}' detectada.",
                                   lineStr, "Valide la entrada del usuario.", "Medium", line)
        };
    }
}
