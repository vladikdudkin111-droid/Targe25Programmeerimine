using TARge25Shop.Core.Dto;

namespace TARge25Shop.Core.ServiceInterface
{
    public interface IOpenWeatherServices
    {
        Task<OpenWeatherRootDto?> OpenWeatherResultHttpClient(string city);
    }
}
