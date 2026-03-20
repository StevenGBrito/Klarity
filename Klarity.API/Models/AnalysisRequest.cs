namespace Klarity.API.Models;

public sealed class AnalysisRequest
{
    public string? VulnerabilityType { get; set; }
    public string? SourceCode { get; set; }
}
