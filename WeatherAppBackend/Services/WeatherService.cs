using System.Net.Http;
using System.Text.Json;
using WeatherAppBackend.Models.DTOs;
using Microsoft.Extensions.Configuration;

namespace WeatherAppBackend.Services;

public class WeatherService
{
    private readonly string _apiKey;
    private readonly string _baseUrl;
    private readonly HttpClient _http;

    public WeatherService(IConfiguration config)
    {
        _apiKey = config["OpenWeather:ApiKey"]!;
        _baseUrl = config["OpenWeather:BaseUrl"]!;
        _http = new HttpClient();
    }

    public async Task<List<WeatherForecast>> Get5DayForecastAsync(string city)
    {
        var url = $"{_baseUrl}forecast?q={city}&appid={_apiKey}&units=metric";
        var response = await _http.GetAsync(url);

        if (!response.IsSuccessStatusCode)
        {
            return new List<WeatherForecast>();
        }

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);

        var forecastList = new List<WeatherForecast>();
        var list = doc.RootElement.GetProperty("list");
        var cityName = doc.RootElement.GetProperty("city").GetProperty("name").GetString() ?? city;

        foreach (var item in list.EnumerateArray())
        {
            var main = item.GetProperty("main");
            var weather = item.GetProperty("weather")[0];

            forecastList.Add(new WeatherForecast
            {
                Date = DateTime.Parse(item.GetProperty("dt_txt").GetString()!),
                Summary = weather.GetProperty("description").GetString()!,
                Temperature = main.GetProperty("temp").GetDouble(),
                FeelsLike = main.GetProperty("feels_like").GetDouble(),
                Humidity = main.GetProperty("humidity").GetInt32(),
                Icon = weather.GetProperty("icon").GetString()!,
                Name = cityName, // Set city name from API response
                Visibility = item.GetProperty("visibility").GetDouble() // Set visibility in meters
            });
        }

        return forecastList;
    }
}