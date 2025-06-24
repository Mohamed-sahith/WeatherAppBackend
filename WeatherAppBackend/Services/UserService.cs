using MongoDB.Driver;
using WeatherAppBackend.Models;

namespace WeatherAppBackend.Services;

public class UserService
{
    private readonly IMongoCollection<User> _users;
    private readonly ILogger<UserService> _logger;

    public UserService(IConfiguration config, ILogger<UserService> logger)
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
            _logger.LogError(ex, "Failed to initialize MongoDB connection.");
            throw;
        }
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        try
        {
            return await _users.Find(u => u.Email == email).FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching user by email: {Email}", email);
            return null;
        }
    }

    public async Task CreateAsync(User user)
    {
        try
        {
            await _users.InsertOneAsync(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user: {Email}", user.Email);
            throw;
        }
    }

    public async Task UpdateUserAsync(User user)
    {
        try
        {
            var filter = Builders<User>.Filter.Eq(u => u.Id, user.Id);
            await _users.ReplaceOneAsync(filter, user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user: {Email}", user.Email);
            throw;
        }
    }
}