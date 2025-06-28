namespace WeatherAppBackend.Models.DTOs;

public class CityImageResponse
{
    public string ImageUrl { get; set; } = string.Empty;
    public string AltDescription { get; set; } = string.Empty;
    public string Photographer { get; set; } = string.Empty;
    public string PhotographerLink { get; set; } = string.Empty;
}