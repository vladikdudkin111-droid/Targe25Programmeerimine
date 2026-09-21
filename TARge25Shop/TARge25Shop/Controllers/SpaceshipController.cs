using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TARge25Shop.Core.Domain;
using TARge25Shop.Core.Dto;
using TARge25Shop.Core.ServiceInterface;
using TARge25Shop.Data;
using TARge25Shop.Models.Spaceship;

namespace TARge25Shop.Controllers
{
    // Controller tegeleb kosmoselaevade lehtede ja kasutaja päringutega.
    public class SpaceshipController : Controller
    {
        // Teenus sisaldab kosmoselaevade loomise, muutmise, vaatamise ja kustutamise loogikat.
        private readonly ISpaceshipServices _spaceshipServices;

        // Failiteenus annab ligipääsu kosmoselaevaga seotud failidele.
        private readonly IFileServices _fileServices;

        // Andmebaasi konteksti kasutame kosmoselaevade nimekirja lugemiseks.
        private readonly TARge25ShopContext _context;

        // Konstruktor saab vajalikud sõltuvused dependency injection konteinerist.
        public SpaceshipController(
            ISpaceshipServices spaceshipServices,
            IFileServices fileServices,
            TARge25ShopContext context)
        {
            _spaceshipServices = spaceshipServices;
            _fileServices = fileServices;
            _context = context;
        }

        // INDEX - kuvab kõik andmebaasis olevad kosmoselaevad.
        public async Task<IActionResult> Index()
        {
            // Loeme kosmoselaevad andmebaasist ja järjestame need loomise aja järgi.
            var spaceshipEntities = await _context.Spaceships
                .OrderBy(x => x.CreatedAt)
                .ToListAsync();

            // Teisendame Domain objektid Index vaatele sobivateks ViewModeliteks.
            var spaceships = spaceshipEntities
                .Select(x => new SpaceshipIndexViewModel
                {
                    Id = x.Id,
                    Name = x.Name,
                    ShipType = x.ShipType,
                    Crew = x.Crew,
                    EnginePower = x.EnginePower,
                    FileCount = _fileServices.FilesFromApi(x.Id).Count,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt
                })
                .ToList();

            // Anname kosmoselaevade nimekirja Index vaatele.
            return View(spaceships);
        }

        // CREATEUPDATE GET - avab ühise loomise või muutmise vormi.
        [HttpGet]
        public async Task<IActionResult> CreateUpdate(Guid? id)
        {
            // Puuduv Id tähendab, et kasutaja soovib luua uue kosmoselaeva.
            if (!id.HasValue || id.Value == Guid.Empty)
            {
                return View(new SpaceshipCreateUpdateViewModel());
            }

            // Olemasoleva Id korral otsime muudetava kosmoselaeva andmebaasist.
            var spaceship = await _spaceshipServices.DetailAsync(id.Value);
            if (spaceship == null)
            {
                return NotFound();
            }

            // Täidame ühise ViewModeli olemasolevate andmete ja failidega.
            var vm = new SpaceshipCreateUpdateViewModel
            {
                Id = spaceship.Id,
                Name = spaceship.Name,
                ShipType = spaceship.ShipType,
                Crew = spaceship.Crew,
                EnginePower = spaceship.EnginePower,
                ExistingImages = ToImageViewModels(spaceship.Id)
            };

            // Avame CreateUpdate vaate muutmise režiimis.
            return View(vm);
        }

        // CREATEUPDATE POST - loob uue või uuendab olemasoleva kosmoselaeva.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUpdate(SpaceshipCreateUpdateViewModel vm)
        {
            // Kontrollime uute failide suurust enne salvestamist.
            ValidateFiles(vm.Files);

            Spaceship? existing = null;

            // Kui Id on olemas, kontrollime, et muudetav kirje on endiselt andmebaasis.
            if (vm.Id.HasValue && vm.Id.Value != Guid.Empty)
            {
                existing = await _spaceshipServices.DetailAsync(vm.Id.Value);
                if (existing == null)
                {
                    return NotFound();
                }
            }

            // Vigase vormi korral taastame olemasolevate failide nimekirja ja näitame vormi uuesti.
            if (!ModelState.IsValid)
            {
                if (existing != null)
                {
                    vm.ExistingImages = ToImageViewModels(existing.Id);
                }

                return View(vm);
            }

            // Teisendame vormi andmed teenusekihile sobivaks DTO-ks.
            var dto = new SpaceshipDto
            {
                Id = existing?.Id ?? Guid.Empty,
                Name = vm.Name,
                ShipType = vm.ShipType,
                Crew = vm.Crew,
                EnginePower = vm.EnginePower,
                Files = vm.Files,
                FileNamesToDelete = vm.FileNamesToDelete
            };

            if (existing == null)
            {
                // Ilma olemasoleva kirjega kutsume loomise teenusemeetodi.
                await _spaceshipServices.Create(dto);
            }
            else
            {
                // Olemasoleva kirjega kutsume muutmise teenusemeetodi.
                await _spaceshipServices.Update(dto);
            }

            // Pärast salvestamist läheme tagasi kosmoselaevade nimekirja.
            return RedirectToAction(nameof(Index));
        }

        // DETAILS GET - kuvab ühe kosmoselaeva detailse info.
        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            // Otsime kosmoselaeva ID järgi.
            var spaceship = await _spaceshipServices.DetailAsync(id);
            if (spaceship == null)
            {
                return NotFound();
            }

            // Kasutame Details lehel sellele mõeldud eraldi ViewModelit.
            var vm = new SpaceshipDetailsViewModel
            {
                Id = spaceship.Id,
                Name = spaceship.Name,
                ShipType = spaceship.ShipType,
                Crew = spaceship.Crew,
                EnginePower = spaceship.EnginePower,
                Images = ToImageViewModels(spaceship.Id),
                CreatedAt = spaceship.CreatedAt,
                UpdatedAt = spaceship.UpdatedAt
            };

            return View(vm);
        }

        // DELETE GET - avab kustutamise kinnitamise lehe.
        [HttpGet]
        public async Task<IActionResult> Delete(Guid id)
        {
            // Otsime kustutatava kosmoselaeva ID järgi.
            var spaceship = await _spaceshipServices.DetailAsync(id);
            if (spaceship == null)
            {
                return NotFound();
            }

            // Kasutame kustutamise lehel ainult seal vajalikke andmeid sisaldavat ViewModelit.
            var vm = new SpaceshipDeleteViewModel
            {
                Id = spaceship.Id,
                Name = spaceship.Name,
                ShipType = spaceship.ShipType,
                Crew = spaceship.Crew,
                EnginePower = spaceship.EnginePower,
                ImageCount = _fileServices.FilesFromApi(spaceship.Id).Count
            };

            return View(vm);
        }

        // DELETE POST - kustutab kosmoselaeva pärast kasutaja kinnitust.
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            // Teenus kustutab kosmoselaeva, selle failid ja FileToApi kirjed.
            var deleted = await _spaceshipServices.Delete(id);
            if (deleted == null)
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Index));
        }

        // DOWNLOAD GET - tagastab valitud faili kasutajale allalaadimiseks.
        [HttpGet]
        public IActionResult DownloadFile(Guid spaceshipId, string storedFileName)
        {
            // Failiteenus kontrollib nii andmebaasikirjet kui ka füüsilise faili olemasolu.
            var file = _fileServices.FileFromApi(spaceshipId, storedFileName);
            if (file == null)
            {
                return NotFound();
            }

            // PhysicalFile saadab serveri kettal oleva faili brauserile.
            return PhysicalFile(file.FilePath, "application/octet-stream", file.FileName);
        }

        // Abimeetod teisendab faili DTO-d õpetaja struktuurile vastavateks ImageViewModeliteks.
        private List<ImageViewModel> ToImageViewModels(Guid spaceshipId)
        {
            return _fileServices.FilesFromApi(spaceshipId)
                .Select(file => new ImageViewModel
                {
                    SpaceshipId = spaceshipId,
                    // FilePath peab sisaldama kettale salvestatud unikaalset failinime.
                    FilePath = file.StoredFileName,
                    FileName = file.FileName,
                    StoredFileName = file.StoredFileName,
                    RelativePath = file.RelativePath,
                    FileSize = file.FileSize,
                    CreatedAt = file.CreatedAt
                })
                .ToList();
        }

        // Abimeetod kontrollib ühe faili ja kõikide failide maksimaalset suurust.
        private void ValidateFiles(IEnumerable<IFormFile> files)
        {
            // Ühe faili maksimaalne lubatud suurus on 10 MB.
            const long maximumFileSize = 10 * 1024 * 1024;

            // Kõikide failide maksimaalne kogusuurus on 25 MB.
            const long maximumTotalSize = 25 * 1024 * 1024;

            long totalSize = 0;

            // Tühjad failid jätame vahele, sest neid ei ole vaja salvestada.
            foreach (var file in files.Where(file => file.Length > 0))
            {
                if (file.Length > maximumFileSize)
                {
                    // Liiga suure faili korral lisame vormile valideerimisvea.
                    ModelState.AddModelError(nameof(SpaceshipCreateUpdateViewModel.Files),
                        $"File '{file.FileName}' is larger than 10 MB.");
                    continue;
                }

                totalSize += file.Length;
                if (totalSize > maximumTotalSize)
                {
                    // Liiga suure kogusuuruse korral lõpetame edasise kontrollimise.
                    ModelState.AddModelError(nameof(SpaceshipCreateUpdateViewModel.Files),
                        "The total size of all files cannot exceed 25 MB.");
                    break;
                }
            }
        }
    }
}
