using MongoDB.Driver;
using WeatherAppBackend.Models;

namespace WeatherAppBackend.Services
{
    public class UserDataService
    {
        private readonly IMongoCollection<User> _users;
        private readonly ILogger<UserDataService> _logger;

        public UserDataService(IConfiguration config, ILogger<UserDataService> logger)
        {
            var client = new MongoClient(config["MongoDB:ConnectionString"] ?? throw new ArgumentNullException("MongoDB:ConnectionString"));
            var database = client.GetDatabase(config["MongoDB:DatabaseName"] ?? throw new ArgumentNullException("MongoDB:DatabaseName"));
            _users = database.GetCollection<User>("Users") ?? throw new InvalidOperationException("Failed to initialize Users collection.");
            _logger = logger;
        }

        public IMongoCollection<User> UsersCollection => _users; // Expose _users as a read-only property

        public async Task AddFavoriteCityAsync(string email, string city)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(city))
            {
                return;
            }

            var user = await _users.Find(u => u.Email == email).FirstOrDefaultAsync();
            if (user == null)
            {
                _logger.LogWarning("User not found for email: {Email}", email);
                return;
            }

            var filter = Builders<User>.Filter.Eq(u => u.Email, email);
            var update = Builders<User>.Update.AddToSet(u => u.FavoriteCities, city.Trim());
            await _users.UpdateOneAsync(filter, update);
        }

        public async Task<List<string>> GetFavoriteCitiesAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return new List<string>();
            }

            var user = await _users.Find(u => u.Email == email).FirstOrDefaultAsync();
            return user?.FavoriteCities ?? new List<string>();
        }

        public async Task RemoveFavoriteCityAsync(string email, string city)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(city))
            {
                return;
            }

            var user = await _users.Find(u => u.Email == email).FirstOrDefaultAsync();
            if (user == null)
            {
                _logger.LogWarning("User not found for email: {Email}", email);
                return;
            }

            var filter = Builders<User>.Filter.Eq(u => u.Email, email);
            var update = Builders<User>.Update.Pull(u => u.FavoriteCities, city.Trim());
            await _users.UpdateOneAsync(filter, update);
        }

        public async Task AddSearchHistoryAsync(string email, string city)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(city))
            {
                _logger.LogWarning("Invalid input: email={Email}, city={City}", email, city);
                throw new ArgumentException("Email and city cannot be null or empty.");
            }

            var user = await _users.Find(u => u.Email == email).FirstOrDefaultAsync();
            if (user == null)
            {
                _logger.LogWarning("User not found for email: {Email}", email);
                throw new InvalidOperationException($"User with email {email} not found.");
            }

            var filter = Builders<User>.Filter.Eq(u => u.Email, email);

            try
            {
                if (user.SearchHistory == null)
                {
                    // Step 1: Set initial list with this city
                    var initUpdate = Builders<User>.Update.Set(u => u.SearchHistory, new List<string> { city.Trim() });
                    await _users.UpdateOneAsync(filter, initUpdate);
                }
                else
                {
                    // Step 2: Pull existing and push new city
                    var update = Builders<User>.Update
                        .Pull(u => u.SearchHistory, city.Trim())
                        .Push(u => u.SearchHistory, city.Trim());

                    var result = await _users.UpdateOneAsync(filter, update);

                    if (result.ModifiedCount == 0)
                    {
                        _logger.LogWarning("No modification made to search history for {Email}", email);
                    }
                    else
                    {
                        _logger.LogInformation("Added city {City} to search history for {Email}", city, email);
                    }
                }
            }
            catch (MongoException ex)
            {
                _logger.LogError(ex, "MongoDB error adding city {City} to search history for {Email}", city, email);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding city {City} to search history for {Email}", city, email);
                throw;
            }
        }


        public async Task<List<string>> GetSearchHistoryAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return new List<string>();
            }

            var user = await _users.Find(u => u.Email == email).FirstOrDefaultAsync();
            return user?.SearchHistory ?? new List<string>();
        }
    }
}