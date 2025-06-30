using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;

namespace WeatherAppBackend.Services
{
    public class GeoCityService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _geoBaseUrl;

        public GeoCityService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _apiKey = config["WeatherApi:ApiKey"];
            _geoBaseUrl = config["WeatherApi:GeoBaseUrl"];
        }

        public async Task<List<GeoCityResult>> SearchCitiesAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return new List<GeoCityResult>();

            string url = $"{_geoBaseUrl}direct?q={query}&limit=10&appid={_apiKey}"; // Increased limit to 10
            var results = await _httpClient.GetFromJsonAsync<List<GeoCityResult>>(url) ?? new List<GeoCityResult>();

            // Switch to contains filter for testing partial matches
            var normalizedQuery = query.ToLower();
            return results
                .Where(r => r.Name.ToLower().Contains(normalizedQuery)) // Changed to Contains
                .GroupBy(r => r.Name)
                .Select(g => g.First())
                .ToList();
        }
    }
}