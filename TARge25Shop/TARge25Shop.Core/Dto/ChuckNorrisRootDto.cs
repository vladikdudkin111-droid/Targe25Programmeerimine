using System.Text.Json.Serialization;

namespace TARge25Shop.Core.Dto
{
    public class ChuckNorrisRootDto
    {
        // Nalja kategooriad; nimekiri võib olla tühi.
        [JsonPropertyName("categories")]
        public List<string> Categories { get; set; } = new();

        // Loomise aeg API algses tekstivormingus.
        [JsonPropertyName("created_at")]
        public string CreatedAt { get; set; } = string.Empty;

        // Chuck Norrise ikooni aadress.
        [JsonPropertyName("icon_url")]
        public string IconUrl { get; set; } = string.Empty;

        // Nalja unikaalne tunnus.
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        // Viimase muutmise aeg API algses tekstivormingus.
        [JsonPropertyName("updated_at")]
        public string UpdatedAt { get; set; } = string.Empty;

        // Selle nalja veebiaadress.
        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;

        // Nalja tekst.
        [JsonPropertyName("value")]
        public string Value { get; set; } = string.Empty;
    }
}
