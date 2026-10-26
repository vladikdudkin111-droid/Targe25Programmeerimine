using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json;
using TARge25Shop.Core.ServiceInterface;
using TARge25Shop.Models.OpenWeather;

//Controller saab kasutajalt linna nime, kutsub Service meetodit ja saadab andmed vaatesse.
namespace TARge25Shop.Controllers
{
    public class OpenWeatherController : Controller
    {
        private readonly IOpenWeatherServices _openWeatherServices;

        public OpenWeatherController(IOpenWeatherServices openWeatherServices)
        {
            _openWeatherServices = openWeatherServices;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? city)
        {
            var vm = new OpenWeatherViewModel { City = city?.Trim() ?? "" };
            // Esimesel avamisel näitame ainult otsinguvormi.
            if (city == null) return View(vm);
            if (string.IsNullOrWhiteSpace(vm.City) || vm.City.Length > 100)
            {
                ViewBag.Error = "Sisesta linna nimi (kuni 100 märki).";
                return View(vm);
            }
            try
            {
                vm.Weather = await _openWeatherServices.OpenWeatherResultHttpClient(vm.City);
                if (vm.Weather?.Main == null || vm.Weather.Weather.Count == 0)
                {
                    vm.Weather = null;
                    ViewBag.Error = "Ilmaandmeid ei leitud. Proovi uuesti.";
                }
            }
            catch (InvalidOperationException)
            {
                ViewBag.Error = "OpenWeather API võti puudub. Lisa see serveri seadistusse.";
            }
            catch (HttpRequestException exception)
            {
                ViewBag.Error = exception.StatusCode switch
                {
                    HttpStatusCode.NotFound => "Linna ei leitud. Kontrolli linna nime.",
                    HttpStatusCode.Unauthorized => "API võti ei kehti või pole veel aktiveeritud.",
                    HttpStatusCode.TooManyRequests => "API päringute limiit on täis. Proovi hiljem uuesti.",
                    _ => "Ilmaandmete laadimine ebaõnnestus. Proovi hiljem uuesti."
                };
            }
            catch (TaskCanceledException)
            {
                ViewBag.Error = "API päring aegus. Proovi uuesti.";
            }
            catch (JsonException)
            {
                ViewBag.Error = "Ilmateenuse vastust ei saanud lugeda. Proovi hiljem uuesti.";
            }
            return View(vm);
        }
    }
}
