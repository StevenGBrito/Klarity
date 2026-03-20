using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Klarity.API.Services;

/// <summary>
/// Servicio de análisis de seguridad 100% local utilizando Ollama.
/// Privacidad total, sin límites de uso y gratuito.
/// </summary>
public class OllamaService
{
    private readonly HttpClient _http;
    private const string OllamaEndpoint = "http://localhost:11434/api/generate";
    private const string DefaultModel = "phi3";

    public OllamaService()
    {
        _http = new HttpClient();
        _http.Timeout = TimeSpan.FromMinutes(2);
    }

    public async Task<string> AnalyzeVulnerabilityAsync(string vulnerabilityType, string sourceCode)
    {
        var prompt = $"""
            Actúa como un experto en ciberseguridad y análisis estático de código (SAST).
            Analiza el siguiente código fuente para la vulnerabilidad: {vulnerabilityType}.
            
            CÓDIGO:
            ```csharp
            {sourceCode}
            ```
            
            REPORTA (BREVE):
            1. ¿Por qué es vulnerable?
            2. Riesgo (qué puede pasar).
            3. Ejemplo de código corregido.
            
            Markdown, Español. Sin saludos ni explicaciones largas.
            """;

        try
        {
            var requestBody = new 
            { 
                model = DefaultModel, 
                prompt = prompt, 
                stream = false,
                options = new { num_predict = 500, temperature = 0.3 }
            };
            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            var response = await _http.PostAsync(OllamaEndpoint, content);

            if (!response.IsSuccessStatusCode)
            {
                return $"""
                    ### ⚠️ Error de Conexión con Ollama
                    El servicio local de Ollama respondió con error ({response.StatusCode}).
                    Asegúrate de que Ollama esté ejecutándose (`ollama serve`).
                    """;
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<OllamaResponse>(jsonResponse);

            return result?.Response ?? "Error: Ollama no devolvió ninguna respuesta.";
        }
        catch (HttpRequestException)
        {
            return """
                ### ❌ Ollama no detectado
                No se pudo conectar con la API de Ollama en `http://localhost:11434`.
                
                **Instrucciones para activar:**
                1. Asegúrate de que Ollama esté instalado y abierto.
                2. Verifica que el modelo esté descargado: `ollama pull llama3`.
                """;
        }
        catch (Exception ex)
        {
            return $"### Error inesperado en el servicio de IA local\n{ex.Message}";
        }
    }

    private class OllamaResponse
    {
        [JsonPropertyName("response")] public string Response { get; set; } = string.Empty;
    }
}
