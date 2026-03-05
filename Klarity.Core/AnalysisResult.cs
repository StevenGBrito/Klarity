namespace Klarity.Core;

public record AnalysisResult(
    string Message,
    string Location,
    string Suggestion,
    string Severity,
    int    LineNumber = 0);
