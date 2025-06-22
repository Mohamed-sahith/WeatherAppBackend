using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WeatherAppBackend.Helpers;
using WeatherAppBackend.Models;
using WeatherAppBackend.Models.DTOs;
using WeatherAppBackend.Services;

namespace WeatherAppBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserService _userService;
    private readonly JwtHelper _jwt;

    public AuthController(UserService userService, JwtHelper jwt)
    {
        _userService = userService;
        _jwt = jwt;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var existing = await _userService.GetByEmailAsync(request.Email);
        if (existing != null)
            return BadRequest("User already exists");

        var user = new User
        {
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
        };

        await _userService.CreateAsync(user);
        return Ok("User registered");
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _userService.GetByEmailAsync(request.Email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return Unauthorized("Invalid credentials");

        var token = _jwt.GenerateToken(user.Email);
        return Ok(new { token, email = user.Email });
    }

    [HttpGet("me")]
    [Authorize]
    public IActionResult Me()
    {
        return Ok(new { Email = User.Identity?.Name });
    }
}
