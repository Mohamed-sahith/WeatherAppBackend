using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WeatherAppBackend.Services;

namespace WeatherAppBackend.Controllers;


[ApiController]
[Route("api/[controller]")]
public class FavoritesController : ControllerBase
{
    private readonly FavouriteCityService _service;

    public FavoritesController(FavouriteCityService service)
    {
        _service = service;
    }

    [HttpPost("add")]
    public async Task<IActionResult> Add([FromBody] FavouriteCityRequest request)
    {
        await _service.AddFavoriteAsync(request.Email, request.City);
        return Ok();
    }

    [HttpGet("{email}")]
    public async Task<IActionResult> Get(string email)
    {
        var cities = await _service.GetFavoritesAsync(email);
        return Ok(cities);
    }

    [HttpDelete("remove")]
    public async Task<IActionResult> Remove([FromBody] FavouriteCityRequest request)
    {
        await _service.RemoveFavoriteAsync(request.Email, request.City);
        return Ok();
    }
}

public class FavouriteCityRequest
{
    public string Email { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
}

