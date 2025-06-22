using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WeatherAppBackend.Services;
using WeatherAppBackend.Models.DTOs;

namespace WeatherAppBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WeatherController : ControllerBase
    {
        private readonly WeatherService _weather;
        private readonly UnsplashService _unsplashService;

        public WeatherController(WeatherService weather, UnsplashService unsplashService)
        {
            _weather = weather;
            _unsplashService = unsplashService;
        }

        // GET: /api/weather/forecast/chennai
        [HttpGet("forecast/{city}")]
        [Authorize]
        public async Task<ActionResult<List<WeatherForecast>>> GetForecast(string city)
        {
            var forecast = await _weather.Get5DayForecastAsync(city);

            if (forecast == null || forecast.Count == 0)
                return NotFound(new { message = $"Weather data not found for '{city}'" });

            return Ok(forecast);
        }

        // GET: /api/weather/city-image/chennai
        [HttpGet("city-image/{city}")]
        [Authorize]
        public async Task<IActionResult> GetCityImage(string city)
        {
            if (string.IsNullOrWhiteSpace(city))
            {
                return BadRequest(new { message = "City name is required." });
            }

            var image = await _unsplashService.GetCityImageAsync(city);
            if (image == null)
            {
                return NotFound(new { message = $"No image found for '{city}'." });
            }

            return Ok(new
            {
                ImageUrl = image.Urls.Regular,
                AltDescription = image.AltDescription,
                Photographer = image.User.Name,
                PhotographerLink = image.User.Links.Html
            });
        }
    }
}