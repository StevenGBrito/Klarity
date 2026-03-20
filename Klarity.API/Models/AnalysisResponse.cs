namespace Klarity.API.Models;

public sealed class AnalysisResponse
{
    public bool Success { get; set; }
    public string? Analysis { get; set; }
    public string? Error { get; set; }
}
