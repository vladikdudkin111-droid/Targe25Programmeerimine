using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TARge25Shop.Core.Dto;
using TARge25Shop.Core.ServiceInterface;
using TARge25Shop.Data;
using TARge25Shop.Models.Spaceship;

namespace TARge25Shop.Controllers
{
    // Controller ühendab kasutaja vaated teenuste ja andmebaasiga.
    public class SpaceshipController : Controller
    {
        // Ühe faili suuruse piirang on 10 MB.
        private const long MaximumFileSize = 10 * 1024 * 1024;

        // Kõigi ühe vormiga saadetud failide piirang on 25 MB.
        private const long MaximumTotalFileSize = 25 * 1024 * 1024;

        // Teenus tegeleb kosmoselaevade loomise, muutmise ja kustutamisega.
        private readonly ISpaceshipServices _spaceshipServices;

        // Failiteenus võimaldab kustutada ühe pildi ilma kosmoselaeva kustutamata.
        private readonly IFileServices _fileServices;

        // DbContexti kasutatakse nimekirja ja piltide lugemiseks.
        private readonly TARge25ShopContext _context;

        // Konstruktor saab sõltuvused ASP.NET Core dependency injection konteinerist.
        public SpaceshipController(
            ISpaceshipServices spaceshipServices,
            TARge25ShopContext context,
            IFileServices fileServices)
        {
            _spaceshipServices = spaceshipServices;
            _context = context;
            _fileServices = fileServices;
        }

        // INDEX - kuvab kõik kosmoselaevad tabelina.
        public async Task<IActionResult> Index()
        {
            // AsNoTracking sobib lugemiseks, sest Index lehel andmeid ei muudeta.
            var result = await _context.Spaceships
                .AsNoTracking()
                .OrderBy(spaceship => spaceship.CreatedAt)
                .Select(spaceship => new SpaceshipIndexViewModel
                {
                    Id = spaceship.Id,
                    Name = spaceship.Name,
                    ShipType = spaceship.ShipType,
                    Crew = spaceship.Crew,
                    EnginePower = spaceship.EnginePower,
                    CreatedAt = spaceship.CreatedAt,
                    UpdatedAt = spaceship.UpdatedAt
                })
                .ToListAsync();

            return View(result);
        }

        // CREATE GET - avab õpetaja näite järgi ühise CreateUpdate vormi.
        [HttpGet]
        public IActionResult Create()
        {
            return View("CreateUpdate", new SpaceshipCreateUpdateViewModel());
        }

        // CREATE POST - kontrollib vormi ning saadab andmed teenusele salvestamiseks.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SpaceshipCreateUpdateViewModel vm)
        {
            // Kontrollime üleslaaditud failide suurusi enne salvestamist.
            ValidateFiles(vm.Files);

            // Vigase vormi korral kuvatakse sama vorm koos valideerimisteadetega.
            if (!ModelState.IsValid)
            {
                return View("CreateUpdate", vm);
            }

            // ViewModel teisendatakse DTO-ks, mida ApplicationServices kiht kasutab.
            var dto = new SpaceshipDto
            {
                Name = vm.Name,
                ShipType = vm.ShipType,
                Crew = vm.Crew,
                EnginePower = vm.EnginePower,
                Files = vm.Files,
                FileToApiDtos = vm.Image.Select(image => new FileToApiDto
                {
                    Id = image.ImageId,
                    ExistingFilePath = image.FilePath,
                    SpaceshipId = image.SpaceshipId
                }).ToArray()
            };

            await _spaceshipServices.Create(dto);
            return RedirectToAction(nameof(Index));
        }

        // UPDATE GET - loeb olemasoleva kirje ja avab sama CreateUpdate vormi.
        [HttpGet]
        public async Task<IActionResult> Update(Guid id)
        {
            var spaceship = await _spaceshipServices.DetailAsync(id);

            // Puuduva ID korral tagastatakse korrektne HTTP 404 vastus.
            if (spaceship == null)
            {
                return NotFound();
            }

            var vm = new SpaceshipCreateUpdateViewModel
            {
                Id = spaceship.Id,
                Name = spaceship.Name,
                ShipType = spaceship.ShipType,
                Crew = spaceship.Crew,
                EnginePower = spaceship.EnginePower,
                CreatedAt = spaceship.CreatedAt,
                UpdatedAt = spaceship.UpdatedAt,
                Image = await LoadImagesAsync(id)
            };

            return View("CreateUpdate", vm);
        }

        // UPDATE POST - muudab olemasolevat kirjet ja võib lisada uusi pilte.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(SpaceshipCreateUpdateViewModel vm)
        {
            // Ilma ID-ta ei ole võimalik teada, millist kirjet muuta.
            if (!vm.Id.HasValue)
            {
                return BadRequest();
            }

            ValidateFiles(vm.Files);

            // Kui valideerimine ebaõnnestub, laadime olemasolevad pildid uuesti.
            if (!ModelState.IsValid)
            {
                vm.Image = await LoadImagesAsync(vm.Id.Value);
                return View("CreateUpdate", vm);
            }

            // Kontrollime enne teenuse väljakutsumist, et kirje on alles olemas.
            if (await _spaceshipServices.DetailAsync(vm.Id.Value) == null)
            {
                return NotFound();
            }

            var dto = new SpaceshipDto
            {
                Id = vm.Id,
                Name = vm.Name,
                ShipType = vm.ShipType,
                Crew = vm.Crew,
                EnginePower = vm.EnginePower,
                Files = vm.Files,
                CreatedAt = vm.CreatedAt,
                UpdatedAt = vm.UpdatedAt
            };

            await _spaceshipServices.Update(dto);
            return RedirectToAction(nameof(Index));
        }

        // REMOVE IMAGE POST - kustutab ühe valitud pildi ja säilitab kosmoselaeva.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveImage(
            Guid imageId, Guid spaceshipId, bool returnToDetails = false)
        {
            // Vigase või puuduva ID korral ei tohi kustutamist alustada.
            if (!ModelState.IsValid || imageId == Guid.Empty || spaceshipId == Guid.Empty)
            {
                return BadRequest(new { message = "Pildi või kosmoselaeva ID on vigane." });
            }

            // Vorm saadab ainult ID-d; tegeliku failitee leiab teenus andmebaasist.
            var removed = await _fileServices.RemoveImageFromApi(new FileToApiDto
            {
                Id = imageId,
                SpaceshipId = spaceshipId
            });

            if (!removed)
            {
                return NotFound(new { message = "Pilti ei leitud selle kosmoselaeva juurest." });
            }

            // JavaScript eemaldab pildikaardi ilma vormi salvestamata või lehte laadimata.
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true });
            }

            // Ilma JavaScriptita suuname tagasi ainult ühele kahest lubatud vaatest.
            return RedirectToAction(
                returnToDetails ? nameof(Details) : nameof(Update),
                new { id = spaceshipId });
        }

        // DELETE GET - kuvab enne kustutamist kinnitamise lehe.
        [HttpGet]
        public async Task<IActionResult> Delete(Guid id)
        {
            var spaceship = await _spaceshipServices.DetailAsync(id);

            if (spaceship == null)
            {
                return NotFound();
            }

            // Domain objekt teisendatakse kustutamise ViewModeliks.
            var vm = new SpaceshipDeleteViewModel
            {
                Id = spaceship.Id,
                Name = spaceship.Name,
                ShipType = spaceship.ShipType,
                Crew = spaceship.Crew,
                EnginePower = spaceship.EnginePower,
                CreatedAt = spaceship.CreatedAt,
                UpdatedAt = spaceship.UpdatedAt,
                Image = await LoadImagesAsync(id)
            };

            return View(vm);
        }

        // DELETE POST - eemaldab kirje alles pärast kasutaja kinnitust.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmation(Guid id)
        {
            var spaceship = await _spaceshipServices.Delete(id);

            if (spaceship == null)
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Index));
        }

        // DETAILS - kuvab ühe kosmoselaeva andmed ja sellega seotud pildid.
        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            var spaceship = await _spaceshipServices.DetailAsync(id);

            if (spaceship == null)
            {
                return NotFound();
            }

            var vm = new SpaceshipDetailsViewModel
            {
                Id = spaceship.Id,
                Name = spaceship.Name,
                ShipType = spaceship.ShipType,
                Crew = spaceship.Crew,
                EnginePower = spaceship.EnginePower,
                CreatedAt = spaceship.CreatedAt,
                UpdatedAt = spaceship.UpdatedAt,
                Image = await LoadImagesAsync(id)
            };

            return View(vm);
        }

        // Abimeetod loeb ühe kosmoselaeva pildid ja teisendab need ViewModeliteks.
        private async Task<List<ImageViewModel>> LoadImagesAsync(Guid spaceshipId)
        {
            return await _context.FileToApis
                .AsNoTracking()
                .Where(file => file.SpaceshipId == spaceshipId)
                .Select(file => new ImageViewModel
                {
                    ImageId = file.Id,
                    FilePath = file.ExistingFilePath,
                    SpaceshipId = file.SpaceshipId
                })
                .ToListAsync();
        }

        // Abimeetod lisab ModelState'i vead, kui failid ületavad lubatud suuruse.
        private void ValidateFiles(IEnumerable<IFormFile> files)
        {
            var uploadedFiles = files.Where(file => file.Length > 0).ToList();

            if (uploadedFiles.Any(file => file.Length > MaximumFileSize))
            {
                ModelState.AddModelError(
                    nameof(SpaceshipCreateUpdateViewModel.Files),
                    "Ühe faili suurus võib olla maksimaalselt 10 MB.");
            }

            if (uploadedFiles.Sum(file => file.Length) > MaximumTotalFileSize)
            {
                ModelState.AddModelError(
                    nameof(SpaceshipCreateUpdateViewModel.Files),
                    "Failide kogusuurus võib olla maksimaalselt 25 MB.");
            }
        }
    }
}
