using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using WeatherAppBackend.Models;

namespace WeatherAppBackend.Services
{
    public class UnsplashService
    {
        private readonly HttpClient _httpClient;
        private readonly string _accessKey;
        private readonly string _baseUrl;

        public UnsplashService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClient = httpClientFactory.CreateClient();
            _accessKey = configuration["Unsplash:AccessKey"];
            _baseUrl = configuration["Unsplash:BaseUrl"]?.TrimEnd('/') ?? "https://api.unsplash.com";
        }

        public async Task<UnsplashPhoto> GetCityImageAsync(string city)
        {
            if (string.IsNullOrWhiteSpace(city))
            {
                return null;
            }

            var url = $"{_baseUrl}/search/photos?query={Uri.EscapeDataString(city)}&client_id={_accessKey}&per_page=1&orientation=landscape";

            try
            {
                var response = await _httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var searchResponse = JsonSerializer.Deserialize<UnsplashSearchResponse>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return searchResponse?.Results?.FirstOrDefault();
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    throw new HttpRequestException("Rate limit exceeded or unauthorized. Please check your Unsplash API key and usage limits.");
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                // Log exception (e.g., using ILogger if injected)
                Console.WriteLine($"Error fetching image: {ex.Message}");
                return null;
            }
        }
    }
}