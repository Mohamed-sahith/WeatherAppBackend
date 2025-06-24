using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WeatherAppBackend.Services;

namespace WeatherAppBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserDataController : ControllerBase
{
    private readonly UserDataService _dataService;
    private readonly ILogger<UserDataController> _logger;

    public UserDataController(UserDataService dataService, ILogger<UserDataController> logger)
    {
        _dataService = dataService;
        _logger = logger;
    }

    [Authorize]
    [HttpPost("add-city")]
    public async Task<IActionResult> AddCity([FromBody] string city)
    {
        if (string.IsNullOrWhiteSpace(city))
        {
            return BadRequest("City name cannot be empty.");
        }

        var email = User.Identity?.Name;
        if (email == null)
        {
            _logger.LogWarning("Unauthorized attempt to add city: {City}", city);
            return Unauthorized(new { Message = "User not authenticated." });
        }

        await _dataService.AddFavoriteCityAsync(email, city);
        return Ok("City added");
    }

    [Authorize]
    [HttpDelete("remove-city")]
    public async Task<IActionResult> RemoveCity([FromBody] string city)
    {
        if (string.IsNullOrWhiteSpace(city))
        {
            return BadRequest("City name cannot be empty.");
        }

        var email = User.Identity?.Name;
        if (email == null)
        {
            _logger.LogWarning("Unauthorized attempt to remove city: {City}", city);
            return Unauthorized(new { Message = "User not authenticated." });
        }

        await _dataService.RemoveFavoriteCityAsync(email, city);
        return Ok("City removed");
    }

    [Authorize]
    [HttpGet("cities")]
    public async Task<IActionResult> GetCities()
    {
        var email = User.Identity?.Name;
        if (email == null)
        {
            _logger.LogWarning("Unauthorized attempt to get cities");
            return Unauthorized(new { Message = "User not authenticated." });
        }

        var cities = await _dataService.GetFavoriteCitiesAsync(email);
        return Ok(cities);
    }
}