using System.Text.Json;
using TARge25Shop.Core.Dto;
using TARge25Shop.Core.ServiceInterface;

namespace TARge25Shop.ApplicationServices.Services
{
    // Rakendab liidest ja suhtleb TheCocktailDB välise API-ga.
    public class CocktailServices : ICocktailServices
    {
        private readonly HttpClient _httpClient;

        // HttpClient antakse konstruktorisse sõltuvussüstiga (Dependency Injection).
        public CocktailServices(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public Task<CocktailRootDto?> CocktailResultHttpClient(string search)
        {
            // Otsime kokteile nime järgi.
            return GetCocktailsAsync("s", search);
        }

        public Task<CocktailRootDto?> CocktailByLetterHttpClient(string letter)
        {
            // Küsime API-st kõik valitud tähega algavad kokteilid.
            return GetCocktailsAsync("f", letter);
        }

        // Ühine abimeetod väldib kahe API päringu koodi kordamist.
        private async Task<CocktailRootDto?> GetCocktailsAsync(string parameter, string value)
        {
            // Koostame API aadressi ja saadame HTTP GET päringu.
            using var response = await _httpClient.GetAsync(
                $"https://www.thecocktaildb.com/api/json/v1/1/search.php?{parameter}={Uri.EscapeDataString(value)}");

            // Kontrollime, kas päring õnnestus.
            response.EnsureSuccessStatusCode();

            // Loeme JSON-vastuse tekstina.
            var json = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                // JSON-i väljade nimede suur- ja väiketähti ei eristata.
                PropertyNameCaseInsensitive = true
            };

            // Muudame JSON-i CocktailRootDto objektiks.
            return JsonSerializer.Deserialize<CocktailRootDto>(json, options);
        }
    }
}
