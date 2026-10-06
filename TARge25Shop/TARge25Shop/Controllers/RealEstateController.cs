using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TARge25Shop.Core.Dto;
using TARge25Shop.Core.ServiceInterface;
using TARge25Shop.Core.Validation;
using TARge25Shop.Data;
using TARge25Shop.Models.RealEstate;

namespace TARge25Shop.Controllers
{
    // Kontroller kontrollib vormi ning ühendab vaated CRUD- ja failiteenustega.
    public class RealEstateController : Controller
    {
        private readonly IRealEstateServices _service;
        private readonly IFileServices _fileServices;
        private readonly TARge25ShopContext _context;

        public RealEstateController(IRealEstateServices service, TARge25ShopContext context, IFileServices fileServices)
        {
            _service = service;
            _context = context;
            _fileServices = fileServices;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // Vaade saab valmis loendi, mitte veel täitmata andmebaasipäringu.
            var result = await _context.RealEstates.AsNoTracking().OrderBy(x => x.CreatedAt)
                .Select(x => new RealEstateIndexViewModel
                {
                    Id = x.Id,
                    Area = x.Area,
                    Location = x.Location,
                    RoomNumber = x.RoomNumber,
                    BuildingType = x.BuildingType
                }).ToListAsync();
            return View(result);
        }

        [HttpGet]
        public IActionResult Create() => View("CreateUpdate", new RealEstateCreateUpdateViewModel());

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RealEstateCreateUpdateViewModel vm)
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
            var vm = new RealEstateCreateUpdateViewModel
            {
                Id = entity.Id,
                Area = entity.Area,
                Location = entity.Location,
                RoomNumber = entity.RoomNumber,
                BuildingType = entity.BuildingType,
                CreatedAt = entity.CreatedAt,
                ModifiedAt = entity.ModifiedAt
            };
            vm.Image.AddRange(await FileFromDatabase(id));
            return View("CreateUpdate", vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(RealEstateCreateUpdateViewModel vm)
        {
            if (!vm.Id.HasValue || vm.Id.Value == Guid.Empty) return BadRequest();
            if (await _service.DetailAsync(vm.Id.Value) == null) return NotFound();
            ValidateFiles(vm);
            if (!ModelState.IsValid)
            {
                // Valideerimisvea järel taastame galerii andmebaasist.
                vm.Image = await FileFromDatabase(vm.Id.Value);
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
            var vm = new RealEstateDetailsViewModel
            {
                Id = entity.Id,
                Area = entity.Area,
                Location = entity.Location,
                RoomNumber = entity.RoomNumber,
                BuildingType = entity.BuildingType,
                CreatedAt = entity.CreatedAt,
                ModifiedAt = entity.ModifiedAt
            };
            vm.Image.AddRange(await FileFromDatabase(id));
            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(Guid id)
        {
            // GET näitab ainult kinnitust; kustutamine toimub POST-päringuga.
            var entity = await _service.DetailAsync(id);
            if (entity == null) return NotFound();
            var vm = new RealEstateDeleteViewModel
            {
                Id = entity.Id,
                Area = entity.Area,
                Location = entity.Location,
                RoomNumber = entity.RoomNumber,
                BuildingType = entity.BuildingType,
                CreatedAt = entity.CreatedAt,
                ModifiedAt = entity.ModifiedAt
            };
            vm.Image.AddRange(await FileFromDatabase(id));
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
            var removed = await _fileServices.RemoveImageFromDatabase(new FileToDatabaseDto
            {
                Id = imageId, RealEstateId = id
            });
            if (removed == null) return NotFound();
            // Ühe pildi eemaldamise järel jääme sama objekti muutmise lehele.
            return RedirectToAction(nameof(Update), new { id });
        }

        private static RealEstateDto ToDto(RealEstateCreateUpdateViewModel vm)
        {
            // Serveri kuupäevi ja salvestatud failiteid vormist üle ei kirjutata.
            return new RealEstateDto
            {
                Id = vm.Id,
                Area = vm.Area,
                Location = vm.Location,
                RoomNumber = vm.RoomNumber,
                BuildingType = vm.BuildingType,
                Files = vm.Files ?? new()
            };
        }

        private void ValidateFiles(RealEstateCreateUpdateViewModel vm)
        {
            var error = ImageUploadRules.Validate(vm.Files);
            if (error != null) ModelState.AddModelError(nameof(vm.Files), error);
        }

        // Tagastame kõigi selle kinnisvara piltide loendi, mitte ühe pildi.
        private async Task<List<RealEstateImageViewModel>> FileFromDatabase(Guid id)
        {
            var files = await _context.FileToDatabases.AsNoTracking()
                .Where(x => x.RealEstateId == id).OrderBy(x => x.ImageTitle).ToListAsync();

            // Base64 teisendamine toimub pärast andmete lugemist C# mälus.
            return files.Select(y => new RealEstateImageViewModel
            {
                ImageId = y.Id,
                ImageTitle = y.ImageTitle,
                ImageData = y.ImageData,
                RealEstateId = y.RealEstateId,
                Image = y.ImageData is { Length: > 0 } && ImageUploadRules.ContentType(y.ImageTitle) is string mime
                    ? string.Format("data:{0};base64,{1}", mime, Convert.ToBase64String(y.ImageData))
                    : null
            }).ToList();
        }

        [HttpGet]
        public async Task<IActionResult> DownloadImage(Guid imageId)
        {
            var image = await _context.FileToDatabases.AsNoTracking().FirstOrDefaultAsync(x => x.Id == imageId);
            if (image?.ImageData == null) return NotFound();
            var fileName = Path.GetFileName((image.ImageTitle ?? "image").Replace('\\', '/'));
            return File(image.ImageData, "application/octet-stream", string.IsNullOrEmpty(fileName) ? "image" : fileName);
        }
    }
}
