using TARge25Shop.Core.Dto;

//ViewModel hoiab linna nime ja saadud ilmaandmeid.
namespace TARge25Shop.Models.OpenWeather
{
    public class OpenWeatherViewModel
    {
        public string City { get; set; } = "";
        public OpenWeatherRootDto? Weather { get; set; }
    }
}
