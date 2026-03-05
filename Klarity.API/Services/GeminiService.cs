using System.Text;
using System.Text.Json;

namespace Klarity.API.Services;

/// <summary>
/// Llama a Google Gemini API (gemini-1.5-flash).
/// Clave gratuita: https://aistudio.google.com/app/apikey
/// </summary>
public sealed class GeminiService(IConfiguration config)
{
    private static readonly HttpClient Http = new();
    private readonly string _apiKey = config["Gemini:ApiKey"] ?? string.Empty;
    private readonly string _model  = config["Gemini:Model"]  ?? "gemini-1.5-flash";
    private const string Base = "https://generativelanguage.googleapis.com/v1beta/models";

    public bool IsConfigured => !string.IsNullOrWhiteSpace(_apiKey);

    public async Task<string> GenerateAsync(string prompt)
    {
        var url     = $"{Base}/{_model}:generateContent?key={_apiKey}";
        var payload = JsonSerializer.Serialize(new
        {
            contents         = new[] { new { parts = new[] { new { text = prompt } } } },
            generationConfig = new { temperature = 0.2, maxOutputTokens = 2048 }
        });

        var response = await Http.PostAsync(url, new StringContent(payload, Encoding.UTF8, "application/json"));
        var body     = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new Exception($"Gemini {(int)response.StatusCode}: {body}");

        using var doc = JsonDocument.Parse(body);
        return doc.RootElement
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString() ?? "(vacío)";
    }

    public static string BuildPrompt(string vulnType, string code) => $"""
        Eres un experto senior en seguridad de aplicaciones (AppSec) y auditor de código C#. 
        Analiza la siguiente vulnerabilidad detectada por Klarity y responde en español usando Markdown elegante.

        ---
        ### 🛡️ Reporte de Vulnerabilidad: {vulnType}
        
        **Contexto del Código:**
        ```csharp
        {code}
        ```

        **Tu tarea:**
        1. **Análisis Técnico**: Explica exactamente qué está ocurriendo y por qué es peligroso bajo el contexto del código proporcionado.
        2. **Vector de Ataque**: Describe un escenario realista de cómo un atacante podría explotar esto.
        3. **Remediación (Código Seguro)**: Proporciona el bloque de código corregido usando las mejores prácticas de .NET (ej. Parametrización, Encriptación, Validación).
        4. **Impacto y Severidad**: Clasifica la severidad (Baja, Media, Alta, Crítica) y menciona la categoría OWASP Top 10 correspondiente.

        Responde de forma concisa pero profesional.
        """;
}
