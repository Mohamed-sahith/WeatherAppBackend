namespace WeatherAppBackend.Models.DTOs;

public class WeatherForecast
{
    public DateTime Date { get; set; }
    public string Summary { get; set; } = string.Empty;
    public double Temperature { get; set; }
    public double FeelsLike { get; set; }
    public int Humidity { get; set; }
    public string Icon { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty; // Add Name for city name
    public double Visibility { get; set; } // Add Visibility in meters
}