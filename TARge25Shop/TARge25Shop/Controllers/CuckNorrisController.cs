using Microsoft.AspNetCore.Mvc;
using TARge25Shop.ApplicationServices.Services;

namespace TARge25Shop.Controllers
{
    public class ChuckNorrisController : Controller
    {
        private readonly ChuckNorrisServices _chuckNorrisServices;

        public ChuckNorrisController(ChuckNorrisServices chuckNorrisServices)
        {
            _chuckNorrisServices = chuckNorrisServices;
        }

        public async Task<IActionResult> Index()
        {
            // Küsime teenuse kaudu API-st juhusliku nalja.
            var result = await _chuckNorrisServices.ChuckNorrisResultHttpClient();

            // Kontrollime, kas API vastusest saadi objekt.
            if (result == null)
            {
                return Content("Nalja laadimine ebaõnnestus.");
            }

            // Anname saadud DTO edasi vaatele.
            return View(result);
        }
    }
}