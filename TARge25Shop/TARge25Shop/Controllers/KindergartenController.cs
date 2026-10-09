using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TARge25Shop.Core.Dto;
using TARge25Shop.Core.ServiceInterface;
using TARge25Shop.Data;
using TARge25Shop.Models.Kindergarten;

namespace TARge25Shop.Controllers
{
    // Controller võtab kasutaja päringud vastu ja valmistab vaadetele andmed ette.
    public class KindergartenController : Controller
    {
        private readonly IKindergartenServices _kindergartenServices;
        private readonly TARge25ShopContext _context;
        private readonly IFileService _fileService;

        public KindergartenController(
            IKindergartenServices kindergartenServices,
            TARge25ShopContext context,
            IFileService fileService)
        {
            _kindergartenServices = kindergartenServices;
            _context = context;
            _fileService = fileService;
        }

        // INDEX - kuvab lasteaiarühmade nimekirja.
        public async Task<IActionResult> Index()
        {
            var kindergartens = await _context.Kindergartens
                .AsNoTracking()
                .OrderBy(x => x.CreatedAt)
                .Select(x => new KindergartenIndexViewModel
                {
                    Id = x.Id,
                    GroupName = x.GroupName,
                    ChildrenCount = x.ChildrenCount,
                    KindergartenName = x.KindergartenName,
                    TeacherName = x.TeacherName,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt
                })
                .ToListAsync();

            return View(kindergartens);
        }

        // CREATE GET - avab tühja ühise vormi.
        [HttpGet]
        public IActionResult Create()
        {
            return View("CreateUpdate", new KindergartenCreateUpdateViewModel());
        }

        // CREATE POST - kontrollib andmeid ja loob uue rühma.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(FileUploadDto.MaxRequestSize)]
        [RequestFormLimits(MultipartBodyLengthLimit = FileUploadDto.MaxRequestSize)]
        public async Task<IActionResult> Create(KindergartenCreateUpdateViewModel vm)
        {
            // Loomisel ei kasutata brauserist saadetud ID-d.
            vm.Id = Guid.Empty;
            ModelState.Remove(nameof(vm.Id));
            if (!ModelState.IsValid)
            {
                return View("CreateUpdate", vm);
            }

            try
            {
                var dto = ToDto(vm);
                dto.Files = await ReadFilesAsync(vm.Files);
                await _kindergartenServices.Create(dto);
            }
            catch (ValidationException exception)
            {
                ModelState.AddModelError(nameof(vm.Files), exception.Message);
                return View("CreateUpdate", vm);
            }
            return RedirectToAction(nameof(Index));
        }

        // DETAILS GET - kuvab rühma kõik andmed.
        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            var kindergarten = await _kindergartenServices.DetailAsync(id);
            if (kindergarten == null)
            {
                return NotFound();
            }

            // See on vaheinstants Domain objekti ja Details ViewModeli vahel.
            var vm = new KindergartenDetailsViewModel
            {
                Id = kindergarten.Id,
                GroupName = kindergarten.GroupName,
                ChildrenCount = kindergarten.ChildrenCount,
                KindergartenName = kindergarten.KindergartenName,
                TeacherName = kindergarten.TeacherName,
                CreatedAt = kindergarten.CreatedAt,
                UpdatedAt = kindergarten.UpdatedAt,
                Gallery = await LoadGalleryAsync(id)
            };
            return View(vm);
        }

        // UPDATE GET - täidab ühise vormi olemasolevate andmetega.
        [HttpGet]
        public async Task<IActionResult> Update(Guid id)
        {
            var kindergarten = await _kindergartenServices.DetailAsync(id);
            if (kindergarten == null)
            {
                return NotFound();
            }

            var vm = new KindergartenCreateUpdateViewModel
            {
                Id = kindergarten.Id,
                GroupName = kindergarten.GroupName,
                ChildrenCount = kindergarten.ChildrenCount,
                KindergartenName = kindergarten.KindergartenName,
                TeacherName = kindergarten.TeacherName,
                Gallery = await LoadGalleryAsync(id, allowDelete: true)
            };
            return View("CreateUpdate", vm);
        }

        // UPDATE POST - salvestab kehtivad muudatused.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(FileUploadDto.MaxRequestSize)]
        [RequestFormLimits(MultipartBodyLengthLimit = FileUploadDto.MaxRequestSize)]
        public async Task<IActionResult> Update([FromRoute] Guid id, KindergartenCreateUpdateViewModel vm)
        {
            // Aadressi ja vormi ID peavad viitama samale kirjele.
            if (id == Guid.Empty || id != vm.Id)
            {
                return BadRequest();
            }

            if (await _kindergartenServices.DetailAsync(id) == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                vm.Gallery = await LoadGalleryAsync(id, allowDelete: true);
                return View("CreateUpdate", vm);
            }

            try
            {
                var dto = ToDto(vm);
                dto.Files = await ReadFilesAsync(vm.Files);
                var updated = await _kindergartenServices.Update(dto);
                if (updated == null)
                {
                    return NotFound();
                }
            }
            catch (ValidationException exception)
            {
                ModelState.AddModelError(nameof(vm.Files), exception.Message);
                vm.Gallery = await LoadGalleryAsync(id, allowDelete: true);
                return View("CreateUpdate", vm);
            }

            return RedirectToAction(nameof(Index));
        }

        // DELETE GET - kuvab kinnituse, kuid ei kustuta veel andmeid.
        [HttpGet]
        public async Task<IActionResult> Delete(Guid id)
        {
            var kindergarten = await _kindergartenServices.DetailAsync(id);
            if (kindergarten == null)
            {
                return NotFound();
            }

            // See on vaheinstants Domain objekti ja Delete ViewModeli vahel.
            var vm = new KindergartenDeleteViewModel
            {
                Id = kindergarten.Id,
                GroupName = kindergarten.GroupName,
                ChildrenCount = kindergarten.ChildrenCount,
                KindergartenName = kindergarten.KindergartenName,
                TeacherName = kindergarten.TeacherName,
                CreatedAt = kindergarten.CreatedAt,
                UpdatedAt = kindergarten.UpdatedAt,
                Gallery = await LoadGalleryAsync(id)
            };
            return View(vm);
        }

        // DELETE POST - kustutab rühma pärast kasutaja kinnitust.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmation(Guid id)
        {
            var deleted = await _kindergartenServices.Delete(id);
            if (deleted == null)
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Index));
        }

        // Pilt loetakse andmebaasist ainult selle kuvamise päringu ajal.
        [HttpGet]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> Image(Guid id, Guid imageId)
        {
            var image = await _fileService.GetImageAsync(id, imageId);
            if (image == null)
            {
                return NotFound();
            }

            Response.Headers["X-Content-Type-Options"] = "nosniff";
            return File(image.Data, image.ContentType);
        }

        // Ühe pildi eemaldamine ei kustuta ankeeti ega teisi pilte.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteImage([FromRoute] Guid id, Guid imageId)
        {
            if (!await _fileService.DeleteImageAsync(id, imageId))
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Update), new { id });
        }

        private async Task<KindergartenImagesViewModel> LoadGalleryAsync(Guid id, bool allowDelete = false)
        {
            var images = await _fileService.GetImagesAsync(id);
            return new KindergartenImagesViewModel
            {
                KindergartenId = id,
                AllowDelete = allowDelete,
                Images = images.Select(x => new KindergartenImageViewModel
                {
                    Id = x.Id,
                    FileName = x.FileName
                }).ToList()
            };
        }

        private async Task<List<FileUploadDto>> ReadFilesAsync(List<IFormFile> files)
        {
            if (files.Count > FileUploadDto.MaxFileCount)
                throw new ValidationException("Korraga saab lisada kuni 10 pilti.");
            if (files.Sum(x => x.Length) > FileUploadDto.MaxTotalSize)
                throw new ValidationException("Piltide kogumaht võib olla kuni 20 MB.");

            var uploads = new List<FileUploadDto>();
            foreach (var file in files)
            {
                if (file.Length == 0 || file.Length > FileUploadDto.MaxFileSize)
                    throw new ValidationException("Pilt ei tohi olla tühi ega suurem kui 5 MB.");

                using var stream = new MemoryStream();
                await file.CopyToAsync(stream, HttpContext.RequestAborted);
                uploads.Add(new FileUploadDto { FileName = file.FileName, Data = stream.ToArray() });
            }
            return uploads;
        }

        // Tuleb teha vaheinstants ViewModeli ja DTO vahel.
        private static KindergartenDto ToDto(KindergartenCreateUpdateViewModel vm)
        {
            return new KindergartenDto
            {
                Id = vm.Id,
                GroupName = vm.GroupName,
                ChildrenCount = vm.ChildrenCount,
                KindergartenName = vm.KindergartenName,
                TeacherName = vm.TeacherName
            };
        }
    }
}
