namespace Klarity.Core.AI;

public class MockAIService : IAIService
{
    public async Task<string> AnalyzeVulnerabilityAsync(string vulnerabilityType, string sourceCode)
    {
        // Simulamos el "tiempo de pensamiento" de una IA real
        await Task.Delay(1200);

        if (vulnerabilityType.Contains("SQL"))
            return SqlResponse();

        if (vulnerabilityType.Contains("Path") || vulnerabilityType.Contains("Traversal"))
            return PathTraversalResponse();

        if (vulnerabilityType.Contains("XSS") || vulnerabilityType.Contains("Cross-Site"))
            return XssResponse();

        if (vulnerabilityType.Contains("Sintaxis") || vulnerabilityType.Contains("Syntax"))
            return SyntaxResponse();

        return "### Análisis de IA\n\nLa IA no pudo determinar el contexto específico. Se recomienda revisar el flujo de datos y validar toda entrada del usuario antes de usarla en operaciones sensibles.";
    }

    // ----- Respuestas específicas por tipo -----

    private static string SqlResponse() => @"### 🔴 Análisis IA: Inyección SQL

**¿Por qué es peligroso?**
Concatenar la entrada del usuario directamente en una consulta SQL permite que un atacante inyecte comandos maliciosos. Por ejemplo, ingresar `' OR '1'='1` podría devolver todos los registros de la base de datos.

**Solución: Consultas Parametrizadas**
```csharp
// ❌ CÓDIGO VULNERABLE
string query = ""SELECT * FROM Users WHERE Name = '"" + username + ""'"";

// ✅ CÓDIGO SEGURO
string query = ""SELECT * FROM Users WHERE Name = @Name"";
using var command = new SqlCommand(query, connection);
command.Parameters.AddWithValue(""@Name"", username);
command.ExecuteReader();
```";

    private static string PathTraversalResponse() => @"### 🔴 Análisis IA: Path Traversal

**¿Por qué es peligroso?**
Si un usuario puede controlar la ruta de un archivo, podría acceder a archivos sensibles del sistema, por ejemplo: `../../etc/passwd` o `C:\Windows\System32\config\SAM`.

**Solución: Validar la ruta resultante**
```csharp
// ❌ CÓDIGO VULNERABLE
string content = File.ReadAllText(userInput);

// ✅ CÓDIGO SEGURO
string baseDir = @""C:\MiApp\Archivos"";
string fullPath = Path.GetFullPath(Path.Combine(baseDir, userInput));

if (!fullPath.StartsWith(baseDir, StringComparison.OrdinalIgnoreCase))
    throw new UnauthorizedAccessException(""Acceso denegado."");

string content = File.ReadAllText(fullPath);
```";

    private static string XssResponse() => @"### 🟡 Análisis IA: XSS (Cross-Site Scripting)

**¿Por qué es peligroso?**
Escribir datos del usuario sin sanitizar en la respuesta HTTP permite que un atacante inyecte scripts maliciosos en el navegador de otras personas (robo de cookies, redirecciones).

**Solución: Codificar la salida**
```csharp
// ❌ CÓDIGO VULNERABLE
Response.Write(""Hola, "" + username);

// ✅ CÓDIGO SEGURO
using System.Text.Encodings.Web;

string safeUsername = HtmlEncoder.Default.Encode(username);
Response.Write(""Hola, "" + safeUsername);
```";

    private static string SyntaxResponse() => @"### ⚠️ Análisis IA: Error de Sintaxis

**¿Qué ocurrió?**
El compilador encontró un error en la estructura del código que impide su compilación.

**Causas frecuentes:**
- Punto y coma faltante al final de una instrucción `;`
- Llave o paréntesis sin cerrar `}` o `)`
- Nombre de variable o método mal escrito
- Uso de una palabra reservada como identificador

**Consejo:** Revise la línea resaltada en rojo y las líneas inmediatamente anteriores.";
}
