# Plan de Integración de IA: Klarity

## Objetivo
Potenciar Klarity integrando un Modelo de Lenguaje (LLM) para que actúe como un experto en seguridad, explicando las vulnerabilidades y generando parches de código seguros.

## Arquitectura Propuesta

### 1. Capa de Abstracción (Klarity.Core)
- **Interfaz `IAIService`**:
    ```csharp
    Task<string> GetRemediationAsync(string vulnerabilityCode, string errorType);
    Task<string> ExplainVulnerabilityAsync(string errorType);
    ```

### 2. Implementación (Klarity.AI - Nuevo Namespace/Folder)
Podemos ofrecer dos implementaciones:
- **`OpenAIService`**: Conecta con la API de OpenAI (GPT-4/3.5). Requiere API Key.
- **`MockAIService`**: (Por defecto) Simula respuestas inteligentes basadas en patrones para demostración sin costo.

### 3. Integración UI (Klarity.UI)
- **Nuevo Panel "AI Assistant"**:
    - Se mostrará al seleccionar una vulnerabilidad.
    - Botón **"Ask AI to Fix"**.
    - Área de texto para mostrar la explicación y el código corregido generado por la IA.

## Flujo de Usuario
1.  Usuario selecciona una vulnerabilidad (ej. SQL Injection) en la lista.
2.  Clic en botón "Explain with AI".
3.  Klarity envía el fragmento de código inseguro al servicio.
4.  La IA responde: "Este código es vulnerable porque concatena strings... Aquí tienes la versión segura usando parámetros: ..."
5.  Se muestra la respuesta en la interfaz.
