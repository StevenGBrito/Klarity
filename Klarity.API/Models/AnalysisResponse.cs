namespace Klarity.API.Models;

/// <summary>Respuesta del endpoint POST /api/analyze</summary>
public class AnalysisResponse
{
    public bool Success { get; set; }
    public string Analysis { get; set; } = string.Empty;
    public string? Error { get; set; }
}
