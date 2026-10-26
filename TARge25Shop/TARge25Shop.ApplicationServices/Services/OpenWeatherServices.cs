using Microsoft.Extensions.Configuration;
using System.Text.Json;
using TARge25Shop.Core.Dto;
using TARge25Shop.Core.ServiceInterface;

//See klass teeb HTTP päringu OpenWeather API-le ja saab ilmaandmed JSON-formaadis.
namespace TARge25Shop.ApplicationServices.Services
{
    public class OpenWeatherServices : IOpenWeatherServices
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public OpenWeatherServices(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<OpenWeatherRootDto?> OpenWeatherResultHttpClient(string city)
        {
            // Võti loetakse serveri seadistusest, mitte lähtekoodist.
            var apiKey = _configuration["OpenWeather:ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new InvalidOperationException("OpenWeather API võti puudub.");

            // metric annab temperatuuri Celsiuse kraadides ja tuule kiiruse m/s.
            using var response = await _httpClient.GetAsync(
                $"https://api.openweathermap.org/data/2.5/weather?q={Uri.EscapeDataString(city)}&appid={Uri.EscapeDataString(apiKey)}&units=metric&lang=et");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            return JsonSerializer.Deserialize<OpenWeatherRootDto>(json, options);
        }
    }
}
