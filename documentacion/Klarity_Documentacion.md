# Documentación del Proyecto Klarity 🛡️

**Versión:** 4.0.0 (Edición Local Ilimitada)
**Fecha:** 18 de Marzo, 2026
**Desarrollado por:** Antigravity AI & Steve

---

## 1. Resumen Ejecutivo

**Klarity** es una herramienta de análisis estático de código (SAST) diseñada para identificar vulnerabilidades de seguridad en aplicaciones C# en tiempo real. Utilizando el compilador **Roslyn**, Klarity construye Grafos de Flujo de Control (CFG) y realiza análisis de flujo de datos (Taint Analysis) para detectar patrones inseguros.

En esta versión 4.0, Klarity integra un **Asistente de IA 100% Local** basado en **Ollama (Llama 3)** a través de una API dedicada (`Klarity.API`), garantizando que el código nunca salga de la máquina del usuario y eliminando cualquier coste o límite de uso.

---

## 2. Funcionalidades Implementadas ✅

### A. Core de Análisis
1.  **Integración con Roslyn**: Carga, parsea y compila código C# dinámicamente.
2.  **Grafo de Flujo de Control (CFG)**: Construcción de grafos para navegar por la lógica del código.
3.  **Análisis de Taint (Data Flow)**: Rastreo de datos desde fuentes no confiables hasta sumideros peligrosos.
4.  **Multi-Vulnerabilidad**: Inyección SQL, Path Traversal y XSS.

### B. Interfaz de Usuario (UI)
1.  **Editor WPF Premium**: Tema oscuro con resaltado de sintaxis y numeración.
2.  **Panel de Resultados**: Tarjetas interactivas con el botón "Preguntar al Asistente IA".
3.  **Navegación Bi-direccional**: Sincronización entre hallazgos y líneas de código.

### C. Asistente de IA Local (Ollama)
1.  **Privacidad por Diseño**: Todo el procesamiento se realiza en local.
2.  **Explicación técnica**: Llama 3 analiza el contexto y explica el riesgo.
3.  **Parches Inteligentes**: Generación automática de código seguro para corregir la vulnerabilidad.
4.  **Sin Límites**: Uso ilimitado y gratuito garantizado.

---

## 3. Arquitectura del Sistema

1.  **Klarity.UI**: Aplicación de escritorio (WPF).
2.  **Klarity.Core**: Biblioteca de lógica (Roslyn + Taint Engine).
3.  **Klarity.API**: Servicio web local (ASP.NET Core) que conecta con Ollama.

---

## 4. Instrucciones de Inicio Rápido

1.  **Pre-requisitos**: Tener instalado Ollama (`ollama pull llama3`).
2.  **Servidor**: Ejecutar `dotnet run --project Klarity.API`.
3.  **Cliente**: Iniciar la aplicación Klarity.UI.

---

## 5. Hoja de Ruta (Roadmap) 🚀

- [ ] Soporte para análisis de proyectos multi-lenguaje.
- [ ] Integración con modelos más ligeros para hardware limitado (Phi-3).
- [ ] Exportación de reportes en PDF/JSON.
