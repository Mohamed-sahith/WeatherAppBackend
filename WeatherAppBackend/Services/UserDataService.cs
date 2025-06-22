using MongoDB.Driver;
using WeatherAppBackend.Models;

namespace WeatherAppBackend.Services;

public class UserDataService
{
    private readonly IMongoCollection<User> _users;

    public UserDataService(IConfiguration config)
    {
        var client = new MongoClient(config["MongoDB:ConnectionString"]);
        var database = client.GetDatabase(config["MongoDB:DatabaseName"]);
        _users = database.GetCollection<User>("Users");
    }

    public async Task AddFavoriteCityAsync(string email, string city)
    {
        var filter = Builders<User>.Filter.Eq(u => u.Email, email);
        var update = Builders<User>.Update.AddToSet(u => u.FavoriteCities, city);
        await _users.UpdateOneAsync(filter, update);
    }

    public async Task<List<string>> GetFavoriteCitiesAsync(string email)
    {
        var user = await _users.Find(u => u.Email == email).FirstOrDefaultAsync();
        return user?.FavoriteCities ?? new List<string>();
    }
    public async Task RemoveFavoriteCityAsync(string email, string city)
    {
        var filter = Builders<User>.Filter.Eq(u => u.Email, email);
        var update = Builders<User>.Update.Pull(u => u.FavoriteCities, city);
        await _users.UpdateOneAsync(filter, update);
    }

}
