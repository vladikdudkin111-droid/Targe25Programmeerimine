using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TARge25Shop.Core.Dto;
using TARge25Shop.Core.ServiceInterface;
using TARge25Shop.Data;
using TARge25Shop.Models.RealEstate;

namespace TARge25Shop.Controllers
{
    // Controller seob kinnisvara vaated teenustega, nagu SpaceshipController.
    public class RealEstateController : Controller
    {
        private readonly IRealEstateServices _realEstateServices;
        private readonly TARge25ShopContext _context;

        // Dependency injection annab CRUD teenuse ja andmebaasikonteksti.
        public RealEstateController(IRealEstateServices realEstateServices,
            TARge25ShopContext context)
        {
            _realEstateServices = realEstateServices;
            _context = context;
        }

        // INDEX - loeb kõik kinnisvaraobjektid ning teisendab need tabeli ridadeks.
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // Lugemisvaade ei vaja Entity Frameworki muudatuste jälgimist.
            var result = await _context.RealEstates.AsNoTracking()
                .OrderBy(item => item.CreatedAt)
                .Select(item => new RealEstateIndexViewModel
                {
                    Id = item.Id,
                    Address = item.Address,
                    Area = item.Area,
                    RoomCount = item.RoomCount,
                    Price = item.Price,
                    CreatedAt = item.CreatedAt,
                    UpdatedAt = item.UpdatedAt
                })
                .ToListAsync();
            return View(result);
        }

        // CREATE GET - avab tühja ühise loomise ja muutmise vormi.
        [HttpGet]
        public IActionResult Create()
        {
            return View("CreateUpdate", new RealEstateCreateUpdateViewModel());
        }

        // CREATE POST - kontrollib vormi ning loob uue objekti.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RealEstateCreateUpdateViewModel vm)
        {
            // Loomise ID määrab teenus; vormist saadetud Id ei vali muutmise režiimi.
            vm.Id = null;
            ModelState.Remove(nameof(vm.Id));
            ValidateForm(vm);
            if (!ModelState.IsValid)
            {
                return View("CreateUpdate", vm);
            }

            await _realEstateServices.Create(ToDto(vm));
            return RedirectToAction(nameof(Index));
        }

        // UPDATE GET - loeb olemasoleva objekti andmed muutmise vormi.
        [HttpGet]
        public async Task<IActionResult> Update(Guid id)
        {
            var realEstate = await _realEstateServices.DetailAsync(id);
            if (realEstate == null)
            {
                return NotFound();
            }

            var vm = new RealEstateCreateUpdateViewModel
            {
                Id = realEstate.Id,
                Address = realEstate.Address,
                Area = realEstate.Area,
                RoomCount = realEstate.RoomCount,
                Price = realEstate.Price
            };
            return View("CreateUpdate", vm);
        }

        // UPDATE POST - salvestab kinnisvara muudetud väljad.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(RealEstateCreateUpdateViewModel vm)
        {
            if (!vm.Id.HasValue || vm.Id.Value == Guid.Empty)
            {
                return BadRequest();
            }

            if (await _realEstateServices.DetailAsync(vm.Id.Value) == null)
            {
                return NotFound();
            }

            ValidateForm(vm);
            if (!ModelState.IsValid)
            {
                // Vigase vormi korral säilitame sisestatud väärtused ja näitame veateateid.
                return View("CreateUpdate", vm);
            }

            var updated = await _realEstateServices.Update(ToDto(vm));
            if (updated == null)
            {
                return NotFound();
            }
            return RedirectToAction(nameof(Index));
        }

        // DETAILS - kuvab ühe objekti põhiandmed ja kuupäevad.
        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            var realEstate = await _realEstateServices.DetailAsync(id);
            if (realEstate == null)
            {
                return NotFound();
            }

            var vm = new RealEstateDetailsViewModel
            {
                Id = realEstate.Id,
                Address = realEstate.Address,
                Area = realEstate.Area,
                RoomCount = realEstate.RoomCount,
                Price = realEstate.Price,
                CreatedAt = realEstate.CreatedAt,
                UpdatedAt = realEstate.UpdatedAt
            };
            return View(vm);
        }

        // DELETE GET - näitab kinnitamise lehte; GET päring ise midagi ei kustuta.
        [HttpGet]
        public async Task<IActionResult> Delete(Guid id)
        {
            var realEstate = await _realEstateServices.DetailAsync(id);
            if (realEstate == null)
            {
                return NotFound();
            }

            var vm = new RealEstateDeleteViewModel
            {
                Id = realEstate.Id,
                Address = realEstate.Address,
                Area = realEstate.Area,
                RoomCount = realEstate.RoomCount,
                Price = realEstate.Price,
                CreatedAt = realEstate.CreatedAt,
                UpdatedAt = realEstate.UpdatedAt
            };
            return View(vm);
        }

        // DELETE POST - kustutab kasutaja kinnitatud objekti.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmation(Guid id)
        {
            if (!ModelState.IsValid || id == Guid.Empty)
            {
                return BadRequest();
            }

            var deleted = await _realEstateServices.Delete(id);
            if (deleted == null)
            {
                return NotFound();
            }
            return RedirectToAction(nameof(Index));
        }

        // Teisendame vormi muudetavad väljad teenusele edastatavaks DTO-ks.
        private static RealEstateDto ToDto(RealEstateCreateUpdateViewModel vm)
        {
            return new RealEstateDto
            {
                Id = vm.Id,
                Address = vm.Address,
                Area = vm.Area,
                RoomCount = vm.RoomCount,
                Price = vm.Price
            };
        }

        // Täiendavad kontrollid käivitatakse serveris ka siis, kui brauseri kontroll puudub.
        private void ValidateForm(RealEstateCreateUpdateViewModel vm)
        {
            // Andmebaas salvestab kaks kümnendkohta; väldime vaikset ümardamist.
            if (vm.Area != decimal.Round(vm.Area, 2))
            {
                ModelState.AddModelError(nameof(vm.Area), "Pindalal võib olla kuni kaks kümnendkohta.");
            }
            if (vm.Price != decimal.Round(vm.Price, 2))
            {
                ModelState.AddModelError(nameof(vm.Price), "Hinnal võib olla kuni kaks kümnendkohta.");
            }
        }
    }
}
