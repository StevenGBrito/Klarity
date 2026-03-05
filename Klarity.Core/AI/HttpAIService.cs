using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;

namespace Klarity.Core.AI;

/// <summary>
/// Cliente HTTP que se conecta a la API de Klarity (Klarity.API) para análisis de vulnerabilidades.
/// </summary>
public class HttpAIService : IAIService
{
    private readonly HttpClient _http;
    private const string ApiBaseUrl = "http://localhost:5050";

    public HttpAIService()
    {
        _http = new HttpClient { BaseAddress = new Uri(ApiBaseUrl) };
        _http.Timeout = TimeSpan.FromSeconds(15);
    }

    public async Task<string> AnalyzeVulnerabilityAsync(string vulnerabilityType, string sourceCode)
    {
        try
        {
            var request = new
            {
                VulnerabilityType = vulnerabilityType,
                SourceCode = sourceCode
            };

            var response = await _http.PostAsJsonAsync("/api/analysis", request);

            if (!response.IsSuccessStatusCode)
                return $"### Error de API\nEl servidor respondió con: {response.StatusCode}";

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ApiResponse>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return result?.Analysis
                ?? "### Error\nLa API no devolvió un análisis válido.";
        }
        catch (HttpRequestException)
        {
            return """
                ### ❌ No se pudo conectar con la API

                La API de Klarity no está disponible en `http://localhost:5050`.

                **Para iniciarla:**
                ```
                cd Klarity.API
                dotnet run
                ```
                """;
        }
        catch (TaskCanceledException)
        {
            return "### ⏱️ Tiempo de espera agotado\nLa API tardó demasiado en responder. Verifique que esté en ejecución.";
        }
        catch (Exception ex)
        {
            return $"### Error inesperado\n{ex.Message}";
        }
    }

    // DTO interno para deserializar la respuesta
    private record ApiResponse(bool Success, string Analysis, string? Error);
}
