# Klarity - Static Analysis Tool Walkthrough

Hemos completado el desarrollo de **Klarity**, su herramienta de análisis estático de código C#.

## Características Implementadas

1.  **Integración con Roslyn**: Extracción de AST y compilación en memoria.
2.  **Grafo de Flujo de Control (CFG)**: Construcción de grafos a partir del código fuente para analizar rutas de ejecución.
3.  **Análisis de Flujo de Datos (DFA)**: Motor iterativo de punto fijo (`FixedPointEngine`) para propagar estados.
4.  **Taint Analysis**: Detección de flujo de datos inseguros desde fuentes (`Console.ReadLine`) hasta sumideros (`ExecuteSql`).
5.  **Interfaz WPF Moderna**: Aplicación con tema oscuro, estilos personalizados y visualización clara de resultados.
6.  **Experiencia de Usuario**: Ventana sin bordes con controles personalizados y layout optimizado.
7.  **Sugerencias Inteligentes**: Klarity ahora no solo detecta, sino que sugiere cómo corregir las vulnerabilidades (ej. usar consultas parametrizadas).
8.  **Análisis en Tiempo Real**: El motor detecta vulnerabilidades automáticamente mientras escribes código, sin necesidad de pulsar botones.
9.  **Visualización Avanzada**: Las líneas de código vulnerables se resaltan en rojo directamente en el editor, y al hacer clic en una alerta, la vista se desplaza automáticamente al error.
10. **Asistente de IA**: Capacidad de consultar a un experto virtual (IA) para obtener explicaciones detalladas y parches de código seguros para cada vulnerabilidad.
11. **Idioma Español**: Interfaz y mensajes totalmente localizados al español para mayor accesibilidad.
12. **Corrección de Sintaxis**: Detecta errores gramaticales en el código (falta de `;`, paréntesis, typos) y sugiere correcciones antes de pasar al análisis de seguridad.

## Cómo Ejecutar

1.  Abra la solución `Klarity.sln` en Visual Studio o ejecute desde la terminal:
    ```powershell
    cd Klarity\Klarity.UI
    dotnet run
    ```
2.  En la ventana de Klarity, haga clic en "Load C# File".
3.  Seleccione el archivo de prueba generado: `C:\Users\steve\.gemini\antigravity\scratch\Klarity\VulnerableCode.cs`.
4.  Observe el resultado en el panel inferior. Debería indicar una vulnerabilidad en la línea donde se usa `input` directamente en `ExecuteSql`.

## Archivos Clave
- [FixedPointEngine.cs](file:///C:/Users/steve/.gemini/antigravity/scratch/Klarity/Klarity.Core/DFA/FixedPointEngine.cs): El motor central de análisis.
- [TaintAnalysis.cs](file:///C:/Users/steve/.gemini/antigravity/scratch/Klarity/Klarity.Core/Taint/TaintAnalysis.cs): Lógica específica de seguridad.
- [CfgBuilder.cs](file:///C:/Users/steve/.gemini/antigravity/scratch/Klarity/Klarity.Core/CFG/CfgBuilder.cs): Constructor del grafo de control.
