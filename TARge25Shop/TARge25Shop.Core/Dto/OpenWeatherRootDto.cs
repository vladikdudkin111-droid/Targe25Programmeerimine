using System.Text.Json.Serialization;

namespace TARge25Shop.Core.Dto
{
    // Current Weather Data API vastuse andmed.
    public class OpenWeatherRootDto
    {
        public string? Name { get; set; }
        public OpenWeatherMainDto? Main { get; set; }
        public List<OpenWeatherConditionDto> Weather { get; set; } = new();
        public OpenWeatherWindDto? Wind { get; set; }
        public OpenWeatherSysDto? Sys { get; set; }
    }

    public class OpenWeatherMainDto
    {
        public double Temp { get; set; }
        [JsonPropertyName("feels_like")]
        public double FeelsLike { get; set; }
        public int Humidity { get; set; }
        public int Pressure { get; set; }
    }

    public class OpenWeatherConditionDto
    {
        public string? Description { get; set; }
    }

    public class OpenWeatherWindDto
    {
        public double Speed { get; set; }
    }

    public class OpenWeatherSysDto
    {
        public string? Country { get; set; }
    }
}
