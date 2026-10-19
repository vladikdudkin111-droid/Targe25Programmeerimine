using TARge25Shop.Core.Dto;

namespace TARge25Shop.Models.Cocktail
{
    // ViewModel sisaldab kõiki andmeid, mida Razor-vaade peab kuvama.
    public class CocktailViewModel
    {
        // Kasutaja sisestatud otsingutekst.
        public string Search { get; set; } = "";
        // Valitud algustäht; algväärtus on A.
        public string Letter { get; set; } = "A";
        // Näitab, kas kuvatakse otsingutulemusi või tähestikukataloogi.
        public bool IsSearchResult { get; set; }
        // Tähed A–Z kataloogi navigeerimiseks.
        public string Letters => "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        // API-st saadud kokteilid, mida lehel kaardikestena kuvatakse.
        public List<CocktailDto> Drinks { get; set; } = new();

        // Koostame koostisosad ja kogused API vastusest.
        public static List<string> GetIngredients(CocktailDto drink)
        {
            var ingredients = new List<string>();
            // Reflection võimaldab järjest lugeda välju 1 kuni 15.
            var type = typeof(CocktailDto);
            for (int i = 1; i <= 15; i++)
            {
                var ingredient = type.GetProperty($"StrIngredient{i}")?.GetValue(drink) as string;
                var measure = type.GetProperty($"StrMeasure{i}")?.GetValue(drink) as string;
                // Tühje koostisosi ei lisata nimekirja.
                if (!string.IsNullOrWhiteSpace(ingredient))
                {
                    ingredients.Add(string.IsNullOrWhiteSpace(measure)
                        ? ingredient
                        : $"{measure.Trim()} {ingredient}");
                }
            }
            return ingredients;
        }
    }
}
