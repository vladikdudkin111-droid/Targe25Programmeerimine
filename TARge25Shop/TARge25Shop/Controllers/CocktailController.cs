using Microsoft.AspNetCore.Mvc;
using TARge25Shop.Core.ServiceInterface;
using TARge25Shop.Models.Cocktail;

namespace TARge25Shop.Controllers
{
    // Võtab vastu veebilehe päringud ja kutsub kokteilide teenust.
    public class CocktailController : Controller
    {
        private readonly ICocktailServices _cocktailServices;

        // Liidese kaudu saadakse teenus Dependency Injection abil.
        public CocktailController(ICocktailServices cocktailServices)
        {
            _cocktailServices = cocktailServices;
        }

        // GET /Cocktail/Index?search=mojito või ?letter=M
        [HttpGet]
        public async Task<IActionResult> Index(string? search, string letter = "A")
        {
            // Kui otsing on täidetud, otsime nime järgi. Muidu kuvame valitud tähte.
            var isSearch = !string.IsNullOrWhiteSpace(search);
            var selectedLetter = !string.IsNullOrWhiteSpace(letter) &&
                letter.Length == 1 && char.IsLetter(letter[0])
                ? letter.ToUpperInvariant()
                : "A";

            // Valmistame ette andmed, mis Razor-vaatesse saadetakse.
            var vm = new CocktailViewModel
            {
                Search = search?.Trim() ?? "",
                Letter = selectedLetter,
                IsSearchResult = isSearch
            };

            try
            {
                // Kasutame vastavalt otsingule kas nime- või tähepäringut.
                var result = isSearch
                    ? await _cocktailServices.CocktailResultHttpClient(vm.Search)
                    : await _cocktailServices.CocktailByLetterHttpClient(selectedLetter.ToLowerInvariant());

                vm.Drinks = result?.Drinks ?? new();
            }
            catch (HttpRequestException)
            {
                ViewBag.Error = "Kokteilide laadimine ebaõnnestus. Proovi hiljem uuesti.";
            }
            catch (TaskCanceledException)
            {
                ViewBag.Error = "API päring aegus. Proovi uuesti.";
            }

            // Edastame ViewModeli Views/Cocktail/Index.cshtml lehele.
            return View(vm);
        }
    }
}
