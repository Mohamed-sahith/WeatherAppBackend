using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WeatherAppBackend.Services;

namespace WeatherAppBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserDataController : ControllerBase
{
    private readonly UserDataService _dataService;

    public UserDataController(UserDataService dataService)
    {
        _dataService = dataService;
    }

    [Authorize]
    [HttpPost("add-city")]
    public async Task<IActionResult> AddCity([FromBody] string city)
    {
        var email = User.Identity?.Name;
        if (email == null) return Unauthorized();

        await _dataService.AddFavoriteCityAsync(email, city);
        return Ok("City added");
    }

    [Authorize]
    [HttpDelete("remove-city")]
    public async Task<IActionResult> RemoveCity([FromBody] string city)
    {
        var email = User.Identity?.Name;
        if (email == null) return Unauthorized();

        await _dataService.RemoveFavoriteCityAsync(email, city);
        return Ok("City removed");
    }

    [Authorize]
    [HttpGet("cities")]
    public async Task<IActionResult> GetCities()
    {
        var email = User.Identity?.Name;
        if (email == null) return Unauthorized();

        var cities = await _dataService.GetFavoriteCitiesAsync(email);
        return Ok(cities);
    }
}
