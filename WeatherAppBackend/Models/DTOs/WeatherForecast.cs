namespace WeatherAppBackend.Models.DTOs;

    public class WeatherForecast
    {
        public DateTime Date { get; set; }
        public string Name { get; set; } = string.Empty;
        public double Temperature { get; set; }
        public string Summary { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public double FeelsLike { get; set; }
        public int Humidity { get; set; }
        public int Visibility { get; set; }
        public double WindSpeed { get; set; }
        public double Pressure { get; set; } // Added for atmospheric pressure in hPa
        public DateTime Sunrise { get; set; } // Added for sunrise time
        public DateTime Sunset { get; set; } // Added for sunset time
    }
