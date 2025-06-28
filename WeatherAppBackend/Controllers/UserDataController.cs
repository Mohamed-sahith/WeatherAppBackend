using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WeatherAppBackend.Services;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;

namespace WeatherAppBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserDataController : ControllerBase
    {
        private readonly UserDataService _userDataService;
        private readonly ILogger<UserDataController> _logger;

        public UserDataController(UserDataService userDataService, ILogger<UserDataController> logger)
        {
            _userDataService = userDataService ?? throw new ArgumentNullException(nameof(userDataService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpPost("add-city")]
        public async Task<IActionResult> AddFavoriteCity([FromBody] string city)
        {
            if (string.IsNullOrWhiteSpace(city))
            {
                return BadRequest("City cannot be null or empty.");
            }

            var email = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrWhiteSpace(email))
            {
                return Unauthorized("User email not found in token.");
            }

            try
            {
                await _userDataService.AddFavoriteCityAsync(email, city);
                _logger.LogInformation("Successfully added {City} to favorites for {Email}", city, email);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding {City} to favorites for {Email}", city, email);
                return StatusCode(500, "Internal server error while adding favorite city.");
            }
        }

        [HttpGet("cities")]
        public async Task<ActionResult<List<string>>> GetFavoriteCities()
        {
            var emailFromToken = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrWhiteSpace(emailFromToken))
            {
                return Unauthorized("User email not found in token.");
            }

            // Prefer query parameter if provided, fallback to token
            var email = HttpContext.Request.Query["email"].FirstOrDefault() ?? emailFromToken;

            try
            {
                var cities = await _userDataService.GetFavoriteCitiesAsync(email);
                _logger.LogInformation("Fetched {Count} favorite cities for {Email}", cities?.Count ?? 0, email);
                return Ok(cities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching favorite cities for {Email}", email);
                return StatusCode(500, "Internal server error while fetching favorite cities.");
            }
        }

        [HttpDelete("remove-city")]
        public async Task<IActionResult> RemoveFavoriteCity([FromBody] string city)
        {
            if (string.IsNullOrWhiteSpace(city))
            {
                return BadRequest("City cannot be null or empty.");
            }

            var email = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrWhiteSpace(email))
            {
                return Unauthorized("User email not found in token.");
            }

            try
            {
                await _userDataService.RemoveFavoriteCityAsync(email, city);
                _logger.LogInformation("Successfully removed {City} from favorites for {Email}", city, email);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing {City} from favorites for {Email}", city, email);
                return StatusCode(500, "Internal server error while removing favorite city.");
            }
        }
    }
}
        

//        [HttpPost("add-search")]
//        public async Task<IActionResult> AddSearchHistory([FromBody] SearchHistoryRequest request)
//        {
//            if (!ModelState.IsValid)
//            {
//                return BadRequest(ModelState);
//            }

//            var email = User.FindFirst(ClaimTypes.Name)?.Value;
//            if (string.IsNullOrWhiteSpace(email))
//            {
//                return Unauthorized("User email not found in token.");
//            }

//            try
//            {
//                await _userDataService.AddSearchHistoryAsync(email, request.City);
//                _logger.LogInformation("Successfully added {City} to search history for {Email}", request.City, email);
//                return Ok();
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error adding {City} to search history for {Email}", request.City, email);
//                return StatusCode(500, "Internal server error while adding search history.");
//            }
//        }

//        [HttpGet("search-history")]
//        public async Task<ActionResult<List<string>>> GetSearchHistory()
//        {
//            var email = User.FindFirst(ClaimTypes.Name)?.Value;
//            if (string.IsNullOrWhiteSpace(email))
//            {
//                return Unauthorized("User email not found in token.");
//            }

//            try
//            {
//                var searches = await _userDataService.GetSearchHistoryAsync(email);
//                _logger.LogInformation("Fetched {Count} search history entries for {Email}", searches?.Count ?? 0, email);
//                return Ok(searches);
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error fetching search history for {Email}", email);
//                return StatusCode(500, "Internal server error while fetching search history.");
//            }
//        }
//    }
//    public class SearchHistoryRequest
//    {
//        [Required]
//        public string City { get; set; }
//    }
//}