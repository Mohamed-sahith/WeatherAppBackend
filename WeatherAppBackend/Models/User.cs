using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace WeatherAppBackend.Models;

public class User
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;

    public List<string> FavoriteCities { get; set; } = new(); // ✅ New field
}
