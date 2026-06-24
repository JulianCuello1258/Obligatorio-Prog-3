using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BeeKeeperApp.Services
{
    public class WeatherService
    {
        private readonly HttpClient _httpClient;

        public WeatherService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ClimaAptitudDto?> GetBeeWeatherSuitabilityAsync(double latitude, double longitude)
        {
            try
            {
                // Requesting standard parameters + GMT timezone to align times in UTC.
                var url = $"https://api.open-meteo.com/v1/forecast?latitude={latitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}&longitude={longitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}&hourly=temperature_2m,relative_humidity_2m,wind_speed_10m,wind_direction_10m,precipitation,shortwave_radiation,soil_temperature_0cm&timezone=GMT";

                var response = await _httpClient.GetFromJsonAsync<OpenMeteoResponse>(url);
                if (response == null || response.Hourly == null || response.Hourly.Time == null)
                {
                    return null;
                }

                // Find index corresponding to current hour in UTC.
                var nowUtc = DateTime.UtcNow;
                int selectedIndex = 0;
                double minDiffSeconds = double.MaxValue;

                for (int i = 0; i < response.Hourly.Time.Count; i++)
                {
                    if (DateTime.TryParse(response.Hourly.Time[i], out var itemTime))
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

                // Ensure index is within range of all lists
                var h = response.Hourly;
                if (selectedIndex >= h.Time.Count ||
                    selectedIndex >= h.Temperature2m.Count ||
                    selectedIndex >= h.RelativeHumidity2m.Count ||
                    selectedIndex >= h.WindSpeed10m.Count ||
                    selectedIndex >= h.WindDirection10m.Count ||
                    selectedIndex >= h.Precipitation.Count ||
                    selectedIndex >= h.ShortwaveRadiation.Count ||
                    selectedIndex >= h.SoilTemperature0cm.Count)
                {
                    return null;
                }

                double temp = h.Temperature2m[selectedIndex];
                double humidity = h.RelativeHumidity2m[selectedIndex];
                double windSpeed = h.WindSpeed10m[selectedIndex];
                double windDirection = h.WindDirection10m[selectedIndex];
                double precipitation = h.Precipitation[selectedIndex];
                double radiation = h.ShortwaveRadiation[selectedIndex];
                double soilTemp = h.SoilTemperature0cm[selectedIndex];

                return EvaluateSuitability(temp, humidity, windSpeed, windDirection, precipitation, radiation, soilTemp);
            }
            catch (Exception)
            {
                // In case of any API or network issue, return null so caller can handle gracefully.
                return null;
            }
        }

        private ClimaAptitudDto EvaluateSuitability(double temp, double hum, double windSpd, double windDir, double prec, double rad, double soilTemp)
        {
            int score = 100;
            var details = new List<string>();

            // 1. Precipitation (mm/h)
            if (prec >= 5.0)
            {
                score = 0;
                details.Add("Lluvia fuerte o tormenta (vuelo imposible)");
            }
            else if (prec > 0)
            {
                int deduction = (int)(prec * 20);
                score -= deduction;
                details.Add($"Lluvia débil/moderada ({prec} mm/h)");
            }

            if (score > 0)
            {
                // 2. Temperature (optimal: 20-30°C)
                if (temp < 10 || temp > 38)
                {
                    score -= 80;
                    details.Add(temp < 10 ? $"Temperatura extremadamente baja ({temp}°C)" : $"Temperatura extremadamente alta ({temp}°C)");
                }
                else if (temp >= 10 && temp < 14)
                {
                    score -= (int)((14 - temp) * 15);
                    details.Add($"Temperatura fría para el pecoreo ({temp}°C)");
                }
                else if (temp >= 14 && temp < 20)
                {
                    score -= (int)((20 - temp) * 5);
                    details.Add($"Temperatura fresca ({temp}°C)");
                }
                else if (temp > 30 && temp <= 35)
                {
                    score -= (int)((temp - 30) * 8);
                    details.Add($"Temperatura calurosa ({temp}°C)");
                }
                else if (temp > 35 && temp <= 38)
                {
                    score -= (int)((temp - 35) * 20);
                    details.Add($"Temperatura muy alta ({temp}°C)");
                }

                // 3. Wind Speed (optimal: < 15 km/h, max: 32 km/h)
                if (windSpd >= 32)
                {
                    score -= 80;
                    details.Add($"Viento fuerte perjudicial ({windSpd} km/h)");
                }
                else if (windSpd >= 24 && windSpd < 32)
                {
                    score -= (int)((windSpd - 24) * 5 + 27);
                    details.Add($"Viento moderado/fuerte ({windSpd} km/h)");
                }
                else if (windSpd >= 15 && windSpd < 24)
                {
                    score -= (int)((windSpd - 15) * 3);
                    details.Add($"Viento moderado ({windSpd} km/h)");
                }

                // 4. Humidity (optimal: 50-80%)
                if (hum < 50)
                {
                    score -= (int)((50 - hum) * 0.5);
                    details.Add($"Humedad relativa baja ({hum}%)");
                }
                else if (hum > 80)
                {
                    score -= (int)((hum - 80) * 1.5);
                    details.Add($"Humedad relativa alta ({hum}%)");
                }

                // 5. Shortwave Radiation (optimal: > 200 W/m²)
                if (rad == 0)
                {
                    score -= 60;
                    details.Add("Sin radiación solar (noche)");
                }
                else if (rad < 50)
                {
                    score -= 30;
                    details.Add($"Radiación solar muy baja / nublado ({rad} W/m²)");
                }
                else if (rad >= 50 && rad < 200)
                {
                    score -= (int)((200 - rad) * 0.15);
                    details.Add($"Radiación solar moderada / semisombra ({rad} W/m²)");
                }

                // 6. Soil Temperature (optimal: 15-30°C)
                if (soilTemp < 10 || soilTemp > 35)
                {
                    score -= 15;
                    details.Add($"Temperatura de suelo desfavorable ({soilTemp}°C)");
                }
                else if (soilTemp >= 10 && soilTemp < 15)
                {
                    score -= (int)((15 - soilTemp) * 2);
                    details.Add($"Temperatura de suelo fría ({soilTemp}°C)");
                }
                else if (soilTemp > 30 && soilTemp <= 35)
                {
                    score -= (int)((soilTemp - 30) * 2);
                    details.Add($"Temperatura de suelo alta ({soilTemp}°C)");
                }
            }

            score = Math.Clamp(score, 0, 100);

            string aptitud;
            string color;

            if (score >= 80)
            {
                aptitud = "Óptimo";
                color = "success";
            }
            else if (score >= 50)
            {
                aptitud = "Aceptable";
                color = "warning";
            }
            else
            {
                aptitud = "No recomendado";
                color = "danger";
            }

            if (details.Count == 0)
            {
                details.Add("Condiciones climáticas excelentes para el apiario.");
            }

            return new ClimaAptitudDto
            {
                Temperatura = temp,
                Humedad = hum,
                VelocidadViento = windSpd,
                DireccionViento = windDir,
                DireccionVientoCard = GetWindDirectionCard(windDir),
                Precipitacion = prec,
                Radiacion = rad,
                TemperaturaSuelo = soilTemp,
                Puntaje = score,
                Aptitud = aptitud,
                Color = color,
                Detalles = details
            };
        }

        private string GetWindDirectionCard(double degree)
        {
            string[] cardinals = { "N", "NE", "E", "SE", "S", "SO", "O", "NO", "N" };
            return cardinals[(int)Math.Round((degree % 360) / 45)];
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
}
