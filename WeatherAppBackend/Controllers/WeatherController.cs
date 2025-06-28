using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net.Http.Json;
using WeatherAppBackend.Models;
using WeatherAppBackend.Models.DTOs;
using WeatherAppBackend.Services;

namespace WeatherAppBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class WeatherController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly WeatherService _weatherService;
        private readonly UnsplashService _unsplashService;

        public WeatherController(IHttpClientFactory httpClientFactory, IConfiguration configuration, WeatherService weatherService, UnsplashService unsplashService)
        {
            _httpClient = httpClientFactory.CreateClient();
            _configuration = configuration;
            _weatherService = weatherService;
            _unsplashService = unsplashService;
        }

        [HttpGet("forecast/{city}")]
        public async Task<ActionResult<List<WeatherForecast>>> GetForecast(string city)
        {
            if (string.IsNullOrWhiteSpace(city))
            {
                Console.WriteLine("GetForecast: City name is null or empty.");
                return BadRequest("City name cannot be empty.");
            }

            try
            {
                var forecasts = await _weatherService.GetForecast(city);
                if (forecasts == null || !forecasts.Any())
                {
                    Console.WriteLine($"GetForecast: No forecast data for {city}.");
                    return NotFound("No forecast data available.");
                }
                return Ok(forecasts);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetForecast: Error fetching forecast for {city}: {ex.Message}");
                return StatusCode(500, "Error fetching weather data.");
            }
        }

        [HttpGet("city-image/{city}")]
        public async Task<ActionResult<CityImageResponse>> GetCityImage(string city)
        {
            if (string.IsNullOrWhiteSpace(city))
            {
                Console.WriteLine("GetCityImage: City name is null or empty.");
                return BadRequest("City name cannot be empty.");
            }

            try
            {
                var cityImage = await _unsplashService.GetCityImageAsync(city);
                if (cityImage == null)
                {
                    Console.WriteLine($"GetCityImage: No image found for {city}.");
                    return NotFound("No image found for the city.");
                }
                return Ok(cityImage);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetCityImage: Error fetching image for {city}: {ex.Message}");
                return StatusCode(500, "Error fetching city image.");
            }
        }
    }

    // Updated WeatherForecast model to include Sunset
    public class WeatherForecast
    {
        public DateTime Date { get; set; }
        public double Temperature { get; set; }
        public double FeelsLike { get; set; }
        public int Humidity { get; set; }
        public int Pressure { get; set; }
        public double WindSpeed { get; set; }
        public int Visibility { get; set; }
        public string Summary { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public DateTime Sunrise { get; set; }
        public DateTime Sunset { get; set; }
    }

    public class OpenWeatherResponse
    {
        public string Cod { get; set; } = string.Empty;
        public string? Message { get; set; }
        public List<OpenWeatherForecast> List { get; set; } = new();
        public CityInfo City { get; set; } = new();
    }

    public class OpenWeatherForecast
    {
        public long Dt { get; set; }
        public MainData? Main { get; set; }
        public List<WeatherData>? Weather { get; set; }
        public WindData? Wind { get; set; }
        public int Visibility { get; set; }
    }

    public class MainData
    {
        public double Temp { get; set; }
        public double FeelsLike { get; set; }
        public int Humidity { get; set; }
        public int Pressure { get; set; }
    }

    public class WeatherData
    {
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
    }

    public class WindData
    {
        public double Speed { get; set; }
    }

    public class CityInfo
    {
        public string Name { get; set; } = string.Empty;
        public long Sunrise { get; set; }
    }

    // Updated Unsplash models to align with previous suggestions
    public class UnsplashSearchResponse
    {
        public int Total { get; set; }
        public int TotalPages { get; set; }
        public List<UnsplashPhoto> Results { get; set; } = new();
    }

    public class UnsplashPhoto
    {
        public string Id { get; set; } = string.Empty;
        public Urls Urls { get; set; } = new();
        public UnsplashUser User { get; set; } = new();
        public string AltDescription { get; set; } = string.Empty;
    }

    public class Urls
    {
        public string Regular { get; set; } = string.Empty;
        public string Small { get; set; } = string.Empty;
        public string Thumb { get; set; } = string.Empty;
    }

    public class UnsplashUser
    {
        public string Name { get; set; } = string.Empty;
        public Links Links { get; set; } = new();
    }

    public class Links
    {
        public string Html { get; set; } = string.Empty;
    }
}