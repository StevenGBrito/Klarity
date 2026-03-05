namespace Klarity.API.Models;

/// <summary>Cuerpo de la petición POST /api/analyze</summary>
public class AnalysisRequest
{
    /// <summary>Tipo de vulnerabilidad detectada (ej. "SQL", "XSS")</summary>
    public string VulnerabilityType { get; set; } = string.Empty;

    /// <summary>Fragmento de código fuente para contexto</summary>
    public string SourceCode { get; set; } = string.Empty;
}
