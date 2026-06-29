# Plan de Implementación: Evaluación Enriquecida para Instalación de Apiarios

Este plan detalla la incorporación de variables adicionales y lógica avanzada de análisis (según las recomendaciones de INTA y sugerencias del cliente) al momento de evaluar la aptitud climática e idoneidad geográfica para instalar un nuevo apiario.

## Decisiones de Diseño e Incorporación de Feedback

1. **Offset de 10 días para Open-Meteo Archive**:
   Para evitar errores por demoras de validación de datos en la API de archivo (especialmente variables de radiación y ERA5), usaremos un offset seguro de 10 días. Consultaremos el rango desde hace **190 días** hasta hace **10 días** para cubrir los 180 días de historial climático.
2. **Umbral Empírico de Nosema**:
   El umbral propuesto de "más de 20% de días (>36 días) con humedad >80% en los últimos 180 días" se considerará una **estimación técnica empírica del equipo de desarrollo** sujeta a validación final con el cliente o bibliografía apícola formal. Se presentará con esta aclaración en el backend y el frontend.
3. **Caché para Consultas Pesadas (Overpass/OpenStreetMap)**:
   Para optimizar la performance y evitar sobrecargar los servidores públicos de OSM ante consultas repetidas por clics del usuario en el mapa, implementaremos un sistema de caché en memoria (`IMemoryCache`) en el backend.
   - Las coordenadas se redondearán a **3 decimales** (aproximadamente una celda de 100 metros) para agrupar consultas cercanas.
   - Las respuestas de Overpass y Open-Meteo Archive se cachearán por **24 horas**.
4. **Visualización de Estimaciones en Frontend**:
   Los cultivos que procedan de la estimación por fallback regional se identificarán claramente con un badge de **"Estimado por Zona"** (color gris/azul suave) frente a los cultivos reales mapeados por satélite/OSM que tendrán el badge **"Detectado por OSM"** (color verde/amarillo).
5. **Justificación de Pesos (45% Clima Actual, 25% Histórico, 30% Entorno)**:
   - **Clima Actual (45%)**: Representa el factor crítico inmediato para el pecoreo y la supervivencia de la colmena en el corto plazo (si hay tormenta o viento extremo en el momento, el vuelo es inviable).
   - **Entorno Geográfico (30%)**: Define la sustentabilidad de la colmena en cuanto a recursos vitales continuos (fuentes de agua dulce) y competencia por pecoreo con otros apiarios.
   - **Clima Histórico (25%)**: Modela las tendencias climáticas recurrentes de la zona que pueden desencadenar problemas sanitarios a mediano plazo (como humedad excesiva que propicie Nosema).
6. **Escenario Sin Cobertura (Fuera de Uruguay)**:
   Si el apiario se evalúa en coordenadas fuera de Uruguay y Overpass no retorna datos, el sistema desactivará el puntaje geográfico y calculará el índice basándose únicamente en el clima (escalado a 100). En la interfaz se mostrará el mensaje: *"Ubicación fuera de cobertura regional. Evaluación basada únicamente en clima actual e historial."*

---

## Proposed Changes

### Servicios y Backend

#### [MODIFY] [WeatherService.cs](file:///c:/Users/Usuario/Documents/GitHub/Obligatorio/Services/WeatherService.cs)
- Inyectar `IMemoryCache` para almacenar las consultas a Overpass y Open-Meteo Archive.
- Inyectar `ApplicationDbContext` para consultar la base de datos de apiarios locales.
- Agregar las clases DTO para las respuestas de **Open-Meteo Archive API** y **OSM Overpass API**.
- Implementar los siguientes métodos auxiliares:
  - `GetHistoricalWeatherAsync(double latitude, double longitude)`: Consulta con offset de 10 días.
  - `GetOverpassSurroundingsAsync(double latitude, double longitude)`: Consulta fuentes de agua y cultivos en Overpass API en un radio de 3000 metros, con redondeo de coordenadas a 3 decimales para la clave del caché.
  - `GetNearbyApiariesAsync(double latitude, double longitude)`: Consulta apiarios cercanos y calcula distancias por Haversine.
  - `GetFallbackSurroundings(double latitude, double longitude)`: Devuelve estimaciones regionales de cultivos y advertencias de agua para Uruguay.
- Enriquecer `ClimaAptitudDto` para contener toda la metadata nueva.
- Refactorizar el flujo en `GetBeeWeatherSuitabilityAsync` para ejecutar las consultas en paralelo con `Task.WhenAll`.

#### [MODIFY] [ApiariosController.cs](file:///c:/Users/Usuario/Documents/GitHub/Obligatorio/Controllers/ApiariosController.cs)
- Asegurar que la acción `Clima(double lat, double lon)` use el nuevo flujo enriquecido.

#### [MODIFY] [Program.cs](file:///c:/Users/Usuario/Documents/GitHub/Obligatorio/Program.cs)
- Registrar el servicio de caché `builder.Services.AddMemoryCache();` para habilitar `IMemoryCache`.

---

### Frontend y Diseño de Interfaz

#### [MODIFY] [apiario-clima.js](file:///c:/Users/Usuario/Documents/GitHub/Obligatorio/wwwroot/js/apiario-clima.js)
Rediseñar por completo la visualización del panel de aptitud en el contenedor `#clima-contenedor` con estética premium, incluyendo:
1. **Cabecera Dinámica**: Estado ("Óptimo", "Aceptable", "No recomendado") acompañado de un círculo de progreso con el puntaje total.
2. **Sección de Entorno (Geografía)**:
   - Tarjetas/Badges indicando cultivos cercanos identificados (soja, girasol, cítricos, etc.) o la estimación regional satelital.
   - Distancia a la fuente de agua más cercana, con colores (verde: <1km, amarillo: 1-3km, rojo: >3km/no detectado).
   - Competencia: Apiarios registrados en el radio de 3km (ej. "Baja (0 apiarios)" o "Alta (3 apiarios a < 2km)").
3. **Sección de Historial (Últimos 180 días)**:
   - Gráfico/Datos de lluvias promedio acumuladas.
   - Alerta sanitaria de **Nosema** destacada en caso de humedad alta recurrente.
   - Frecuencia de vientos fuertes y heladas.
4. **Lista Detallada de Por Qué Sí / Por Qué No**:
   - Sección interactiva estructurada con las razones exactas para instalar o no, con iconos claros (✅ para puntos a favor, ⚠️ para advertencias).

---

## Verification Plan

### Automated Tests
- Ejecutar la aplicación en modo desarrollo local.
- Validar las respuestas HTTP de las nuevas llamadas internas (Open-Meteo Archive y Overpass) y el rendimiento en paralelo.

### Manual Verification
1. Probar en el mapa de registro de apiarios haciendo clic en:
   - Coordenadas de Salto, Uruguay (ej: -31.38, -57.96) para comprobar que detecta o estima cítricos/forestación y fuentes de agua como el Río Uruguay.
   - Coordenadas de Soriano/Colonia (ej: -34.15, -57.85) para verificar estimación de soja/pasturas.
2. Registrar un apiario de prueba en un punto, y luego intentar evaluar otro punto a menos de 2km para verificar que el sistema cuenta el apiario local registrado e informa la competencia.
3. Comprobar que los tiempos de carga del panel se mantienen ágiles y no bloquean el flujo de registro.
