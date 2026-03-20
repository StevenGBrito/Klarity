using Microsoft.AspNetCore.Mvc;
using Klarity.API.Models;
using Klarity.API.Services;

namespace Klarity.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnalysisController : ControllerBase
{
    private readonly ILogger<AnalysisController> _logger;
    private readonly OllamaService _ollama;

    public AnalysisController(ILogger<AnalysisController> logger, OllamaService ollama)
    {
        _logger = logger;
        _ollama = ollama;
    }

    /// <summary>
    /// Analiza una vulnerabilidad usando el Motor de Inteligencia Local Klarity.
    /// Sin límites de cuota, sin dependencia de internet.
    /// POST /api/analysis
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Analyze([FromBody] AnalysisRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.VulnerabilityType))
            return BadRequest(new AnalysisResponse { Success = false, Error = "VulnerabilityType es requerido." });

        _logger.LogInformation("Analizando vulnerabilidad: {Type} (Motor Local)", request.VulnerabilityType);

        try
        {
            // Ejecutar el motor de IA local (Ollama)
            var analysis = await _ollama.AnalyzeVulnerabilityAsync(request.VulnerabilityType, request.SourceCode ?? string.Empty);

            return Ok(new AnalysisResponse 
            { 
                Success = true, 
                Analysis = analysis 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error crítico durante el análisis local");
            
            return Ok(new AnalysisResponse
            {
                Success = false,
                Error = "Error interno del motor de análisis local."
            });
        }
    }

    /// <summary>Estado del motor de análisis.</summary>
    [HttpGet("health")]
    public IActionResult Health() =>
        Ok(new
        {
            Status      = "OK",
            Mode        = "Local-First",
            Engine      = "Ollama/Klarity",
            Timestamp   = DateTime.UtcNow,
            Version     = "4.0 (Ilimitada)"
        });
}
