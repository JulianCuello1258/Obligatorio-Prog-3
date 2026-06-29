# 🐝 BeeKeeperApp - Sistema de Gestión e Inteligencia Apícola

¡Bienvenido a **BeeKeeperApp**! Este sistema está diseñado para ayudar al **apicultor Matías Verges** a gestionar sus apiarios, colmenas, reinas, sanidad, producción de miel, tareas de mantenimiento y movimientos de trashumancia. Además, cuenta con un módulo de inteligencia climática y geográfica que evalúa la aptitud de los terrenos para instalar nuevos apiarios utilizando datos satelitales y de OpenStreetMap.

Este documento sirve como **guía e índice de archivos** para comprender la estructura del proyecto y saber dónde se gestiona cada función del trabajo diario en el campo.

---

## 🗺️ Guía de Módulos para el Apicultor

A continuación se detallan los módulos clave de la aplicación, asociando sus archivos fuente correspondientes para facilitar su localización:

### 1. Mapa e Instalación de Apiarios
* **Propósito**: Permite al apicultor buscar ubicaciones en el mapa y analizar la aptitud del terreno para la instalación de colmenas. Evalúa factores críticos como la presencia de cultivos melíferos (soja, girasol, cítricos), distancia a fuentes de agua dulce y el riesgo sanitario de *Nosema* (basado en la humedad ambiental histórica).
* **Vistas (Frontend)**:
  * [Mapa/Index.cshtml (Vista del Mapa)](file:///c:/Users/Usuario/Documents/GitHub/Obligatorio/Views/Mapa/Index.cshtml) — Panel visual y mapa interactivo.
  * [apiario-clima.js (Lógica del Clima)](file:///c:/Users/Usuario/Documents/GitHub/Obligatorio/wwwroot/js/apiario-clima.js) — Muestra badges de aptitud ("Estimado por Zona" / "Detectado por OSM") y calcula los semáforos de distancia a fuentes de agua.
* **Backend y APIs**:
  * [MapaController.cs](file:///c:/Users/Usuario/Documents/GitHub/Obligatorio/Controllers/MapaController.cs) y [ApiariosController.cs:Clima](file:///c:/Users/Usuario/Documents/GitHub/Obligatorio/Controllers/ApiariosController.cs#L20-L29) — Controlan los endpoints del mapa y la aptitud.
  * [WeatherService.cs](file:///c:/Users/Usuario/Documents/GitHub/Obligatorio/Services/WeatherService.cs) — Servicio central que consume Open-Meteo (clima actual e histórico de 180 días) y Overpass API (OSM) para agua y cultivos. Implementa caché en memoria de 24 horas para optimizar el rendimiento.

### 2. Gestión de Apiarios y Competencia
* **Propósito**: Alta, baja, modificación y listado de los apiarios físicos del productor (fijos o trasladables, ubicación en Uruguay, sección policial, etc.). Permite comparar el rendimiento productivo entre distintos apiarios.
* **Archivos Relacionados**:
  * [ApiariosController.cs](file:///c:/Users/Usuario/Documents/GitHub/Obligatorio/Controllers/ApiariosController.cs) — Controlador con el CRUD y la vista de comparación de producción.
  * [Apiario.cs (Modelo de Apiario)](file:///c:/Users/Usuario/Documents/GitHub/Obligatorio/Models/Entities/Apiario.cs) — Estructura de la entidad con sus propiedades.
  * [Views/Apiarios/](file:///c:/Users/Usuario/Documents/GitHub/Obligatorio/Views/Apiarios/) — Formularios de creación, edición, detalles y la tabla comparativa.

### 3. Registro de Colmenas y Control de Reinas
* **Propósito**: Permite llevar el inventario de las colmenas en cada apiario. Registra el tipo de colmena (Langstroth, Dadant), nivel de población (Fuerte, Media, Débil), temperamento (Mansa, Agresiva) y el estado de salud/presencia de la reina.
* **Archivos Relacionados**:
  * [ColmenasController.cs](file:///c:/Users/Usuario/Documents/GitHub/Obligatorio/Controllers/ColmenasController.cs) — Gestión de colmenas.
  * [Colmena.cs (Modelo de Colmena)](file:///c:/Users/Usuario/Documents/GitHub/Obligatorio/Models/Entities/Colmena.cs) — Entidad de colmena.
  * [Reina.cs (Modelo de Reina)](file:///c:/Users/Usuario/Documents/GitHub/Obligatorio/Models/Entities/Reina.cs) — Estado de la reina y fecha de nacimiento.
  * [Views/Colmenas/](file:///c:/Users/Usuario/Documents/GitHub/Obligatorio/Views/Colmenas/) — Vistas para registrar y auditar colmenas y sus reinas.

### 4. Sanidad Apícola e Inspecciones de Campo
* **Propósito**: Control de plagas y enfermedades (principalmente *Varroasis* y *Nosemosis*). El apicultor registra los síntomas detectados, tratamientos aplicados (ej. Ácido Oxálico) y programa las próximas dosis. Cuenta con **transcripción de voz** para facilitar la toma de notas en el campo con las manos ocupadas.
* **Archivos Relacionados**:
  * [SanidadController.cs](file:///c:/Users/Usuario/Documents/GitHub/Obligatorio/Controllers/SanidadController.cs) — Lógica de inspecciones sanitarias.
  * [Revision.cs (Modelo de Revisión)](file:///c:/Users/Usuario/Documents/GitHub/Obligatorio/Models/Entities/Revision.cs) — Registra la ficha clínica de la colmena.
  * [voiceTranscription.js](file:///c:/Users/Usuario/Documents/GitHub/Obligatorio/wwwroot/js/voiceTranscription.js) — Módulo JS de reconocimiento de voz (Speech-to-Text).
  * [Views/Sanidad/](file:///c:/Users/Usuario/Documents/GitHub/Obligatorio/Views/Sanidad/) — Vistas para el historial clínico y registro de inspecciones sanitarias.

### 5. Cosecha y Producción de Miel
* **Propósito**: Registra las extracciones de miel en kilogramos de cada colmena para evaluar la productividad individual y colectiva de los apiarios.
* **Archivos Relacionados**:
  * [ProduccionController.cs](file:///c:/Users/Usuario/Documents/GitHub/Obligatorio/Controllers/ProduccionController.cs) — Gestión de cosechas.
  * [Extraccion.cs (Modelo de Extracción)](file:///c:/Users/Usuario/Documents/GitHub/Obligatorio/Models/Entities/Extraccion.cs) — Registro de kilogramos y fecha.
  * [Views/Produccion/](file:///c:/Users/Usuario/Documents/GitHub/Obligatorio/Views/Produccion/) — Interfaz para registrar extracciones y ver estadísticas.

### 6. Agenda de Tareas Apícolas
* **Propósito**: Una agenda de tareas programadas para el mantenimiento del apiario (ej. "Cortar pasto", "Aplicar tratamiento contra Varroa", "Alimentar colmenas"). Se vincula a apiarios o colmenas específicas.
* **Archivos Relacionados**:
  * [TareasController.cs](file:///c:/Users/Usuario/Documents/GitHub/Obligatorio/Controllers/TareasController.cs) — Controlador de la agenda.
  * [Tarea.cs (Modelo de Tarea)](file:///c:/Users/Usuario/Documents/GitHub/Obligatorio/Models/Entities/Tarea.cs) — Entidad de la tarea (título, descripción, fecha programada, completada).
  * [Views/Tareas/](file:///c:/Users/Usuario/Documents/GitHub/Obligatorio/Views/Tareas/) — Tablero o listado de tareas pendientes y realizadas.

### 7. Trashumancia
* **Propósito**: Planificación y registro del traslado físico de colmenas de un apiario origen a un apiario destino, validando que el apiario de destino tenga la trashumancia habilitada y calculando la distancia del recorrido.
* **Archivos Relacionados**:
  * [TrashumanciaController.cs](file:///c:/Users/Usuario/Documents/GitHub/Obligatorio/Controllers/TrashumanciaController.cs) — Controlador de traslados.
  * [Trashumancia.cs (Modelo de Trashumancia)](file:///c:/Users/Usuario/Documents/GitHub/Obligatorio/Models/Entities/Trashumancia.cs) — Historial de traslados entre apiarios.
  * [Views/Trashumancia/](file:///c:/Users/Usuario/Documents/GitHub/Obligatorio/Views/Trashumancia/) — Vistas para planificar un nuevo traslado.

---

## 📂 Índice de Documentación y Planificaciones

Toda la documentación conceptual y de desarrollo técnico se encuentra centralizada en la carpeta [documentacion/](file:///c:/Users/Usuario/Documents/GitHub/Obligatorio/documentacion/):

* 📄 [documentacion/planes/Obligatorio - Ing. Software.docx](file:///c:/Users/Usuario/Documents/GitHub/Obligatorio/documentacion/planes/Obligatorio%20-%20Ing.%20Software.docx)
  * **Descripción**: Documento principal del obligatorio escolar/universitario. Contiene la especificación de requisitos de software, casos de uso, diagrama de clases, reglas de negocio y alcance del sistema apícola.
* 📝 [documentacion/planes/implementation_plan.md](file:///c:/Users/Usuario/Documents/GitHub/Obligatorio/documentacion/planes/implementation_plan.md)
  * **Descripción**: Plan de implementación técnica para la evaluación enriquecida climática y geográfica. Detalla los offsets de 10 días para Open-Meteo, umbrales empíricos de Nosema (días de humedad > 80%), caching de Overpass API y el fallback regional de cultivos.

---

## 🛠️ Índice General del Código Fuente

A continuación se detalla la ubicación de los archivos más importantes del proyecto para fines de desarrollo o mantenimiento técnico:

### Estructura del Backend (ASP.NET Core MVC)

| Archivo / Carpeta | Descripción |
| :--- | :--- |
| 🗄️ [Data/ApplicationDbContext.cs](file:///c:/Users/Usuario/Documents/GitHub/Obligatorio/Data/ApplicationDbContext.cs) | Contexto de Entity Framework Core para la persistencia en base de datos SQL Server. |
| 🌱 [Data/DbInitializer.cs](file:///c:/Users/Usuario/Documents/GitHub/Obligatorio/Data/DbInitializer.cs) | Carga de datos semilla iniciales para pruebas rápidas (apiarios, colmenas, tareas, tratamientos). |
| 🛡️ [Filters/SessionAuthAttribute.cs](file:///c:/Users/Usuario/Documents/GitHub/Obligatorio/Filters/SessionAuthAttribute.cs) | Filtro personalizado que valida que el usuario apicultor haya iniciado sesión antes de acceder al sistema. |
| ⚙️ [Program.cs](file:///c:/Users/Usuario/Documents/GitHub/Obligatorio/Program.cs) | Punto de entrada de la aplicación. Configura la inyección de dependencias (`IMemoryCache`, `WeatherService`, `DbContext`) y los middleware. |
| 🌍 [appsettings.json](file:///c:/Users/Usuario/Documents/GitHub/Obligatorio/appsettings.json) | Configuración general de la aplicación, incluyendo la cadena de conexión a SQL Server. |

### Modelos de Datos (`Models/Entities/`)

Representan los objetos de negocio del dominio de la apicultura:
* [Enums.cs](file:///c:/Users/Usuario/Documents/GitHub/Obligatorio/Models/Entities/Enums.cs) — Contiene los tipos y estados del negocio (Tipo de apiario, nivel de población, temperamento, salud de la reina, tipo de revisión, etc.).
* [Clima.cs](file:///c:/Users/Usuario/Documents/GitHub/Obligatorio/Models/Entities/Clima.cs) — Entidad para almacenar lecturas de clima específicas.
* [Exportacion.cs](file:///c:/Users/Usuario/Documents/GitHub/Obligatorio/Models/Entities/Exportacion.cs) — Registro y validación de exportaciones de miel.

### Frontend Estático (`wwwroot/`)

* 🎨 [wwwroot/css/beekeeper.css](file:///c:/Users/Usuario/Documents/GitHub/Obligatorio/wwwroot/css/beekeeper.css) — Estilos personalizados con colores y temática apícola (tonos miel, ámbar, verde naturaleza).
* 🪄 [wwwroot/js/animations.js](file:///c:/Users/Usuario/Documents/GitHub/Obligatorio/wwwroot/js/animations.js) — Micro-animaciones para mejorar la experiencia de usuario y hacer la interfaz dinámica.

---

## 🚀 Instrucciones para Iniciar el Sistema

1. **Configurar Base de Datos**:
   * Asegúrate de tener SQL Server instalado y corriendo localmente.
   * Modifica la cadena de conexión `DefaultConnection` en el archivo [appsettings.json](file:///c:/Users/Usuario/Documents/GitHub/Obligatorio/appsettings.json) si es necesario.
2. **Restaurar y Ejecutar**:
   * Ejecuta el comando en tu terminal dentro de la raíz del proyecto:
     ```bash
     dotnet run
     ```
3. **Acceso al Sistema**:
   * Abre tu navegador en la dirección local indicada por la consola (usualmente `https://localhost:7198` o similar).
   * Al iniciar, `DbInitializer` creará automáticamente la base de datos de pruebas con apiarios de ejemplo (Melilla, Canelones, Ruta 5) y datos de producción para comenzar de inmediato.
