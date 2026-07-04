# 🌦️ Servicio de Clima e Integración con la API Open-Meteo

**Chat de referencia:** `3024cbda-5ed3-402c-ab02-0882fe162408`  
**Fecha:** 2026-06-16  
**Área:** `Services/WeatherService.cs`, `Controllers/ApiariosController.cs`, `Views/Apiarios/Create.cshtml`, `Views/Apiarios/Edit.cshtml`

---

## Contexto

Se integró un módulo meteorológico inteligente para asistir al apicultor en la toma de decisiones al instalar o trasladar apiarios. Se diseñó un servicio C# backend que consume datos meteorológicos en tiempo real y geográficos a partir de las coordenadas seleccionadas en un mapa Leaflet interactivo.

---

## Cambios realizados

### 1. Creación del Servicio `WeatherService.cs`
* Se desarrolló una clase de servicio inyectable que realiza peticiones HTTP concurrentes a la API libre de **Open-Meteo** (`forecast` y `archive`).
* Se obtienen los siguientes parámetros clave para la apicultura:
  * `temperature_2m` (Temperatura ambiental a 2 metros).
  * `relative_humidity_2m` (Humedad relativa del aire).
  * `wind_speed_10m` / `wind_direction_10m` (Velocidad y dirección del viento).
  * `precipitation` (Precipitación acumulada en la hora).
  * `shortwave_radiation` (Radiación solar de onda corta - estimulador de vuelo).
  * `soil_temperature_0cm` (Temperatura superficial del suelo).

---

### 2. Algoritmo de Evaluación y Deducción de Puntaje Apícola
El servicio procesa las variables climáticas aplicando penalizaciones y bonificaciones cuantitativas sobre una base de **100 puntos**:

#### 🛑 Umbrales de Aptitud
* **Óptimo (>= 75 pts):** Las abejas tienen condiciones ideales de vuelo, baja velocidad de viento, temperatura templada y buena radiación solar.
* **Aceptable (50 a 74 pts):** Condiciones aptas pero con advertencias menores (ligero viento, humedad moderada o cielo nublado).
* **No recomendado (< 50 pts):** Condiciones perjudiciales o de riesgo para la supervivencia o pecoreo (heladas recurrentes, vientos fuertes continuos o sequías).

---

### 3. Exposición de Endpoint y Consumo AJAX
* Se agregó un endpoint en `ApiariosController.cs` (`/Apiarios/Clima`) que recibe latitud y longitud, llama al servicio y devuelve un objeto estructurado en formato JSON.
* En `Views/Apiarios/Create.cshtml` y `Edit.cshtml`, se añadió una función JavaScript vinculada al mapa. Cada vez que el apicultor mueve el marcador en el mapa de Leaflet, se realiza una consulta asíncrona al endpoint y se renderiza un widget con el desglose de factores climáticos y la recomendación de aptitud en tiempo real.

---

## Archivos modificados

| Archivo | Cambio |
|---|---|
| `Services/WeatherService.cs` | Lógica de llamada HTTP a Open-Meteo y algoritmo de ponderación. |
| `Controllers/ApiariosController.cs` | Endpoint `Clima` expuesto como `JsonResult`. |
| `Views/Apiarios/Create.cshtml` | Inicialización de mapa Leaflet, marcador interactivo y contenedor del widget del clima. |
| `Views/Apiarios/Edit.cshtml` | Inicialización del widget climático precargado con las coordenadas del apiario actual. |

---

## Notas técnicas
* Se implementó **`IMemoryCache`** en el servicio de clima para almacenar los resultados históricos de geolocalización e información geográfica (OpenStreetMap y Nominatim) por un lapso de 24 horas. Esto reduce el consumo de cuotas de las APIs externas y mejora notablemente el tiempo de respuesta ante consultas repetidas sobre las mismas coordenadas.
