# Documentación Completa del Proyecto Klarity 🛡️

Este documento consolida toda la información técnica y de usuario del proyecto Klarity.

---

## 1. Información General

**Nombre:** Klarity
**Versión:** 2.0.0
**Fecha de Entrega:** 5 de Marzo, 2026
**Tecnologías:** C#, .NET 8, Roslyn, WPF, ASP.NET Core, Google Gemini 1.5 Flash.

---

## 2. Resumen Ejecutivo

**Klarity** es una herramienta de análisis estático de código (SAST) avanzada que detecta vulnerabilidades de seguridad en tiempo real. Utiliza el poder del compilador **Roslyn** para realizar análisis semánticos y de flujo de datos, y lo combina con la inteligencia de **Gemini 1.5** para proporcionar remediaciones inteligentes.

---

## 3. Funcionalidades Detalladas

### A. Detección de Vulnerabilidades
- **Inyección SQL**: Identifica concatenaciones peligrosas en consultas a bases de datos.
- **Path Traversal**: Detecta accesos inseguros al sistema de archivos mediante inputs de usuario.
- **XSS (Cross-Site Scripting)**: Alerta sobre la renderización de datos no saneados en salidas de respuesta.

### B. Análisis de Proyecto Completo 📂
- **Escaneo Masivo**: Al abre una carpeta, Klarity escanea automáticamente todos los archivos `.cs` de forma recursiva.
- **Resultados Consolidados**: Todos los hallazgos se muestran en una única lista, indicando el archivo de origen.
- **Navegación Inteligente**: Al hacer clic en un resultado, el editor cambia automáticamente al archivo correspondiente y se desplaza a la línea vulnerable.

### C. Análisis de Flujo de Datos (Taint Analysis)
El motor de Klarity rastrea el origen de los datos (*Sources*) hasta los puntos de ejecución peligrosos (*Sinks*), asegurando que cualquier dato que provenga del exterior sea validado antes de su uso.

### D. Asistente con IA
Integración con **Gemini 1.5 Flash** para:
- Explicar detalladamente la naturaleza de la vulnerabilidad encontrada.
- Proporción de parches de código seguros y mejores prácticas (ej. parametrización).
- Sugerencias de remediación contextuales al código del usuario.

---

## 4. Arquitectura del Proyecto

La solución se divide en tres proyectos modulares:

1.  **Klarity.UI (WPF)**:
    - Interfaz de usuario premium con tema oscuro.
    - Editor enriquecido con numeración de líneas y resaltado de vulnerabilidades.
    - Resultados interactivos con scroll automático.
2.  **Klarity.Core (.NET 8)**:
    - Motor de análisis basado en Roslyn.
    - Generador de Grafos de Flujo de Control (CFG).
    - Cliente de comunicación con la API de IA.
3.  **Klarity.API (ASP.NET Core)**:
    - Backend que gestiona las peticiones a los modelos de lenguaje de Google.
    - Capa de abstracción para el análisis dinámico de código.

---

## 5. Walkthrough (Guía de Uso)

### Instalación y Configuración
1. Clonar el repositorio.
2. Abrir `Klarity.sln` en Visual Studio 2022 o superior.
3. Configurar la `ApiKey` de Gemini en `Klarity.API/appsettings.json`.

### Ejecución
1. Iniciar el proyecto `Klarity.API` (Puerto 5050).
2. Iniciar el proyecto `Klarity.UI`.
3. Cargar un archivo de prueba. Klarity analizará el código automáticamente al escribir o cargar.

---

## 6. Resultados de Verificación

El sistema ha sido probado con diversos casos de uso:
- ✅ Detección exitosa de `SELECT * FROM Users WHERE Id = '` + input + `'`.
- ✅ Detección de `File.ReadAllText(input)`.
- ✅ Generación de respuestas coherentes por parte de la IA de Gemini.

---

## 7. Conclusión

Klarity representa un avance significativo en las herramientas de seguridad asistidas por IA para desarrolladores .NET. Su arquitectura desacoplada permite escalar a nuevas reglas de análisis y modelos de lenguaje de forma sencilla.
