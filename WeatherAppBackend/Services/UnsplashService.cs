using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using WeatherAppBackend.Models;
using WeatherAppBackend.Models.DTOs;

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

        public async Task<CityImageResponse> GetCityImageAsync(string city)
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

                    var photo = searchResponse?.Results?.FirstOrDefault();
                    if (photo == null)
                    {
                        Console.WriteLine($"UnsplashService: No image found for {city}.");
                        return null;
                    }

                    return new CityImageResponse
                    {
                        ImageUrl = photo.Urls?.Regular ?? string.Empty,
                        AltDescription = photo.AltDescription ?? $"Image of {city}",
                        Photographer = photo.User?.Name ?? "Unknown",
                        PhotographerLink = photo.User?.Links?.Html ?? string.Empty
                    };
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    throw new HttpRequestException("Rate limit exceeded or unauthorized. Please check your Unsplash API key and usage limits.");
                }
                else
                {
                    Console.WriteLine($"UnsplashService: API error for {city}: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UnsplashService: Error fetching image for {city}: {ex.Message}");
                return null;
            }
        }
    }
}