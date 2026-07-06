using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.EntityFrameworkCore;
using BeeKeeperApp.Data;
using BeeKeeperApp.Models.Entities;

namespace BeeKeeperApp.Services
{
    public class WeatherService
    {
        private readonly HttpClient _httpClient;
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;

        public WeatherService(HttpClient httpClient, ApplicationDbContext context, IMemoryCache cache)
        {
            _httpClient = httpClient;
            _context = context;
            _cache = cache;
        }

        public async Task<ClimaAptitudDto?> GetBeeWeatherSuitabilityAsync(double latitude, double longitude)
        {
            try
            {
                // Run all data source queries in parallel for peak performance
                var forecastTask = GetCurrentForecastAsync(latitude, longitude);
                var archiveTask = GetHistoricalWeatherCachedAsync(latitude, longitude);
                var osmTask = GetOverpassSurroundingsCachedAsync(latitude, longitude);
                var dbTask = GetNearbyApiariesAsync(latitude, longitude);
                var geocodeTask = GetGeocodeInfoCachedAsync(latitude, longitude);

                await Task.WhenAll(forecastTask, archiveTask, osmTask, dbTask, geocodeTask);

                var forecast = await forecastTask;
                bool isForecastFallback = false;
                if (forecast == null)
                {
                    Console.WriteLine("[WeatherService] GetBeeWeatherSuitabilityAsync warning: GetCurrentForecastAsync returned null (probably rate limited 429). Using fallback.");
                    isForecastFallback = true;
                    forecast = new OpenMeteoResponse
                    {
                        Latitude = latitude,
                        Longitude = longitude,
                        Hourly = new HourlyData
                        {
                            Time = new List<string> { DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm") },
                            Temperature2m = new List<double> { 20.0 },
                            RelativeHumidity2m = new List<double> { 60.0 },
                            WindSpeed10m = new List<double> { 10.0 },
                            WindDirection10m = new List<double> { 180.0 },
                            Precipitation = new List<double> { 0.0 },
                            ShortwaveRadiation = new List<double> { 200.0 },
                            SoilTemperature0cm = new List<double> { 18.0 }
                        }
                    };
                }

                var archive = await archiveTask;
                if (archive == null)
                {
                    Console.WriteLine("[WeatherService] GetBeeWeatherSuitabilityAsync warning: GetHistoricalWeatherCachedAsync returned null.");
                }

                var osm = await osmTask;
                if (osm == null)
                {
                    Console.WriteLine("[WeatherService] GetBeeWeatherSuitabilityAsync warning: GetOverpassSurroundingsCachedAsync returned null.");
                }

                var nearbyApiaries = await dbTask;
                var geocode = await geocodeTask;
                if (geocode == null)
                {
                    Console.WriteLine("[WeatherService] GetBeeWeatherSuitabilityAsync warning: GetGeocodeInfoCachedAsync returned null.");
                }

                // Evaluate the component scores
                var dto = ProcessSuitability(latitude, longitude, forecast, archive, osm, nearbyApiaries, geocode, isForecastFallback);
                return dto;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WeatherService] Exception in GetBeeWeatherSuitabilityAsync: {ex.Message}\n{ex.StackTrace}");
                return null;
            }
        }

        private async Task<OpenMeteoResponse?> GetCurrentForecastAsync(double latitude, double longitude)
        {
            try
            {
                var url = $"https://api.open-meteo.com/v1/forecast?latitude={latitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}&longitude={longitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}&hourly=temperature_2m,relative_humidity_2m,wind_speed_10m,wind_direction_10m,precipitation,shortwave_radiation,soil_temperature_0cm&timezone=GMT";
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                return await _httpClient.GetFromJsonAsync<OpenMeteoResponse>(url, cts.Token);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WeatherService] Exception in GetCurrentForecastAsync: {ex.Message}\n{ex.StackTrace}");
                return null;
            }
        }

        private async Task<OpenMeteoArchiveResponse?> GetHistoricalWeatherCachedAsync(double latitude, double longitude)
        {
            double cachedLat = Math.Round(latitude, 3);
            double cachedLon = Math.Round(longitude, 3);
            string cacheKey = $"hist_weather_{cachedLat.ToString(System.Globalization.CultureInfo.InvariantCulture)}_{cachedLon.ToString(System.Globalization.CultureInfo.InvariantCulture)}";

            if (!_cache.TryGetValue(cacheKey, out OpenMeteoArchiveResponse? result))
            {
                result = await GetHistoricalWeatherAsync(latitude, longitude);
                if (result != null)
                {
                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                        .SetAbsoluteExpiration(TimeSpan.FromHours(24));
                    _cache.Set(cacheKey, result, cacheEntryOptions);
                }
            }
            return result;
        }

        private async Task<OpenMeteoArchiveResponse?> GetHistoricalWeatherAsync(double latitude, double longitude)
        {
            try
            {
                // Offset of 10 days to guarantee data validation in the Archive API (ERA5/Radiation)
                var endDate = DateTime.UtcNow.AddDays(-10).ToString("yyyy-MM-dd");
                var startDate = DateTime.UtcNow.AddDays(-190).ToString("yyyy-MM-dd");

                var url = $"https://archive-api.open-meteo.com/v1/archive?latitude={latitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}&longitude={longitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}&start_date={startDate}&end_date={endDate}&daily=precipitation_sum,relative_humidity_2m_mean,temperature_2m_mean,wind_speed_10m_max&timezone=GMT";

                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("User-Agent", "BeeKeeperApp/1.0");

                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(12));
                var response = await _httpClient.SendAsync(request, cts.Token);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<OpenMeteoArchiveResponse>();
                }
                else
                {
                    Console.WriteLine($"[WeatherService] GetHistoricalWeatherAsync returned status: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WeatherService] Exception in GetHistoricalWeatherAsync: {ex.Message}\n{ex.StackTrace}");
            }
            return null;
        }

        private async Task<OverpassResponse?> GetOverpassSurroundingsCachedAsync(double latitude, double longitude)
        {
            double cachedLat = Math.Round(latitude, 3);
            double cachedLon = Math.Round(longitude, 3);
            string cacheKey = $"osm_surroundings_{cachedLat.ToString(System.Globalization.CultureInfo.InvariantCulture)}_{cachedLon.ToString(System.Globalization.CultureInfo.InvariantCulture)}";

            if (!_cache.TryGetValue(cacheKey, out OverpassResponse? result))
            {
                result = await GetOverpassSurroundingsAsync(latitude, longitude);
                if (result != null)
                {
                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                        .SetAbsoluteExpiration(TimeSpan.FromHours(24));
                    _cache.Set(cacheKey, result, cacheEntryOptions);
                }
            }
            return result;
        }

        private async Task<OverpassResponse?> GetOverpassSurroundingsAsync(double latitude, double longitude)
        {
            try
            {
                var query = $@"
[out:json][timeout:15];
(
  node(around:3000, {latitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}, {longitude.ToString(System.Globalization.CultureInfo.InvariantCulture)})[""natural""=""water""];
  way(around:3000, {latitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}, {longitude.ToString(System.Globalization.CultureInfo.InvariantCulture)})[""natural""=""water""];
  relation(around:3000, {latitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}, {longitude.ToString(System.Globalization.CultureInfo.InvariantCulture)})[""natural""=""water""];
  node(around:3000, {latitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}, {longitude.ToString(System.Globalization.CultureInfo.InvariantCulture)})[""waterway""];
  way(around:3000, {latitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}, {longitude.ToString(System.Globalization.CultureInfo.InvariantCulture)})[""waterway""];
  relation(around:3000, {latitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}, {longitude.ToString(System.Globalization.CultureInfo.InvariantCulture)})[""waterway""];
  node(around:3000, {latitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}, {longitude.ToString(System.Globalization.CultureInfo.InvariantCulture)})[""landuse""=""farmland""];
  way(around:3000, {latitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}, {longitude.ToString(System.Globalization.CultureInfo.InvariantCulture)})[""landuse""=""farmland""];
  relation(around:3000, {latitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}, {longitude.ToString(System.Globalization.CultureInfo.InvariantCulture)})[""landuse""=""farmland""];
  node(around:3000, {latitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}, {longitude.ToString(System.Globalization.CultureInfo.InvariantCulture)})[""landuse""=""orchard""];
  way(around:3000, {latitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}, {longitude.ToString(System.Globalization.CultureInfo.InvariantCulture)})[""landuse""=""orchard""];
  relation(around:3000, {latitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}, {longitude.ToString(System.Globalization.CultureInfo.InvariantCulture)})[""landuse""=""orchard""];
  node(around:3000, {latitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}, {longitude.ToString(System.Globalization.CultureInfo.InvariantCulture)})[""landuse""=""vineyard""];
  way(around:3000, {latitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}, {longitude.ToString(System.Globalization.CultureInfo.InvariantCulture)})[""landuse""=""vineyard""];
  relation(around:3000, {latitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}, {longitude.ToString(System.Globalization.CultureInfo.InvariantCulture)})[""landuse""=""vineyard""];
  node(around:3000, {latitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}, {longitude.ToString(System.Globalization.CultureInfo.InvariantCulture)})[""crop""];
  way(around:3000, {latitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}, {longitude.ToString(System.Globalization.CultureInfo.InvariantCulture)})[""crop""];
  relation(around:3000, {latitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}, {longitude.ToString(System.Globalization.CultureInfo.InvariantCulture)})[""crop""];
);
out tags center;
";
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("data", query)
                });

                var request = new HttpRequestMessage(HttpMethod.Post, "https://overpass-api.de/api/interpreter")
                {
                    Content = content
                };
                request.Headers.Add("User-Agent", "BeeKeeperApp/1.0 (contact: support@beekeeperapp.com)");

                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(8));
                var response = await _httpClient.SendAsync(request, cts.Token);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<OverpassResponse>();
                }
                else
                {
                    Console.WriteLine($"[WeatherService] GetOverpassSurroundingsAsync returned status: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WeatherService] Exception in GetOverpassSurroundingsAsync: {ex.Message}\n{ex.StackTrace}");
            }
            return null;
        }

        private async Task<List<NearbyApiaryDto>> GetNearbyApiariesAsync(double latitude, double longitude)
        {
            var nearby = new List<NearbyApiaryDto>();
            try
            {
                var apiarios = await _context.Apiarios.ToListAsync();
                foreach (var apiario in apiarios)
                {
                    double dist = CalculateDistance(latitude, longitude, apiario.Latitud, apiario.Longitud);
                    if (dist <= 3000) // Within 3km foraging radius
                    {
                        nearby.Add(new NearbyApiaryDto
                        {
                            Nombre = apiario.Nombre,
                            Latitud = apiario.Latitud,
                            Longitud = apiario.Longitud,
                            DistanciaMetros = dist
                        });
                    }
                }
            }
            catch (Exception)
            {
                // Silent catch
            }
            return nearby.OrderBy(a => a.DistanciaMetros).ToList();
        }

        private ClimaAptitudDto ProcessSuitability(
            double latitude,
            double longitude,
            OpenMeteoResponse forecast,
            OpenMeteoArchiveResponse? archive,
            OverpassResponse? osm,
            List<NearbyApiaryDto> nearbyApiaries,
            NominatimResponse? geocode,
            bool isForecastFallback)
        {
            // Find current forecast metrics
            var nowUtc = DateTime.UtcNow;
            int selectedIndex = 0;
            double minDiffSeconds = double.MaxValue;

            for (int i = 0; i < forecast.Hourly.Time.Count; i++)
            {
                if (DateTime.TryParse(forecast.Hourly.Time[i], out var itemTime))
                {
                    var itemTimeUtc = DateTime.SpecifyKind(itemTime, DateTimeKind.Utc);
                    var diff = Math.Abs((itemTimeUtc - nowUtc).TotalSeconds);
                    if (diff < minDiffSeconds)
                    {
                        minDiffSeconds = diff;
                        selectedIndex = i;
                    }
                }
            }

            var h = forecast.Hourly;
            double temp = h.Temperature2m[selectedIndex];
            double humidity = h.RelativeHumidity2m[selectedIndex];
            double windSpeed = h.WindSpeed10m[selectedIndex];
            double windDirection = h.WindDirection10m[selectedIndex];
            double precipitation = h.Precipitation[selectedIndex];
            double radiation = h.ShortwaveRadiation[selectedIndex];
            double soilTemp = h.SoilTemperature0cm[selectedIndex];

            var dto = new ClimaAptitudDto
            {
                Temperatura = temp,
                Humedad = humidity,
                VelocidadViento = windSpeed,
                DireccionViento = windDirection,
                DireccionVientoCard = GetWindDirectionCard(windDirection),
                Precipitacion = precipitation,
                Radiacion = radiation,
                TemperaturaSuelo = soilTemp,
                RazonesSi = new List<string>(),
                RazonesNo = new List<string>()
            };

            if (isForecastFallback)
            {
                dto.RazonesNo.Add("No se pudo conectar con el clima en tiempo real actual (Rate limit o desconexión). La aptitud se calcula con el histórico y entorno.");
            }

            // 2. Historical Weather Analysis (25 pts base weight)
            int historicalScore = 100;
            if (archive != null && archive.Daily != null && archive.Daily.Time.Count > 0)
            {
                dto.TieneDatosHistoricos = true;
                var daily = archive.Daily;

                var validRain = daily.PrecipitationSum.Where(p => p.HasValue).Select(p => p!.Value).ToList();
                dto.LluviaAcumulada180Dias = validRain.Sum();
                dto.LluviaPromedioDiaria180Dias = validRain.Count > 0 ? dto.LluviaAcumulada180Dias / validRain.Count : 0;

                var validHum = daily.RelativeHumidity2mMean.Where(h2 => h2.HasValue).Select(h2 => h2!.Value).ToList();
                dto.DiasHumedadAlta180Dias = validHum.Count(humVal => humVal > 80);

                var validWind = daily.WindSpeed10mMax.Where(w => w.HasValue).Select(w => w!.Value).ToList();
                dto.DiasVientoFuerte180Dias = validWind.Count(wVal => wVal > 24);
                dto.PorcentajeDiasVientoFuerte = validWind.Count > 0 ? (double)dto.DiasVientoFuerte180Dias / validWind.Count * 100 : 0;

                var validTemp = daily.Temperature2mMean.Where(t2 => t2.HasValue).Select(t2 => t2!.Value).ToList();
                dto.DiasHeladas180Dias = validTemp.Count(tVal => tVal < 5);

                // Calculations and deductions for historical data
                if (dto.PorcentajeDiasVientoFuerte > 50)
                {
                    historicalScore -= 30;
                    dto.RazonesNo.Add($"Vientos fuertes frecuentes ({Math.Round(dto.PorcentajeDiasVientoFuerte)}% de los días en los últimos 180 días), lo que reduce la viabilidad de pecoreo continuo.");
                }
                else if (dto.PorcentajeDiasVientoFuerte > 30)
                {
                    historicalScore -= 15;
                    dto.RazonesNo.Add($"Vientos fuertes moderadamente frecuentes ({Math.Round(dto.PorcentajeDiasVientoFuerte)}% de los días en los últimos 180 días).");
                }
                else if (dto.PorcentajeDiasVientoFuerte <= 15)
                {
                    dto.RazonesSi.Add("Baja frecuencia de vientos fuertes en los últimos 180 días, ideal para vuelos sin interrupciones.");
                }

                if (dto.DiasHumedadAlta180Dias > 50)
                {
                    historicalScore -= 20;
                    dto.RazonesNo.Add($"Humedad media elevada recurrente ({dto.DiasHumedadAlta180Dias} de 180 días con HR > 80%).");
                }
                else if (dto.DiasHumedadAlta180Dias > 30)
                {
                    historicalScore -= 10;
                }

                // Sanitay Warning: Nosema disease risk (empirical/estimated threshold subject to validation)
                if (dto.DiasHumedadAlta180Dias > 36)
                {
                    dto.AlertaSanitaria = $"Humedad superior al 80% en {dto.DiasHumedadAlta180Dias} de los últimos 180 días. Esto puede favorecer la aparición de Nosema apis/ceranae (umbral de riesgo estimado por el equipo, sujeto a validación bibliográfica).";
                }

                if (dto.DiasHeladas180Dias > 25)
                {
                    historicalScore -= 15;
                    dto.RazonesNo.Add($"Presencia recurrente de temperaturas invernales extremas o heladas ({dto.DiasHeladas180Dias} días con media < 5°C), aumentando riesgo de enfriamiento de la cría.");
                }

                if (dto.LluviaPromedioDiaria180Dias < 0.5)
                {
                    historicalScore -= 15;
                    dto.RazonesNo.Add("Bajo nivel de lluvias acumuladas (posible sequía), lo que reduce la producción de néctar en la flora.");
                }
            }
            else
            {
                dto.TieneDatosHistoricos = false;
                historicalScore = 100; // neutral fallback
            }

            // 3. Geographic Surroundings Analysis (30% weight)
            int geoScore = 100;
            bool hasFallbackUsed = false;

            // 3a. Process local apiary competition
            dto.ApiariosCercanosCount = nearbyApiaries.Count;
            if (nearbyApiaries.Count > 0)
            {
                dto.DistanciaApiarioMasCercano = nearbyApiaries.Min(a => a.DistanciaMetros);
                if (nearbyApiaries.Count >= 3)
                {
                    geoScore -= 35;
                }
                else
                {
                    geoScore -= (nearbyApiaries.Count == 1) ? 10 : 20;
                }
            }
            // Sin competencia: se omite el mensaje para mantener el foco en factores de largo plazo.

            // 3b. Process OSM or Fallback for water and crops
            ProcessGeographics(latitude, longitude, osm, dto, ref geoScore, ref hasFallbackUsed);

            // 4. Calculate Final Composite Score and Aptitud Level
            bool hasGeographicData = IsInUruguay(latitude, longitude) || (osm != null && osm.Elements.Count > 0);

            double finalScore;
            if (hasGeographicData)
            {
                // Weighted (excluding current weather as it varies hourly/daily): 45% historical weather + 55% surroundings geography
                finalScore = (Math.Clamp(historicalScore, 0, 100) * 0.45) + (Math.Clamp(geoScore, 0, 100) * 0.55);
            }
            else
            {
                // Outside coverage area & no OSM (excluding current weather): based 100% on historical climate
                finalScore = Math.Clamp(historicalScore, 0, 100);
                dto.CultivosOrigen = "No detectado";
                dto.AguaOrigen = "No detectado";
                dto.RazonesNo.Add("Ubicación fuera de cobertura regional para estimaciones agrícolas. Evaluación basada únicamente en clima histórico.");
            }

            int scoreInt = (int)Math.Clamp(Math.Round(finalScore), 0, 100);
            dto.Puntaje = scoreInt;

            if (scoreInt >= 75)
            {
                dto.Aptitud = "Óptimo";
                dto.Color = "success";
            }
            else if (scoreInt >= 50)
            {
                dto.Aptitud = "Aceptable";
                dto.Color = "warning";
            }
            else
            {
                dto.Aptitud = "No recomendado";
                dto.Color = "danger";
            }

            // Geocoding processing
            if (geocode != null && geocode.Address != null)
            {
                dto.Departamento = CleanDepartmentName(geocode.Address.State ?? geocode.Address.County ?? "");
                dto.Paraje = geocode.Address.Suburb ?? geocode.Address.Hamlet ?? geocode.Address.Village ?? geocode.Address.Neighbourhood ?? geocode.Address.Town ?? geocode.Address.City ?? geocode.Address.Road ?? geocode.Address.Municipality ?? geocode.Address.County ?? "";
                
                if (string.IsNullOrEmpty(dto.Paraje) && !string.IsNullOrEmpty(geocode.DisplayName))
                {
                    var parts = geocode.DisplayName.Split(',');
                    if (parts.Length > 0)
                    {
                        dto.Paraje = parts[0].Trim();
                    }
                }
                
                if (string.IsNullOrEmpty(dto.Paraje))
                {
                    dto.Paraje = "Paraje Rural";
                }

                dto.Zona = DetermineZona(geocode);
                dto.SeccionPolicial = EstimateSeccionPolicial(latitude, longitude, dto.Departamento);
            }
            else
            {
                // Fallback offline logic
                if (IsInUruguay(latitude, longitude))
                {
                    dto.Departamento = GetClosestDepartment(latitude, longitude);
                    dto.SeccionPolicial = EstimateSeccionPolicial(latitude, longitude, dto.Departamento);
                    
                    var parajeDict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                    {
                        { "Artigas", "Bella Unión" },
                        { "Salto", "Daymán" },
                        { "Rivera", "Tranqueras" },
                        { "Paysandú", "Quebracho" },
                        { "Tacuarembó", "Paso de los Toros" },
                        { "Cerro Largo", "Fraile Muerto" },
                        { "Río Negro", "Young" },
                        { "Durazno", "Sarandí del Yí" },
                        { "Treinta y Tres", "Vergara" },
                        { "Soriano", "Mercedes Rural" },
                        { "Flores", "Trinidad Rural" },
                        { "Florida", "Sarandí Grande" },
                        { "Lavalleja", "Solís de Mataojo" },
                        { "Rocha", "Chuy Rural" },
                        { "Colonia", "Tarariras" },
                        { "San José", "Libertad" },
                        { "Canelones", "Sauce" },
                        { "Maldonado", "San Carlos" },
                        { "Montevideo", "Melilla" }
                    };

                    dto.Paraje = parajeDict.TryGetValue(dto.Departamento, out var p) ? p : "Paraje Rural";
                    dto.Zona = "Rural";
                }
                else
                {
                    dto.Departamento = "";
                    dto.SeccionPolicial = "";
                    dto.Paraje = "";
                    dto.Zona = "Rural";
                }
            }

            dto.SugerirTrashumanciaHabilitada = dto.Puntaje >= 50;

            // Fallback for details list if empty
            if (dto.RazonesSi.Count == 0 && dto.RazonesNo.Count == 0)
            {
                dto.Detalles.Add("Condiciones generales aceptables para la instalación.");
            }
            else
            {
                // Sync with original Detalles list to support any old UI bindings
                dto.Detalles.AddRange(dto.RazonesSi.Select(r => $"✅ {r}"));
                dto.Detalles.AddRange(dto.RazonesNo.Select(r => $"⚠️ {r}"));
            }

            return dto;
        }

        private int EvaluateCurrentWeather(double temp, double hum, double windSpd, double prec, double rad, double soilTemp, out List<string> reasonsSi, out List<string> reasonsNo)
        {
            int score = 100;
            reasonsSi = new List<string>();
            reasonsNo = new List<string>();

            // Precipitation
            if (prec >= 5.0)
            {
                score = 0;
                reasonsNo.Add("Lluvia fuerte o tormenta actual (vuelo de abejas imposible).");
            }
            else if (prec > 0)
            {
                score -= (int)(prec * 20);
                reasonsNo.Add($"Lluvia débil/moderada actual ({prec} mm/h).");
            }

            if (score > 0)
            {
                // Temperature
                if (temp >= 20 && temp <= 30)
                {
                    reasonsSi.Add($"Temperatura actual óptima para el pecoreo ({temp}°C).");
                }
                else if (temp < 10 || temp > 38)
                {
                    score -= 80;
                    reasonsNo.Add(temp < 10 ? $"Temperatura extremadamente baja ({temp}°C)." : $"Temperatura extremadamente alta ({temp}°C).");
                }
                else if (temp >= 10 && temp < 14)
                {
                    score -= (int)((14 - temp) * 15);
                    reasonsNo.Add($"Temperatura fría para el pecoreo ({temp}°C).");
                }
                else if (temp >= 14 && temp < 20)
                {
                    score -= (int)((20 - temp) * 5);
                    reasonsNo.Add($"Temperatura templada/fresca ({temp}°C).");
                }
                else if (temp > 30 && temp <= 35)
                {
                    score -= (int)((temp - 30) * 8);
                    reasonsNo.Add($"Temperatura calurosa ({temp}°C).");
                }
                else if (temp > 35 && temp <= 38)
                {
                    score -= (int)((temp - 35) * 20);
                    reasonsNo.Add($"Temperatura muy alta ({temp}°C).");
                }

                // Wind
                if (windSpd < 15)
                {
                    reasonsSi.Add($"Vientos actuales suaves ({windSpd} km/h), condiciones excelentes de vuelo.");
                }
                else if (windSpd >= 32)
                {
                    score -= 80;
                    reasonsNo.Add($"Viento fuerte perjudicial ({windSpd} km/h) — riesgo de desorientación y pérdida de abejas.");
                }
                else if (windSpd >= 24 && windSpd < 32)
                {
                    score -= (int)((windSpd - 24) * 5 + 27);
                    reasonsNo.Add($"Viento moderado/fuerte ({windSpd} km/h).");
                }
                else if (windSpd >= 15 && windSpd < 24)
                {
                    score -= (int)((windSpd - 15) * 3);
                    reasonsNo.Add($"Viento moderado ({windSpd} km/h).");
                }

                // Humidity
                if (hum >= 50 && hum <= 80)
                {
                    reasonsSi.Add($"Humedad relativa óptima ({hum}%).");
                }
                else if (hum < 50)
                {
                    score -= (int)((50 - hum) * 0.5);
                    reasonsNo.Add($"Humedad relativa actual baja ({hum}%).");
                }
                else if (hum > 80)
                {
                    score -= (int)((hum - 80) * 1.5);
                    reasonsNo.Add($"Humedad relativa actual alta ({hum}%).");
                }

                // Radiation
                if (rad >= 200)
                {
                    reasonsSi.Add("Buena radiación solar actual, estimula la salida de pecoreadoras.");
                }
                else if (rad == 0)
                {
                    score -= 60;
                    reasonsNo.Add("Sin radiación solar (noche).");
                }
                else if (rad < 50)
                {
                    score -= 30;
                    reasonsNo.Add($"Radiación solar muy baja / cielo muy nublado ({rad} W/m²).");
                }
                else if (rad >= 50 && rad < 200)
                {
                    score -= (int)((200 - rad) * 0.15);
                    reasonsNo.Add($"Radiación solar moderada / semisombra ({rad} W/m²).");
                }

                // Soil temp
                if (soilTemp < 10 || soilTemp > 35)
                {
                    score -= 15;
                }
                else if (soilTemp >= 10 && soilTemp < 15)
                {
                    score -= (int)((15 - soilTemp) * 2);
                }
                else if (soilTemp > 30 && soilTemp <= 35)
                {
                    score -= (int)((soilTemp - 30) * 2);
                }
            }

            return Math.Clamp(score, 0, 100);
        }

        private void ProcessGeographics(
            double latitude,
            double longitude,
            OverpassResponse? osm,
            ClimaAptitudDto dto,
            ref int geoScore,
            ref bool hasFallbackUsed)
        {
            double minWaterDist = double.MaxValue;
            var osmCrops = new HashSet<string>();
            bool osmWaterFound = false;

            if (osm != null && osm.Elements != null && osm.Elements.Count > 0)
            {
                foreach (var el in osm.Elements)
                {
                    double? elLat = el.Lat ?? el.Center?.Lat;
                    double? elLon = el.Lon ?? el.Center?.Lon;

                    if (elLat.HasValue && elLon.HasValue)
                    {
                        double dist = CalculateDistance(latitude, longitude, elLat.Value, elLon.Value);

                        // Check water sources
                        bool isWater = el.Tags.ContainsKey("natural") && el.Tags["natural"] == "water";
                        bool isWaterway = el.Tags.ContainsKey("waterway");
                        if (isWater || isWaterway)
                        {
                            osmWaterFound = true;
                            if (dist < minWaterDist)
                            {
                                minWaterDist = dist;
                            }
                        }

                        // Check crops
                        if (el.Tags.ContainsKey("crop"))
                        {
                            var cropName = TranslateCrop(el.Tags["crop"]);
                            osmCrops.Add(cropName);
                        }
                        else if (el.Tags.ContainsKey("landuse"))
                        {
                            var landuse = el.Tags["landuse"];
                            if (landuse == "farmland") osmCrops.Add("Cultivo agrícola general");
                            else if (landuse == "orchard") osmCrops.Add("Frutales");
                            else if (landuse == "vineyard") osmCrops.Add("Viñedo");
                            else if (landuse == "meadow") osmCrops.Add("Pasturas");
                        }
                    }
                }
            }

            // 1. Water Evaluation (OSM vs Fallback)
            if (osmWaterFound && minWaterDist <= 3000)
            {
                dto.TieneAguaCercana = true;
                dto.DistanciaAguaMasCercana = minWaterDist;
                dto.AguaOrigen = "OSM";
                dto.FuentesAguaDetalle = $"Fuente de agua detectada vía satélite/OSM a {Math.Round(minWaterDist)}m de la ubicación.";

                if (minWaterDist < 1000)
                {
                    dto.RazonesSi.Add($"Cercanía excelente a fuente de agua dulce natural ({Math.Round(minWaterDist)}m).");
                }
                else
                {
                    dto.RazonesSi.Add($"Acceso a fuente de agua dulce a {Math.Round(minWaterDist)}m (rango óptimo de pecoreo).");
                }
            }
            else
            {
                // Trigger Fallback for water
                var fallback = GetFallbackSurroundings(latitude, longitude);
                if (fallback != null)
                {
                    dto.TieneAguaCercana = fallback.TieneAguaCercana;
                    dto.DistanciaAguaMasCercana = fallback.DistanciaAguaMasCercana;
                    dto.AguaOrigen = "Estimado";
                    dto.FuentesAguaDetalle = fallback.FuentesAguaDetalle;
                    dto.RazonesSi.Add($"Disponibilidad de agua estimada regionalmente para la zona.");
                }
                else
                {
                    dto.TieneAguaCercana = false;
                    dto.AguaOrigen = "No detectado";
                    dto.FuentesAguaDetalle = "No se detectaron fuentes de agua dulce en un radio de 3km.";
                    geoScore -= 20;
                    dto.RazonesNo.Add("Sin fuentes de agua detectadas a menos de 3km. Será necesario colocar bebederos artificiales en el apiario.");
                }
            }

            // 2. Crop Evaluation (OSM vs Fallback)
            if (osmCrops.Count > 0)
            {
                dto.CultivosDetectados = osmCrops.ToList();
                dto.CultivosOrigen = "OSM";
                dto.CultivosDetalle = $"Cultivos detectados satelitalmente (OSM) a menos de 3km: {string.Join(", ", dto.CultivosDetectados)}.";
                dto.RazonesSi.Add($"Presencia comprobada de flora melífera/agrícola favorable: {string.Join(", ", dto.CultivosDetectados)}.");
            }
            else
            {
                // Trigger Fallback for crops
                var fallback = GetFallbackSurroundings(latitude, longitude);
                if (fallback != null && fallback.CultivosDetectados.Count > 0)
                {
                    dto.CultivosDetectados = fallback.CultivosDetectados;
                    dto.CultivosOrigen = "Estimado";
                    dto.CultivosDetalle = fallback.CultivosDetalle;
                    dto.RazonesSi.Add($"Cultivos potenciales en la zona (estimación regional): {string.Join(", ", dto.CultivosDetectados)}.");
                }
                else
                {
                    dto.CultivosOrigen = "No detectado";
                    dto.CultivosDetalle = "No se identificaron zonas de cultivos en las bases cartográficas.";
                    geoScore -= 15;
                    dto.RazonesNo.Add("Escasa vegetación de cultivo melífero mapeada a menos de 3km. Verifique flora nativa o silvestre local en terreno.");
                }
            }
        }

        private string TranslateCrop(string englishCrop)
        {
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "soy", "Soja" },
                { "soybean", "Soja" },
                { "sunflower", "Girasol" },
                { "citrus", "Cítricos" },
                { "orange", "Cítricos (Naranjo)" },
                { "lemon", "Cítricos (Limonero)" },
                { "corn", "Maíz" },
                { "maize", "Maíz" },
                { "wheat", "Trigo" },
                { "barley", "Cebada" },
                { "rice", "Arroz" },
                { "pasture", "Pasturas" },
                { "grass", "Praderas" },
                { "alfalfa", "Alfalfa" },
                { "clover", "Trébol" },
                { "eucalyptus", "Eucalipto" },
                { "forest", "Área Forestal" },
                { "vineyard", "Viñedo" },
                { "orchard", "Frutales" }
            };

            return dict.TryGetValue(englishCrop, out var value) ? value : englishCrop;
        }

        private ClimaAptitudDto? GetFallbackSurroundings(double lat, double lon)
        {
            if (!IsInUruguay(lat, lon))
            {
                // Outside Uruguay
                return null;
            }

            var dto = new ClimaAptitudDto
            {
                TieneAguaCercana = true,
                DistanciaAguaMasCercana = 1500, // acceptable baseline
                AguaOrigen = "Estimado",
                CultivosOrigen = "Estimado"
            };

            // Estimate based on Uruguayan agricultural zoning
            if (lat > -32.5) // Northern Uruguay
            {
                if (lon > -56.5) // Northwest: Salto, Paysandú, Artigas
                {
                    dto.CultivosDetectados = new List<string> { "Cítricos", "Arándanos", "Eucalipto (Plantación forestal)" };
                    dto.CultivosDetalle = "Zonificación agropecuaria del norte: alta presencia de plantaciones de cítricos, frutales y forestación de eucalipto.";
                    dto.FuentesAguaDetalle = "Influencia de la cuenca del Río Uruguay. Se estima disponibilidad razonable en arroyos locales y tajamares ganaderos.";
                    dto.DistanciaAguaMasCercana = 1200;
                }
                else // Northeast: Rivera, Tacuarembó, Cerro Largo
                {
                    dto.CultivosDetectados = new List<string> { "Eucalipto", "Pinos (Forestación)", "Praderas naturales", "Arroz (zonas bajas)" };
                    dto.CultivosDetalle = "Zonificación agro-forestal del noreste: dominancia de pasturas de campo natural y montes forestales de eucalipto/pino.";
                    dto.FuentesAguaDetalle = "Serranías y cabeceras de cuenca (Río Negro). Abundancia de cañadas y tajamares en valles.";
                    dto.DistanciaAguaMasCercana = 1800;
                }
            }
            else // Southern Uruguay
            {
                if (lon > -56.5) // West: Soriano, Colonia, Río Negro, San José
                {
                    dto.CultivosDetectados = new List<string> { "Soja", "Trigo", "Girasol", "Alfalfa/Tréboles (Pasturas lecheras)" };
                    dto.CultivosDetalle = "Zona agrícola núcleo del litoral oeste: predominio de cultivos de secano de alta floración melífera y praderas artificiales.";
                    dto.FuentesAguaDetalle = "Cuencas de arroyos tributarios del Río Uruguay e importantes fuentes superficiales artificiales (tajamares).";
                    dto.DistanciaAguaMasCercana = 1000;
                }
                else if (lon < -56.5 && lat < -34.3) // South: Canelones, Montevideo, Florida meridional
                {
                    dto.CultivosDetectados = new List<string> { "Frutales de carozo", "Viñedos", "Horticultura", "Praderas mixtas" };
                    dto.CultivosDetalle = "Franja granjera y vitivinícola del sur: alta variedad de frutales templados, vides de floración y praderas de pastoreo intensivo.";
                    dto.FuentesAguaDetalle = "Zona de la cuenca del Río Santa Lucía y cañadas costeras. Alta disponibilidad de fuentes superficiales.";
                    dto.DistanciaAguaMasCercana = 1100;
                }
                else // East: Treinta y Tres, Rocha, Maldonado, Lavalleja
                {
                    dto.CultivosDetectados = new List<string> { "Praderas naturales", "Trébol blanco", "Lotos", "Monte nativo", "Arroz" };
                    dto.CultivosDetalle = "Zona de llanuras y serranías del este: praderas con leguminosas melíferas, monte natural serrano y cuencas arroceras.";
                    dto.FuentesAguaDetalle = "Cercanía a lagunas costeras y humedales del Este. Excelente densidad de cañadas, bañados y arroyos.";
                    dto.DistanciaAguaMasCercana = 900;
                }
            }

            return dto;
        }

        private static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            var R = 6371000; // Earth's radius in meters
            var phi1 = lat1 * Math.PI / 180;
            var phi2 = lat2 * Math.PI / 180;
            var deltaPhi = (lat2 - lat1) * Math.PI / 180;
            var deltaLambda = (lon2 - lon1) * Math.PI / 180;

            var a = Math.Sin(deltaPhi / 2) * Math.Sin(deltaPhi / 2) +
                    Math.Cos(phi1) * Math.Cos(phi2) *
                    Math.Sin(deltaLambda / 2) * Math.Sin(deltaLambda / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return R * c;
        }

        private string GetWindDirectionCard(double degree)
        {
            string[] cardinals = { "N", "NE", "E", "SE", "S", "SO", "O", "NO", "N" };
            return cardinals[(int)Math.Round((degree % 360) / 45)];
        }

        private bool IsInUruguay(double lat, double lon)
        {
            return lat >= -35.0 && lat <= -30.0 && lon >= -58.5 && lon <= -53.0;
        }

        private async Task<NominatimResponse?> GetGeocodeInfoCachedAsync(double latitude, double longitude)
        {
            double cachedLat = Math.Round(latitude, 4);
            double cachedLon = Math.Round(longitude, 4);
            string cacheKey = $"geocode_{cachedLat.ToString(System.Globalization.CultureInfo.InvariantCulture)}_{cachedLon.ToString(System.Globalization.CultureInfo.InvariantCulture)}";

            if (!_cache.TryGetValue(cacheKey, out NominatimResponse? result))
            {
                result = await GetGeocodeInfoAsync(latitude, longitude);
                if (result != null)
                {
                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                        .SetAbsoluteExpiration(TimeSpan.FromHours(24));
                    _cache.Set(cacheKey, result, cacheEntryOptions);
                }
            }
            return result;
        }

        private async Task<NominatimResponse?> GetGeocodeInfoAsync(double latitude, double longitude)
        {
            try
            {
                var url = $"https://nominatim.openstreetmap.org/reverse?format=jsonv2&lat={latitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}&lon={longitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}&zoom=18&addressdetails=1";
                
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("User-Agent", "BeeKeeperApp/1.0 (contact: support@beekeeperapp.com)");

                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(8));
                var response = await _httpClient.SendAsync(request, cts.Token);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<NominatimResponse>();
                }
                else
                {
                    Console.WriteLine($"[WeatherService] GetGeocodeInfoAsync returned status: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WeatherService] Exception in GetGeocodeInfoAsync: {ex.Message}\n{ex.StackTrace}");
            }
            return null;
        }

        private string CleanDepartmentName(string state)
        {
            if (string.IsNullOrEmpty(state)) return string.Empty;
            return state.Replace("Departamento de ", "", StringComparison.OrdinalIgnoreCase)
                        .Replace("Departamento ", "", StringComparison.OrdinalIgnoreCase)
                        .Trim();
        }

        private string DetermineZona(NominatimResponse geocode)
        {
            var addr = geocode.Address;
            if (!string.IsNullOrEmpty(addr.Hamlet) || 
                !string.IsNullOrEmpty(addr.Village) || 
                geocode.Type == "farm" || 
                geocode.Type == "isolated_dwellings")
            {
                return "Rural";
            }
            
            if (!string.IsNullOrEmpty(addr.City) || !string.IsNullOrEmpty(addr.Suburb))
            {
                return "Urbana";
            }
            
            if (!string.IsNullOrEmpty(addr.Town) || !string.IsNullOrEmpty(addr.Neighbourhood))
            {
                return "Suburbana";
            }
            
            return "Rural";
        }

        private string EstimateSeccionPolicial(double lat, double lon, string departamento)
        {
            int sectionNum = (int)(Math.Abs(lat * 100 + lon * 100) % 12) + 1;
            return sectionNum switch
            {
                1 => "1ra",
                2 => "2da",
                3 => "3ra",
                4 => "4ta",
                5 => "5ta",
                6 => "6ta",
                7 => "7ma",
                8 => "8va",
                9 => "9na",
                10 => "10ma",
                11 => "11ra",
                12 => "12ma",
                _ => $"{sectionNum}ª"
            };
        }

        private string GetClosestDepartment(double lat, double lon)
        {
            var departments = new (string Name, double Lat, double Lon)[]
            {
                ("Artigas", -30.6, -56.9),
                ("Salto", -31.4, -57.1),
                ("Rivera", -31.5, -55.5),
                ("Paysandú", -32.0, -57.2),
                ("Tacuarembó", -32.2, -55.9),
                ("Cerro Largo", -32.4, -54.3),
                ("Río Negro", -32.7, -57.3),
                ("Durazno", -33.0, -56.0),
                ("Treinta y Tres", -33.0, -54.3),
                ("Soriano", -33.5, -57.8),
                ("Flores", -33.6, -56.9),
                ("Florida", -33.8, -55.8),
                ("Lavalleja", -34.0, -55.0),
                ("Rocha", -34.0, -54.0),
                ("Colonia", -34.2, -57.7),
                ("San José", -34.3, -56.7),
                ("Canelones", -34.5, -56.0),
                ("Maldonado", -34.7, -54.8),
                ("Montevideo", -34.8, -56.2)
            };

            string closestDept = "Montevideo";
            double minDist = double.MaxValue;

            foreach (var dept in departments)
            {
                double dist = CalculateDistance(lat, lon, dept.Lat, dept.Lon);
                if (dist < minDist)
                {
                    minDist = dist;
                    closestDept = dept.Name;
                }
            }

            return closestDept;
        }
    }

    public class ClimaAptitudDto
    {
        public double Temperatura { get; set; }
        public double Humedad { get; set; }
        public double VelocidadViento { get; set; }
        public double DireccionViento { get; set; }
        public string DireccionVientoCard { get; set; } = string.Empty;
        public double Precipitacion { get; set; }
        public double Radiacion { get; set; }
        public double TemperaturaSuelo { get; set; }
        public int Puntaje { get; set; }
        public string Aptitud { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public List<string> Detalles { get; set; } = new();

        // --- GEOLOCALIZACIÓN AUTO-COMPLETADA ---
        public string Departamento { get; set; } = string.Empty;
        public string SeccionPolicial { get; set; } = string.Empty;
        public string Zona { get; set; } = string.Empty;
        public string Paraje { get; set; } = string.Empty;
        public bool SugerirTrashumanciaHabilitada { get; set; }

        // --- HISTORIAL CLIMÁTICO (180 DÍAS) ---
        public double LluviaAcumulada180Dias { get; set; }
        public double LluviaPromedioDiaria180Dias { get; set; }
        public int DiasHumedadAlta180Dias { get; set; }
        public int DiasVientoFuerte180Dias { get; set; }
        public double PorcentajeDiasVientoFuerte { get; set; }
        public int DiasHeladas180Dias { get; set; }
        public string AlertaSanitaria { get; set; } = string.Empty;
        public bool TieneDatosHistoricos { get; set; }

        // --- ENTORNO GEOGRÁFICO (3KM) ---
        public int ApiariosCercanosCount { get; set; }
        public double? DistanciaApiarioMasCercano { get; set; }
        public double? DistanciaAguaMasCercana { get; set; }
        public List<string> CultivosDetectados { get; set; } = new();
        public bool TieneAguaCercana { get; set; }
        public string FuentesAguaDetalle { get; set; } = string.Empty;
        public string CultivosDetalle { get; set; } = string.Empty;
        public string CultivosOrigen { get; set; } = "No detectado";
        public string AguaOrigen { get; set; } = "No detectado";

        // --- RAZONES DETALLADAS ---
        public List<string> RazonesSi { get; set; } = new();
        public List<string> RazonesNo { get; set; } = new();
    }

    public class NearbyApiaryDto
    {
        public string Nombre { get; set; } = string.Empty;
        public double Latitud { get; set; }
        public double Longitud { get; set; }
        public double DistanciaMetros { get; set; }
    }

    public class OverpassResponse
    {
        [JsonPropertyName("elements")]
        public List<OverpassElement> Elements { get; set; } = new();
    }

    public class OverpassElement
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("lat")]
        public double? Lat { get; set; }

        [JsonPropertyName("lon")]
        public double? Lon { get; set; }

        [JsonPropertyName("center")]
        public OverpassCenter? Center { get; set; }

        [JsonPropertyName("tags")]
        public Dictionary<string, string> Tags { get; set; } = new();
    }

    public class OverpassCenter
    {
        [JsonPropertyName("lat")]
        public double Lat { get; set; }

        [JsonPropertyName("lon")]
        public double Lon { get; set; }
    }

    public class OpenMeteoArchiveResponse
    {
        [JsonPropertyName("daily")]
        public DailyArchiveData Daily { get; set; } = new();
    }

    public class DailyArchiveData
    {
        [JsonPropertyName("time")]
        public List<string> Time { get; set; } = new();

        [JsonPropertyName("precipitation_sum")]
        public List<double?> PrecipitationSum { get; set; } = new();

        [JsonPropertyName("relative_humidity_2m_mean")]
        public List<double?> RelativeHumidity2mMean { get; set; } = new();

        [JsonPropertyName("temperature_2m_mean")]
        public List<double?> Temperature2mMean { get; set; } = new();

        [JsonPropertyName("wind_speed_10m_max")]
        public List<double?> WindSpeed10mMax { get; set; } = new();
    }

    public class OpenMeteoResponse
    {
        [JsonPropertyName("latitude")]
        public double Latitude { get; set; }

        [JsonPropertyName("longitude")]
        public double Longitude { get; set; }

        [JsonPropertyName("hourly")]
        public HourlyData Hourly { get; set; } = new();
    }

    public class HourlyData
    {
        [JsonPropertyName("time")]
        public List<string> Time { get; set; } = new();

        [JsonPropertyName("temperature_2m")]
        public List<double> Temperature2m { get; set; } = new();

        [JsonPropertyName("relative_humidity_2m")]
        public List<double> RelativeHumidity2m { get; set; } = new();

        [JsonPropertyName("wind_speed_10m")]
        public List<double> WindSpeed10m { get; set; } = new();

        [JsonPropertyName("wind_direction_10m")]
        public List<double> WindDirection10m { get; set; } = new();

        [JsonPropertyName("precipitation")]
        public List<double> Precipitation { get; set; } = new();

        [JsonPropertyName("shortwave_radiation")]
        public List<double> ShortwaveRadiation { get; set; } = new();

        [JsonPropertyName("soil_temperature_0cm")]
        public List<double> SoilTemperature0cm { get; set; } = new();
    }

    public class NominatimResponse
    {
        [JsonPropertyName("address")]
        public NominatimAddress Address { get; set; } = new();

        [JsonPropertyName("display_name")]
        public string DisplayName { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;
    }

    public class NominatimAddress
    {
        [JsonPropertyName("state")]
        public string State { get; set; } = string.Empty;

        [JsonPropertyName("county")]
        public string County { get; set; } = string.Empty;

        [JsonPropertyName("municipality")]
        public string Municipality { get; set; } = string.Empty;

        [JsonPropertyName("city")]
        public string City { get; set; } = string.Empty;

        [JsonPropertyName("town")]
        public string Town { get; set; } = string.Empty;

        [JsonPropertyName("village")]
        public string Village { get; set; } = string.Empty;

        [JsonPropertyName("hamlet")]
        public string Hamlet { get; set; } = string.Empty;

        [JsonPropertyName("suburb")]
        public string Suburb { get; set; } = string.Empty;

        [JsonPropertyName("neighbourhood")]
        public string Neighbourhood { get; set; } = string.Empty;

        [JsonPropertyName("road")]
        public string Road { get; set; } = string.Empty;

        [JsonPropertyName("postcode")]
        public string Postcode { get; set; } = string.Empty;

        [JsonPropertyName("country")]
        public string Country { get; set; } = string.Empty;
    }
}
