using Microsoft.AspNetCore.Mvc;
using TARge25Shop.Core.ServiceInterface;
using TARge25Shop.Models.ChuckNorris;

namespace TARge25Shop.Controllers
{
    public class ChuckNorrisController : Controller
    {
        private readonly IChuckNorrisServices _chuckNorrisServices;

        public ChuckNorrisController
            (
                IChuckNorrisServices chuckNorrisServices
            )
        {
            _chuckNorrisServices = chuckNorrisServices;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Joke()
        {
            var joke = await _chuckNorrisServices.ChuckNorrisResultHttpClient();

            if (joke == null)
            {
                return Content("Nalja laadimine ebaõnnestus.");
            }

            ChuckNorrisViewModel vm = new();

            //vm.Categories = joke.Categories;
            vm.CreatedAt = joke.CreatedAt;
            vm.IconUrl = joke.IconUrl;
            vm.Id = joke.Id;
            vm.UpdatedAt = joke.UpdatedAt;
            vm.Url = joke.Url;
            vm.Value = joke.Value;

            return View("Joke", vm);
        }
    }
}
