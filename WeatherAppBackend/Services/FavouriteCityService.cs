using MongoDB.Driver;
using WeatherAppBackend.Models;

namespace WeatherAppBackend.Services
{
    public class FavouriteCityService
    {
        private readonly IMongoCollection<FavouriteCity> _collection;

        public FavouriteCityService(IConfiguration config)
        {
            var client = new MongoClient(config["MongoDB:ConnectionString"]);
            var database = client.GetDatabase(config["MongoDB:DatabaseName"]);
            _collection = database.GetCollection<FavouriteCity>(config["MongoDB:FavoritesCollection"]);
        }

        public async Task AddFavoriteAsync(string email, string city)
        {
            var existing = await _collection.Find(x => x.Email == email && x.City == city).FirstOrDefaultAsync();
            if (existing == null)
            {
                await _collection.InsertOneAsync(new FavouriteCity { Email = email, City = city });
            }
        }

        public async Task<List<string>> GetFavoritesAsync(string email)
        {
            var favorites = await _collection.Find(f => f.Email == email).ToListAsync();
            return favorites.Select(f => f.City).ToList();
        }

        public async Task RemoveFavoriteAsync(string email, string city)
        {
            await _collection.DeleteOneAsync(f => f.Email == email && f.City == city);
        }
    }
}

