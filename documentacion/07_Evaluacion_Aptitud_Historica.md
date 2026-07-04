# 🌦️ Evaluación de Aptitud de Ubicación Basada en Clima Histórico

**Chat de referencia:** `da8a39dd-7576-4a8c-b446-1aae295e17fc`  
**Fecha:** 2026-06-30  
**Área:** `Services/WeatherService.cs`, `wwwroot/js/apiario-clima.js`

---

## Contexto

El sistema de recomendación geográfica de apiarios evaluaba la idoneidad ("Aptitud") de las coordenadas geográficas de un apiario en base al clima meteorológico instantáneo (temperatura de la hora actual, velocidad del viento en ese instante, lluvia momentánea, etc.). 

**El problema:** Para un apicultor, el hecho de que esté lloviendo hoy o de que sea de noche en el instante de la consulta no significa que la ubicación del apiario sea mala a largo plazo. Las decisiones de instalación se planean a semanas o meses vista.

---

## Cambios realizados

### 1. Desvinculación del clima instantáneo en la calificación
* Se eliminó el factor meteorológico en tiempo real (instantáneo) de la fórmula de puntuación de aptitud de ubicación.
* El cálculo del **Puntaje de Aptitud** final se reestructuró para depender únicamente de dos factores estables de mediano y largo plazo:
  1. **Clima histórico reciente (últimos 180 días):** Representa el 45% del peso. Evalúa heladas recurrentes, vientos fuertes continuos, humedad promedio alta y sequías históricas a través de la API histórica de Open-Meteo.
  2. **Entorno geográfico (radio de 3km):** Representa el 55% del peso. Evalúa fuentes de agua dulce naturales, áreas de cultivo/flora cercanas y competencia (distancia a otros apiarios) extraída por OpenStreetMap (Overpass API).

```csharp
// Nueva ponderación estable (antes se incluía EvaluateCurrentWeather)
if (hasGeographicData)
{
    // 45% clima histórico + 55% geografía del entorno
    finalScore = (Math.Clamp(historicalScore, 0, 100) * 0.45) + (Math.Clamp(geoScore, 0, 100) * 0.55);
}
else
{
    // Fuera de cobertura regional: 100% clima histórico
    finalScore = Math.Clamp(historicalScore, 0, 100);
}
```

---

### 2. Clasificación de factores históricos en la UI
* Se adaptó la lista de **Factores a Favor (✅)** y **Advertencias (⚠️)** en el panel de detalles para no listar alertas efímeras (como "Sin radiación solar (noche)").
* Los avisos ahora detallan la frecuencia histórica de eventos:
  * Porcentaje de días con vientos superiores a 24 km/h en los últimos 180 días.
  * Recuento de heladas (días con temperatura media menor a 5 °C).
  * Humedad media extrema (días con humedad mayor al 80% que elevan riesgos de sanidad como *Nosema*).
  * Niveles de sequía acumulados.

---

## Archivos modificados

| Archivo | Cambio |
|---|---|
| `Services/WeatherService.cs` | Modificación del método `ProcessSuitability` para recalcular ponderaciones sin depender de la hora/clima actual. |

---

## Notas técnicas
* El servicio meteorológico continúa descargando datos del pronóstico por hora (`GetCurrentForecastAsync`) ya que la interfaz de usuario aún muestra las variables meteorológicas actuales de forma informativa en la parte superior del panel del clima, pero estas **ya no restan ni suman puntos** al índice de recomendación geográfica.
