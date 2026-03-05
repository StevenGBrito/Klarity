# Documentación del Proyecto Klarity 🛡️

**Versión:** 2.0.0
**Fecha:** 26 de Febrero, 2026
**Desarrollado por:** Antigravity AI & Steve

---

## 1. Resumen Ejecutivo

**Klarity** es una herramienta de análisis estático de código (SAST) diseñada para identificar vulnerabilidades de seguridad en aplicaciones C# en tiempo real. Utilizando el compilador **Roslyn**, Klarity construye Grafos de Flujo de Control (CFG) y realiza análisis de flujo de datos (Taint Analysis) para detectar patrones inseguros como la Inyección SQL, Path Traversal y XSS. 

En esta versión 2.0, Klarity integra un **Asistente de IA Real** basado en **Google Gemini 1.5 Flash** a través de una API dedicada (`Klarity.API`), lo que permite explicaciones precisas y parches de código inteligentes.

---

## 2. Funcionalidades Implementadas ✅

### A. Core de Análisis
1.  **Integración con Roslyn**: Carga, parsea y compila código C# dinámicamente.
2.  **Grafo de Flujo de Control (CFG)**: Construcción de grafos para navegar por la lógica del código.
3.  **Análisis de Taint (Data Flow)**:
    *   Rastreo de datos "sucios" desde fuentes no confiables (`Console.ReadLine`, `Request.QueryString`, args) hasta sumideros peligrosos.
    *   Soporte para asignaciones, concatenación y paréntesis.
4.  **Multi-Vulnerabilidad**:
    *   ✅ Inyección SQL
    *   ✅ Path Traversal
    *   ✅ XSS - Cross-Site Scripting
5.  **Detección de Errores Sintácticos**: Diagnósticos de Roslyn en tiempo real incorporados en el editor.

### B. Interfaz de Usuario (UI)
1.  **Editor WPF Moderno**: Tema oscuro con resaltado de sintaxis básico y líneas vulnerables.
2.  **Numeración de Líneas**: El editor ahora incluye números de línea para facilitar la navegación.
3.  **Panel de Resultados**: Tarjetas con Severidad, Ubicación y el botón "Consultar IA".
4.  **Sincronización Automática**: Al seleccionar una vulnerabilidad, el editor se desplaza a la línea exacta.

### C. Asistente de IA (Gemini 1.5 Flash)
1.  **Análisis Dinámico**: Gemini analiza el fragmento de código específico y los sumideros detectados.
2.  **Explicación Detallada**: Describe por qué el código es vulnerable y qué impacto podría tener.
3.  **Sugerencia de Parche**: Proporciona un bloque de código seguro (ej. parametrización de consultas).
4.  **Arquitectura Cliente-Servidor**: La aplicación UI se comunica con `Klarity.API` para procesar las peticiones de IA.

---

## 3. Arquitectura del Sistema

Klarity se divide en tres componentes principales:

1.  **Klarity.UI**: Aplicación de escritorio (WPF) que funciona como el editor y visualizador de resultados.
2.  **Klarity.Core**: Biblioteca de lógica que contiene el motor de análisis Roslyn, CFG y Taint Analysis.
3.  **Klarity.API**: Servicio web (ASP.NET Core) que actúa como puente hacia los LLM (Gemini) y proporciona servicios de análisis remotos.

```
Klarity.sln
├── Klarity.Core/          # Motor de análisis
│   ├── AI/                # Cliente HttpAIService
│   ├── CFG/               # Constructor de Grafos
│   ├── DFA/               # Motor de Iteración
│   └── Taint/             # Reglas de Seguridad
├── Klarity.API/           # Servicio REST (Puerto 5050)
│   ├── Controllers/       # AnalysisController (Análisis con IA)
│   └── Services/          # GeminiService (Integración SDK)
└── Klarity.UI/            # Aplicación de Escritorio
```

---

## 4. Cómo Utilizar

1.  **Configurar Gemini**: Añada su `ApiKey` en `Klarity.API/appsettings.json`.
2.  **Iniciar API**: Ejecute `dotnet run` en el directorio `Klarity.API`.
3.  **Iniciar Klarity**: Ejecute la aplicación Klarity.UI.
4.  **Analizar**: Cargue un archivo `.cs` (ej. `VulnerableCode.cs`) y consulte los resultados en tiempo real.

---

## 5. Próximos Pasos (Hoja de Ruta) 🚀

- [ ] **Análisis Inter-procedural**: Seguir variables entre diferentes métodos y archivos.
- [ ] **Reglas Externas**: Cargar patrones de vulnerabilidad desde archivos JSON.
- [ ] **Soporte CLI**: Versión de terminal para integración con CI/CD.
- [ ] **Modo Offline**: Mejorar las respuestas predefinidas si no hay conexión a internet.
