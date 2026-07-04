# 🐝 Sanganos S.A. — Sistema de Gestión Apícola
## 📚 Guía de Evaluación para el Docente

¡Bienvenido al sistema **Sanganos S.A.**! Esta plataforma web interactiva está diseñada para resolver los desafíos cotidianos del apicultor, facilitando la administración de colmenas y automatizando las declaraciones institucionales. 

Este documento ha sido redactado como una **guía práctica de navegación** para orientarlo en la corrección, detallando qué hace cada sección y cómo probar sus características más importantes paso a paso sin profundizar en tecnicismos de código.

---

## 🚀 Cómo Iniciar la Aplicación

Para desplegar el sistema localmente y comenzar con la evaluación, siga estos sencillos pasos:

1. **Requisitos previos:** Asegúrese de tener instalado el SDK de .NET y una base de datos local (SQL Server Express).
2. **Ejecutar el Servidor:** Abra una consola en el directorio raíz del proyecto y ejecute:
   ```bash
   dotnet watch run
   ```
3. **Acceder a la Web:** Una vez iniciado, abra su navegador e ingrese a la dirección local provista en la terminal (por ejemplo, `https://localhost:7198`).
4. **Datos de Prueba Iniciales:** El sistema se autoinicializa con un juego de datos cargados de antemano (apiarios en Melilla, Ruta 5, colmenas activas, registros de sanidad y tareas pendientes) para que pueda probar la plataforma de inmediato sin necesidad de completar formularios vacíos desde cero.

---

## 🖥️ Recorrido por las Secciones de la Página

Al ingresar al sistema, encontrará un menú lateral de navegación rápido. A continuación, se detalla qué características buscar e interactuar en cada una de ellas:

### 🔒 1. Pantalla de Acceso (Login)
* **Qué hace:** Protege el acceso al panel del apicultor. 
* **Qué evaluar:** La pantalla cuenta con una estética moderna de efecto vidrio templado sobre un fondo temático apícola, diseñada para una carga visual fluida y responsiva.

### 📊 2. Panel Principal (Menú Principal)
* **Qué hace:** Es la central de control del apicultor.
* **Qué evaluar:**
  * **Tarjetas de Estadísticas:** Muestran de forma dinámica el número de apiarios totales, colmenas activas y tareas pendientes.
  * **Inspecciones Recientes:** Una tabla de auditoría clínica rápida que clasifica a las colmenas como "Saludable" o "Enferma" según los últimos reportes de campo.
  * **Acciones Rápidas:** Botones directos para agilizar las operaciones de uso frecuente.

### 🗺️ 3. Mapa de Exploración y Aptitud Geográfica
* **Qué hace:** Permite buscar y evaluar terrenos en Uruguay antes de instalar físicamente un apiario.
* **Qué evaluar:**
  * **Marcador Leaflet:** Haga clic en cualquier punto del mapa para mover el pin o ingrese coordenadas de latitud/longitud.
  * **Algoritmo de Aptitud Apícola:** El sistema evalúa automáticamente la viabilidad de la zona basándose en:
    * **Clima histórico (180 días):** Identifica heladas invernales severas, vientos excesivos o sequías.
    * **Entorno Geográfico:** Busca recursos hídricos naturales (lagunas, arroyos) y zonas de cultivos melíferos (soja, girasol, cítricos) en un radio de 3 km mediante mapas satelitales.
    * **Riesgo Sanitario:** Alerta si la humedad histórica favorece la aparición de parásitos o enfermedades.
  * **Semáforos de Ubicación:** Muestra de forma interactiva una puntuación final (Óptimo, Aceptable o No Recomendado) con badges verdes, amarillos y rojos según la idoneidad.

### 🛖 4. Gestión de Apiarios
* **Qué hace:** Administra los establecimientos de colmenas.
* **Qué evaluar:**
  * **Carga de Producción Acumulada:** Cada tarjeta de apiario muestra el volumen de miel total extraído de todas sus colmenas en tiempo real (ej. *Producido: 340,5 kg*).
  * **Autocompletado de Datos (Ubicación):** Al crear o editar un apiario, si selecciona una ubicación en el mapa, los campos de *Departamento*, *Sección Policial*, *Paraje/Localidad* y la sugerencia de *Trashumancia* se rellenan automáticamente con una animación de brillo dorado.
  * **Comparación de Apiarios:** Permite comparar el rendimiento neto en kilos de miel de todos los apiarios en una tabla organizada para identificar los terrenos más rentables.

### 🐝 5. Gestión de Colmenas
* **Qué hace:** Registra e inventaría las poblaciones de abejas.
* **Qué evaluar:**
  * **Registro Masivo (En Cantidad):** En el formulario de creación, defina los parámetros generales y asigne una cantidad (por ejemplo, registrar 30 colmenas juntas en un solo paso).
  * **Control de la Reina:** Active o desactive el checkbox de "Reina Presente". Si se desmarca, los campos de salud y fecha de nacimiento se deshabilitan y limpian de inmediato para evitar incoherencias.
  * **Filtros Avanzados:** Ordene el listado general por ID, Fecha de creación, Tipo, Nivel de población (Fuerte > Media > Débil) o por cantidad de miel producida.

### 🩺 6. Sanidad e Inspecciones Clínicas
* **Qué hace:** Historial médico de las colmenas y tratamientos aplicados.
* **Qué evaluar:**
  * **Registro de Revisión:** Permite realizar una inspección a una colmena específica o a un apiario completo.
  * **Transcripción de Voz a Texto:** Incluye un botón de micrófono para que el apicultor pueda dictar las notas de campo (enfermedades, observaciones) sin necesidad de escribir en el teclado (ideal para el trabajo con guantes).
  * **Reglas de Tratamiento:** Si se selecciona un medicamento, el sistema exige de forma obligatoria ingresar una fecha para la segunda dosis y bloquea fechas incoherentes en el futuro para la reina.

### 🍯 7. Registro de Cosechas (Producción)
* **Qué hace:** Registra las extracciones periódicas de miel.
* **Qué evaluar:**
  * **Formulario Adaptativo:** Permite alternar mediante botones rápidos si la cosecha se registra "Por Apiario" (producción global dividida equitativamente) o "Por Colmena" (rendimiento detallado individual).

### 🚚 8. Planificación de Trashumancia (Traslados)
* **Qué hace:** Gestiona la movilidad de colmenas para aprovechar floraciones estacionales.
* **Qué evaluar:**
  * **Cálculo de Distancia Automático:** Al seleccionar el apiario de origen y el de destino, la aplicación calcula la distancia vial real entre ambos puntos en kilómetros.
  * **Validación de Destino:** Impide el traslado si el apiario receptor no cuenta con la habilitación para recibir colmenas.

### 🚢 9. Exportación y Declaración Jurada (MGAP)
* **Qué hace:** Genera la documentación oficial para la venta y exportación de miel.
* **Qué evaluar:**
  * **Control de Stock en Exportación:** Al registrar una venta internacional, el sistema valida que no se supere la miel disponible en stock (evitando números negativos o desbordamientos).
  * **Declaración Jurada Oficial (Impresión y PDF):** Genera el formulario con el formato institucional del Ministerio de Ganadería, Agricultura y Pesca de Uruguay (MGAP).
  * **Formato A4 Limpio:** Al imprimir (`Ctrl + P` o botón Imprimir), el documento se reorganiza en hojas independientes respetando los márgenes A4, oculta controles web interactivos y añade la firma de exoneración del timbre profesional (Sección D) al pie de la segunda página.

### 📅 10. Agenda de Tareas de Mantenimiento
* **Qué hace:** Planificador de actividades del apicultor.
* **Qué evaluar:**
  * Permite agendar tareas preventivas y marcarlas como completadas mediante un sistema ágil de tarjetas transparentes.

---

## 🎨 Características de Diseño a Observar

Durante su corrección, notará un enfoque prioritario en la experiencia de usuario (UX) mediante:
* **Diseño Glassmorphism (Efecto Vidrio):** Paneles translúcidos sobre fondo dinámico que otorgan profundidad.
* **Micro-animaciones:** Efectos de transición en botones, cambios de pestañas y glows informativos al autocompletarse datos del mapa.
* **Responsive Design:** La navegación lateral se adapta automáticamente a pantallas de tabletas y dispositivos móviles.
