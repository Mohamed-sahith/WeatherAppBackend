using System.Text.Json.Serialization;

namespace WeatherAppBackend.Models
{
    public class UnsplashPhoto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("urls")]
        public Urls Urls { get; set; }

        [JsonPropertyName("user")]
        public UnsplashUser User { get; set; }

        [JsonPropertyName("alt_description")]
        public string AltDescription { get; set; }
    }

    public class Urls
    {
        [JsonPropertyName("regular")]
        public string Regular { get; set; }

        [JsonPropertyName("small")]
        public string Small { get; set; }

        [JsonPropertyName("thumb")]
        public string Thumb { get; set; }
    }

    public class UnsplashUser
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("links")]
        public Links Links { get; set; }
    }

    public class Links
    {
        [JsonPropertyName("html")]
        public string Html { get; set; }
    }

    public class UnsplashSearchResponse
    {
        [JsonPropertyName("results")]
        public List<UnsplashPhoto> Results { get; set; }
    }
}