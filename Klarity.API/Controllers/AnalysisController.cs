using Microsoft.AspNetCore.Mvc;
using Klarity.API.Models;
using Klarity.API.Services;

namespace Klarity.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnalysisController : ControllerBase
{
    private readonly ILogger<AnalysisController> _logger;
    private readonly GeminiService _gemini;

    public AnalysisController(ILogger<AnalysisController> logger, GeminiService gemini)
    {
        _logger = logger;
        _gemini = gemini;
    }

    /// <summary>
    /// Analiza una vulnerabilidad. Usa Gemini si hay API key, de lo contrario respuestas predefinidas.
    /// POST /api/analysis
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Analyze([FromBody] AnalysisRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.VulnerabilityType))
            return BadRequest(new AnalysisResponse { Success = false, Error = "VulnerabilityType es requerido." });

        _logger.LogInformation("Analizando: {Type} | Gemini activo: {Active}", 
            request.VulnerabilityType, _gemini.IsConfigured);

        try
        {
            string analysis;

            if (_gemini.IsConfigured)
            {
                // ── Modo IA Real (Gemini) ──────────────────────────────────
                var prompt = GeminiService.BuildPrompt(
                    request.VulnerabilityType,
                    request.SourceCode);

                analysis = await _gemini.GenerateAsync(prompt);
            }
            else
            {
                // ── Modo Fallback (predefinido) ────────────────────────────
                await Task.Delay(400); // simular latencia
                analysis = BuildFallbackAnalysis(request.VulnerabilityType);
            }

            return Ok(new AnalysisResponse { Success = true, Analysis = analysis });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al llamar a Gemini");
            return Ok(new AnalysisResponse
            {
                Success  = true,
                Analysis = BuildFallbackAnalysis(request.VulnerabilityType) +
                           $"\n\n> ⚠️ *Gemini no disponible: {ex.Message}*"
            });
        }
    }

    /// <summary>Endpoint de salud — verifica si Gemini está configurado.</summary>
    [HttpGet("health")]
    public IActionResult Health() =>
        Ok(new
        {
            Status       = "OK",
            GeminiActive = _gemini.IsConfigured,
            Timestamp    = DateTime.UtcNow,
            Version      = "2.0"
        });

    // ─── Respuestas de respaldo ────────────────────────────────────────────

    private static string BuildFallbackAnalysis(string type)
    {
        if (type.Contains("SQL", StringComparison.OrdinalIgnoreCase))
            return """
                ### 🔴 Inyección SQL (Modo offline)

                **Explicación:** Concatenar input del usuario en SQL permite manipular la consulta.

                **Código seguro:**
                ```csharp
                using var cmd = new SqlCommand("SELECT * FROM Users WHERE Name = @n", conn);
                cmd.Parameters.AddWithValue("@n", username);
                ```
                > 💡 Configura tu clave de Gemini en `appsettings.json` para análisis con IA real.
                """;

        if (type.Contains("Path", StringComparison.OrdinalIgnoreCase) ||
            type.Contains("Traversal", StringComparison.OrdinalIgnoreCase))
            return """
                ### 🔴 Path Traversal (Modo offline)

                **Explicación:** El atacante puede navegar fuera del directorio permitido.

                **Código seguro:**
                ```csharp
                var full = Path.GetFullPath(Path.Combine(baseDir, userInput));
                if (!full.StartsWith(baseDir)) throw new UnauthorizedAccessException();
                ```
                > 💡 Configura tu clave de Gemini para análisis con IA real.
                """;

        if (type.Contains("XSS", StringComparison.OrdinalIgnoreCase))
            return """
                ### 🟡 XSS (Modo offline)

                **Código seguro:**
                ```csharp
                Response.Write(HtmlEncoder.Default.Encode(userInput));
                ```
                > 💡 Configura tu clave de Gemini para análisis con IA real.
                """;

        return $"### ⚠️ {type}\n\nValida toda entrada del usuario.\n\n> 💡 Configura Gemini para análisis detallado.";
    }
}
