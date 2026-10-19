using System.Collections.Generic;

namespace TARge25Shop.Core.Dto
{
    // API JSON-vastuse põhiobjekt, mis sisaldab leitud jookide loendit.
    public class CocktailRootDto
    {
        // JSON-i "drinks" massiiv; vastus võib olla null, kui midagi ei leitud.
        public List<CocktailDto>? Drinks { get; set; }
    }

    // Ühe kokteili andmed: nimi, foto, kategooria, juhend ja koostisosad.
    public class CocktailDto
    {
        public string? IdDrink { get; set; }
        public string? StrDrink { get; set; }
        public string? StrDrinkThumb { get; set; }
        public string? StrCategory { get; set; }
        public string? StrAlcoholic { get; set; }
        public string? StrGlass { get; set; }
        public string? StrInstructions { get; set; }
        // API saadab koostisosad eraldi väljadel StrIngredient1 ... StrIngredient15.
        // Vastavad kogused asuvad väljadel StrMeasure1 ... StrMeasure15.
        public string? StrIngredient1 { get; set; }
        public string? StrMeasure1 { get; set; }
        public string? StrIngredient2 { get; set; }
        public string? StrMeasure2 { get; set; }
        public string? StrIngredient3 { get; set; }
        public string? StrMeasure3 { get; set; }
        public string? StrIngredient4 { get; set; }
        public string? StrMeasure4 { get; set; }
        public string? StrIngredient5 { get; set; }
        public string? StrMeasure5 { get; set; }
        public string? StrIngredient6 { get; set; }
        public string? StrMeasure6 { get; set; }
        public string? StrIngredient7 { get; set; }
        public string? StrMeasure7 { get; set; }
        public string? StrIngredient8 { get; set; }
        public string? StrMeasure8 { get; set; }
        public string? StrIngredient9 { get; set; }
        public string? StrMeasure9 { get; set; }
        public string? StrIngredient10 { get; set; }
        public string? StrMeasure10 { get; set; }
        public string? StrIngredient11 { get; set; }
        public string? StrMeasure11 { get; set; }
        public string? StrIngredient12 { get; set; }
        public string? StrMeasure12 { get; set; }
        public string? StrIngredient13 { get; set; }
        public string? StrMeasure13 { get; set; }
        public string? StrIngredient14 { get; set; }
        public string? StrMeasure14 { get; set; }
        public string? StrIngredient15 { get; set; }
        public string? StrMeasure15 { get; set; }
    }
}
