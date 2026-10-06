using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TARge25Shop.Core.Dto;
using TARge25Shop.Core.ServiceInterface;
using TARge25Shop.Core.Validation;
using TARge25Shop.Data;
using TARge25Shop.Models.Spaceship;

namespace TARge25Shop.Controllers
{
    // Kontroller kontrollib vormi ning ühendab vaated CRUD- ja failiteenustega.
    public class SpaceshipController : Controller
    {
        private readonly ISpaceshipServices _service;
        private readonly IFileServices _fileServices;
        private readonly TARge25ShopContext _context;

        public SpaceshipController(ISpaceshipServices service, TARge25ShopContext context, IFileServices fileServices)
        {
            _service = service;
            _context = context;
            _fileServices = fileServices;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // Vaade saab valmis loendi, mitte veel täitmata andmebaasipäringu.
            var result = await _context.Spaceships.AsNoTracking().OrderBy(x => x.CreatedAt)
                .Select(x => new SpaceshipIndexViewModel
                {
                    Id = x.Id,
                    Name = x.Name,
                    ShipType = x.ShipType,
                    CreatedAt = x.CreatedAt,
                    Crew = x.Crew
                }).ToListAsync();
            return View(result);
        }

        [HttpGet]
        public IActionResult Create() => View("CreateUpdate", new SpaceshipCreateUpdateViewModel());

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SpaceshipCreateUpdateViewModel vm)
        {
            // Loomisel määrab ID teenus, mitte kasutaja peidetud vormiväli.
            vm.Id = null;
            ModelState.Remove(nameof(vm.Id));
            ValidateFiles(vm);
            if (!ModelState.IsValid) return View("CreateUpdate", vm);
            await _service.Create(ToDto(vm));
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Update(Guid id)
        {
            var entity = await _service.DetailAsync(id);
            if (entity == null) return NotFound();
            var vm = new SpaceshipCreateUpdateViewModel
            {
                Id = entity.Id,
                Name = entity.Name,
                ShipType = entity.ShipType,
                Crew = entity.Crew,
                EnginePower = entity.EnginePower,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt
            };
            vm.Image.AddRange(await FilesFromApi(id));
            return View("CreateUpdate", vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(SpaceshipCreateUpdateViewModel vm)
        {
            if (!vm.Id.HasValue || vm.Id.Value == Guid.Empty) return BadRequest();
            if (await _service.DetailAsync(vm.Id.Value) == null) return NotFound();
            ValidateFiles(vm);
            if (!ModelState.IsValid)
            {
                // Valideerimisvea järel taastame galerii andmebaasist.
                vm.Image = await FilesFromApi(vm.Id.Value);
                return View("CreateUpdate", vm);
            }
            var result = await _service.Update(ToDto(vm));
            if (result == null) return NotFound();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            var entity = await _service.DetailAsync(id);
            if (entity == null) return NotFound();
            var vm = new SpaceshipDetailsViewModel
            {
                Id = entity.Id,
                Name = entity.Name,
                ShipType = entity.ShipType,
                Crew = entity.Crew,
                EnginePower = entity.EnginePower,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt
            };
            vm.Image.AddRange(await FilesFromApi(id));
            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(Guid id)
        {
            // GET näitab ainult kinnitust; kustutamine toimub POST-päringuga.
            var entity = await _service.DetailAsync(id);
            if (entity == null) return NotFound();
            var vm = new SpaceshipDeleteViewModel
            {
                Id = entity.Id,
                Name = entity.Name,
                ShipType = entity.ShipType,
                Crew = entity.Crew,
                EnginePower = entity.EnginePower,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt
            };
            vm.Image.AddRange(await FilesFromApi(id));
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmation(Guid id)
        {
            if (!ModelState.IsValid || id == Guid.Empty) return BadRequest();
            var result = await _service.Delete(id);
            if (result == null) return NotFound();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveImage(Guid id, Guid imageId)
        {
            if (!ModelState.IsValid || id == Guid.Empty || imageId == Guid.Empty) return BadRequest();
            if (await _service.DetailAsync(id) == null) return NotFound();
            var removed = await _fileServices.RemoveImageFromApi(new FileToApiDto
            {
                Id = imageId, SpaceshipId = id
            });
            if (removed == null) return NotFound();
            // Ühe pildi eemaldamise järel jääme sama objekti muutmise lehele.
            return RedirectToAction(nameof(Update), new { id });
        }

        private static SpaceshipDto ToDto(SpaceshipCreateUpdateViewModel vm)
        {
            // Serveri kuupäevi ja salvestatud failiteid vormist üle ei kirjutata.
            return new SpaceshipDto
            {
                Id = vm.Id,
                Name = vm.Name,
                ShipType = vm.ShipType,
                Crew = vm.Crew,
                EnginePower = vm.EnginePower,
                Files = vm.Files ?? new()
            };
        }

        private void ValidateFiles(SpaceshipCreateUpdateViewModel vm)
        {
            var error = ImageUploadRules.Validate(vm.Files);
            if (error != null) ModelState.AddModelError(nameof(vm.Files), error);
        }

        // Andmebaasist loeme piltide nimed; failid serveeritakse wwwroot kaustast.
        private async Task<List<ImageViewModel>> FilesFromApi(Guid id)
        {
            return await _context.FileToApis.AsNoTracking().Where(x => x.SpaceshipId == id)
                .Select(x => new ImageViewModel
                {
                    ImageId = x.Id, FilePath = x.ExistingFilePath, SpaceshipId = x.SpaceshipId
                }).ToListAsync();
        }
    }
}
