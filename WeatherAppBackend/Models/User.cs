using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace WeatherAppBackend.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [BsonElement("email")]
        public string Email { get; set; } = string.Empty;

        [BsonElement("passwordHash")]
        public string PasswordHash { get; set; } = string.Empty;

        [BsonElement("favoriteCities")]
        public List<string> FavoriteCities { get; set; } = new List<string>();

        [BsonElement("searchHistory")]
        public List<string> SearchHistory { get; set; } = new List<string>();

        // Method to ensure indexes (no BsonIgnore needed)
        public void EnsureIndexes(IMongoCollection<User> collection)
        {
            var indexKeys = Builders<User>.IndexKeys.Ascending(u => u.Email);
            collection.Indexes.CreateOne(new CreateIndexModel<User>(indexKeys, new CreateIndexOptions { Unique = true }));
        }
    }
}