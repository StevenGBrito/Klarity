# Documentación Completa del Proyecto Klarity 🛡️

Este documento consolida toda la información técnica y de usuario de **Klarity v2.5.0**, la solución integral de seguridad y calidad para infraestructura crítica.

---

## 1. Información General

**Nombre:** Klarity
**Versión:** 2.5.0
**Fecha de Actualización:** 20 de Marzo, 2026
**Tecnologías:** C#, .NET 8, Roslyn, WPF, ASP.NET Core, **Ollama (Llama 3 - 100% Local)**.

---

## 2. Resumen Ejecutivo

**Klarity** es un ecosistema avanzado de **Shift-Left Security** y **Linting** diseñado para detectar vulnerabilidades y problemas de calidad en tiempo real. Combina la precisión determinista del compilador **Roslyn** con la inteligencia generativa local de **Llama 3**, garantizando la soberanía de los datos y la privacidad total bajo el estándar de EGEHID.

---

## 3. Funcionalidades Detalladas

### A. Detección de Vulnerabilidades (Seguridad) 🔒
- **Inyecciones**: SQL Injection, Command Injection (`Process.Start`).
- **Path Traversal**: Accesos inseguros al sistema de archivos.
- **XSS**: Detección de salidas no saneadas en web.
- **Taint Analysis Avanzado**: Rastreo de fuentes inseguras (`Request.Headers`, `Cookies`) hasta sumideros peligrosos.
- **Saneadores (Sanitizers)**: Reconoce automáticamente funciones de limpieza (`HtmlEncode`, `int.Parse`) para eliminar falsos positivos.

### B. Análisis de Calidad (Linting) ✨
- **Mantenibilidad**: Alertas por anidamiento excesivo (>3 niveles) y métodos demasiado extensos.
- **Rendimiento**: Identifica concatenación de strings en bucles sugiriendo `StringBuilder`.
- **Mejores Prácticas**: Detecta bloques `catch` vacíos y valida convenciones de nombres (PascalCase).

### C. Análisis de Flujo de Control (CFG) 📉
Generación de grafos complejos que soportan estructuras lógicas de negocio profundas:
- Bucles `foreach` y `for`.
- Sentencias `switch` y `if-else` anidados.
- Análisis de alcanzabilidad de código mediante puntos fijos.

### D. Asistente IA 100% Local 🤖
Integración con **Ollama** para:
- **Remediación Instantánea**: Generación de parches de código funcionales en < 2 segundos.
- **Explicación Contextual**: Diagnósticos detallados sin conexión a la nube.
- **Privacidad Total**: El código fuente nunca abandona la red corporativa.

---

## 4. Arquitectura del Proyecto

1.  **Klarity.Core**: Motor de análisis semántico, Taint Analysis, CFG y Quality Rules sobre el AST de Roslyn.
2.  **Klarity.API**: Backend en ASP.NET Core que orquestra la comunicación con Ollama y gestiona la ingeniería de prompts.
3.  **Klarity.UI**: Cliente WPF premium con editor dinámico, resaltado de líneas vulnerables y explorador de proyectos interactivo.

---

## 5. Guía de Inicio Rápido

### Requisitos
- .NET 8 SDK
- Ollama instalado y corriendo el modelo `llama3`.

### Ejecución
1. Iniciar **Klarity.API** (Backend de IA).
2. Iniciar **Klarity.UI** (Panel de Control).
3. Seleccionar "Cargar Proyecto" para un escaneo recursivo de todos los archivos `.cs`.

---

## 6. Resultados de Verificación (QA)

✅ **SQLi**: Detecta exitosamente `cmd.CommandText = "..." + input`.
✅ **Quality**: Identifica correctamente `catch { }` como riesgo alto.
✅ **Performance**: Flag en `s += item` dentro de `foreach`.
✅ **AI Patching**: Genera correcciones válidas parametrizadas para SQLi y XSS.

---

## 7. Conclusión

Klarity v2.5.0 redefine el análisis estático local, permitiendo a los desarrolladores de EGEHID escribir código seguro y eficiente sin comprometer la privacidad institucional.

---
*Klarity: La evolución de la ciberseguridad privada.*
