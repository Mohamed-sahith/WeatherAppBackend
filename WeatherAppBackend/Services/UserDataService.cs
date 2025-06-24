using MongoDB.Driver;
using WeatherAppBackend.Models;

namespace WeatherAppBackend.Services;

public class UserDataService
{
    private readonly IMongoCollection<User> _users;
    private readonly ILogger<UserDataService> _logger;

    public UserDataService(IConfiguration config, ILogger<UserDataService> logger)
    {
        try
        {
            var client = new MongoClient(config["MongoDB:ConnectionString"]);
            var database = client.GetDatabase(config["MongoDB:DatabaseName"]);
            _users = database.GetCollection<User>("Users");
            _logger = logger;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to initialize MongoDB connection.");
            throw;
        }
    }

    public async Task AddFavoriteCityAsync(string email, string city)
    {
        try
        {
            var user = await _users.Find(u => u.Email == email).FirstOrDefaultAsync();
            if (user == null)
            {
                _logger.LogWarning("User not found for email: {Email}", email);
                return; // Or throw an exception if user must exist
            }

            var filter = Builders<User>.Filter.Eq(u => u.Email, email);
            var update = Builders<User>.Update.AddToSet(u => u.FavoriteCities, city);
            await _users.UpdateOneAsync(filter, update);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding city {City} for user {Email}", city, email);
            throw;
        }
    }

    public async Task<List<string>> GetFavoriteCitiesAsync(string email)
    {
        try
        {
            var user = await _users.Find(u => u.Email == email).FirstOrDefaultAsync();
            return user?.FavoriteCities ?? new List<string>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching favorite cities for user {Email}", email);
            throw;
        }
    }

    public async Task RemoveFavoriteCityAsync(string email, string city)
    {
        try
        {
            var user = await _users.Find(u => u.Email == email).FirstOrDefaultAsync();
            if (user == null)
            {
                _logger.LogWarning("User not found for email: {Email}", email);
                return; // Or throw an exception if user must exist
            }

            var filter = Builders<User>.Filter.Eq(u => u.Email, email);
            var update = Builders<User>.Update.Pull(u => u.FavoriteCities, city);
            await _users.UpdateOneAsync(filter, update);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing city {City} for user {Email}", city, email);
            throw;
        }
    }
}