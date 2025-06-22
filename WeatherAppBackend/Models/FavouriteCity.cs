using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace WeatherAppBackend.Models
{
    public class FavouriteCity
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public string Email { get; set; } = string.Empty;

        public string City { get; set; } = string.Empty;

        public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    }
}
