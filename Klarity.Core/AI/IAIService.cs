namespace Klarity.Core.AI;

public interface IAIService
{
    Task<string> AnalyzeVulnerabilityAsync(string vulnerabilityType, string sourceCode);
}
