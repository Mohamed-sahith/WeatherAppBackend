using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using WeatherAppBackend.Models.DTOs;

namespace WeatherAppBackend.Services
{
    public class WeatherService
    {
        private readonly string _apiKey;
        private readonly string _baseUrl;
        private readonly HttpClient _httpClient;
        private readonly ILogger<WeatherService> _logger;

        public WeatherService(IHttpClientFactory httpClientFactory, IConfiguration config, ILogger<WeatherService> logger)
        {
            _httpClient = httpClientFactory.CreateClient();
            _apiKey = config["WeatherApi:ApiKey"]!;
            _baseUrl = config["WeatherApi:BaseUrl"] ?? "https://api.openweathermap.org/data/2.5/";
            _logger = logger;
        }

        public async Task<List<WeatherForecast>> GetForecast(string city)
        {
            try
            {
                // Fetch current weather data
                var currentUrl = $"{_baseUrl}weather?q={Uri.EscapeDataString(city)}&appid={_apiKey}&units=metric";
                var currentResponse = await _httpClient.GetAsync(currentUrl);

                WeatherForecast? currentForecast = null;
                if (currentResponse.IsSuccessStatusCode)
                {
                    var currentJson = await currentResponse.Content.ReadAsStringAsync();
                    using var currentDoc = JsonDocument.Parse(currentJson);

                    var main = currentDoc.RootElement.GetProperty("main");
                    var weather = currentDoc.RootElement.GetProperty("weather")[0];
                    var wind = currentDoc.RootElement.GetProperty("wind");
                    var sys = currentDoc.RootElement.GetProperty("sys");
                    var cityName = currentDoc.RootElement.GetProperty("name").GetString() ?? city;

                    var sunriseUnix = sys.GetProperty("sunrise").GetInt64();
                    var sunsetUnix = sys.GetProperty("sunset").GetInt64();
                    var sunrise = DateTimeOffset.FromUnixTimeSeconds(sunriseUnix).DateTime;
                    var sunset = DateTimeOffset.FromUnixTimeSeconds(sunsetUnix).DateTime;

                    currentForecast = new WeatherForecast
                    {
                        Date = DateTime.Now,
                        Name = cityName,
                        Temperature = main.GetProperty("temp").GetDouble(),
                        FeelsLike = main.GetProperty("feels_like").GetDouble(),
                        Humidity = main.GetProperty("humidity").GetInt32(),
                        Pressure = (int)main.GetProperty("pressure").GetDouble(), // Explicit cast
                        Visibility = (int)currentDoc.RootElement.GetProperty("visibility").GetDouble(), // Explicit cast
                        WindSpeed = wind.GetProperty("speed").GetDouble(),
                        Summary = weather.GetProperty("description").GetString()!,
                        Icon = weather.GetProperty("icon").GetString()!,
                        Sunrise = sunrise,
                        Sunset = sunset
                    };
                }
                else
                {
                    _logger.LogWarning("Failed to fetch current weather for {City}: {StatusCode}", city, currentResponse.StatusCode);
                }

                // Fetch 5-day forecast data
                var forecastUrl = $"{_baseUrl}forecast?q={Uri.EscapeDataString(city)}&appid={_apiKey}&units=metric";
                var forecastResponse = await _httpClient.GetAsync(forecastUrl);

                if (!forecastResponse.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to fetch forecast for {City}: {StatusCode}", city, forecastResponse.StatusCode);
                    return currentForecast != null ? new List<WeatherForecast> { currentForecast } : new List<WeatherForecast>();
                }

                var forecastJson = await forecastResponse.Content.ReadAsStringAsync();
                using var forecastDoc = JsonDocument.Parse(forecastJson);

                var forecastList = new List<WeatherForecast>();
                var list = forecastDoc.RootElement.GetProperty("list");
                var forecastCityName = forecastDoc.RootElement.GetProperty("city").GetProperty("name").GetString() ?? city;

                foreach (var item in list.EnumerateArray())
                {
                    var main = item.GetProperty("main");
                    var weather = item.GetProperty("weather")[0];
                    var wind = item.GetProperty("wind");

                    forecastList.Add(new WeatherForecast
                    {
                        Date = DateTime.Parse(item.GetProperty("dt_txt").GetString()!),
                        Name = forecastCityName,
                        Temperature = main.GetProperty("temp").GetDouble(),
                        FeelsLike = main.GetProperty("feels_like").GetDouble(),
                        Humidity = main.GetProperty("humidity").GetInt32(),
                        Pressure = (int)main.GetProperty("pressure").GetDouble(), // Explicit cast
                        Visibility = (int)item.GetProperty("visibility").GetDouble(), // Explicit cast
                        WindSpeed = wind.GetProperty("speed").GetDouble(),
                        Summary = weather.GetProperty("description").GetString()!,
                        Icon = weather.GetProperty("icon").GetString()!,
                        Sunrise = DateTime.MinValue,
                        Sunset = DateTime.MinValue
                    });
                }

                if (currentForecast != null)
                {
                    forecastList.Insert(0, currentForecast);
                }

                return forecastList;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching weather forecast for {City}", city);
                return new List<WeatherForecast>();
            }
        }
    }
}