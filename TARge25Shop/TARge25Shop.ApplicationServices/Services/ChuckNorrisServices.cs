using System.Text.Json;
using TARge25Shop.Core.Dto;
using TARge25Shop.Core.ServiceInterface;

namespace TARge25Shop.ApplicationServices.Services
{
    public class ChuckNorrisServices : IChuckNorrisServices
    {
        private readonly HttpClient _httpClient;

        public ChuckNorrisServices(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ChuckNorrisRootDto?> ChuckNorrisResultHttpClient()
        {
            // Saadame päringu juhusliku nalja saamiseks.
            using var response = await _httpClient.GetAsync(
                "https://api.chucknorris.io/jokes/random");

            // Annab veateate, kui response ei ole edukas.
            response.EnsureSuccessStatusCode();

            // Loeme vastuse JSON-tekstina.
            var json = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                // Ignoreerime JSON-i omaduste nimedes suur- ja väiketähti.
                PropertyNameCaseInsensitive = true
            };

            // Deserialiseerime JSON-i ChuckNorrisRootDto objektiks.
            var result = JsonSerializer.Deserialize<ChuckNorrisRootDto>(
                json, options);

            // Tagastame saadud objekti.
            return result;
        }
    }
}