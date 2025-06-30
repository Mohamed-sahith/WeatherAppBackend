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
        private readonly GeoCityService _geoCityService;

        // ✅ Single unified constructor
        public WeatherController(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            WeatherService weatherService,
            UnsplashService unsplashService,
            GeoCityService geoCityService)
        {
            _httpClient = httpClientFactory.CreateClient();
            _configuration = configuration;
            _weatherService = weatherService;
            _unsplashService = unsplashService;
            _geoCityService = geoCityService;
        }

        [HttpGet("forecast/{city}")]
        public async Task<ActionResult<List<WeatherForecast>>> GetForecast(string city)
        {
            if (string.IsNullOrWhiteSpace(city))
                return BadRequest("City name cannot be empty.");

            try
            {
                var forecasts = await _weatherService.GetForecast(city);
                if (forecasts == null || !forecasts.Any())
                    return NotFound("No forecast data available.");
                return Ok(forecasts);
            }
            catch (Exception)
            {
                return StatusCode(500, "Error fetching weather data.");
            }
        }

        [HttpGet("city-image/{city}")]
        public async Task<ActionResult<CityImageResponse>> GetCityImage(string city)
        {
            if (string.IsNullOrWhiteSpace(city))
                return BadRequest("City name cannot be empty.");

            try
            {
                var cityImage = await _unsplashService.GetCityImageAsync(city);
                if (cityImage == null)
                    return NotFound("No image found for the city.");
                return Ok(cityImage);
            }
            catch (Exception)
            {
                return StatusCode(500, "Error fetching city image.");
            }
        }

        [HttpGet("search-cities")]
        public async Task<IActionResult> SearchCities([FromQuery] string q)
        {
            if (string.IsNullOrWhiteSpace(q)) return BadRequest("Query required.");
            var results = await _geoCityService.SearchCitiesAsync(q);
            return Ok(results);
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