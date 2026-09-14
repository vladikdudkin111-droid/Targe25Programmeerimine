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

        // Andmebaasi konteksti kasutame nimekirja lugemiseks.
        private readonly TARge25ShopContext _context;

        // Constructor saab vajalikud sõltuvused dependency injection kaudu.
        public SpaceshipController(
            ISpaceshipServices spaceshipServices,
            TARge25ShopContext context)
        {
            _spaceshipServices = spaceshipServices;
            _context = context;
        }

        // INDEX - kuvab kõik andmebaasis olevad kosmoselaevad.
        public async Task<IActionResult> Index()
        {
            // Loeme kosmoselaevad andmebaasist ja järjestame need loomise aja järgi.
            var spaceships = await _context.Spaceships
                .OrderBy(x => x.CreatedAt)
                .Select(x => new SpaceshipIndexViewModel
                {
                    // Teisendame Domain objekti ViewModeliks, mida vaade kasutab.
                    Id = x.Id,
                    Name = x.Name,
                    ShipType = x.ShipType,
                    Crew = x.Crew,
                    EnginePower = x.EnginePower,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt
                })
                .ToListAsync();

            // Anname kosmoselaevade nimekirja Index vaatele.
            return View(spaceships);
        }

        // CREATE GET - avab uue kosmoselaeva loomise vormi.
        [HttpGet]
        public IActionResult Create()
        {
            // Anname vaatele tühja ViewModeli.
            return View(new SpaceshipCreateViewModel());
        }

        // CREATE POST - võtab vormilt andmed vastu ja loob uue kosmoselaeva.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SpaceshipCreateViewModel vm)
        {
            // Kui sisestatud andmed ei vasta valideerimisreeglitele, näitame vormi uuesti.
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            // Teisendame ViewModeli DTO-ks, et saata andmed teenusesse.
            var dto = new SpaceshipDto
            {
                Name = vm.Name,
                ShipType = vm.ShipType,
                Crew = vm.Crew,
                EnginePower = vm.EnginePower
            };

            // Kutsume teenuse välja ja salvestame uue kosmoselaeva andmebaasi.
            await _spaceshipServices.Create(dto);

            // Pärast loomist läheme tagasi kosmoselaevade nimekirja.
            return RedirectToAction(nameof(Index));
        }

        // DETAILS GET - kuvab ühe kosmoselaeva detailse info.
        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            // Otsime kosmoselaeva ID järgi.
            var spaceship = await _spaceshipServices.DetailAsync(id);

            // Kui sellise ID-ga kosmoselaeva ei ole, tagastame 404 vastuse.
            if (spaceship == null)
            {
                return NotFound();
            }

            // Teisendame Domain objekti ViewModeliks ja saadame Details vaatele.
            var vm = ToIndexViewModel(spaceship);
            return View(vm);
        }

        // UPDATE GET - avab olemasoleva kosmoselaeva muutmise vormi.
        [HttpGet]
        public async Task<IActionResult> Update(Guid id)
        {
            // Otsime muudetava kosmoselaeva ID järgi.
            var spaceship = await _spaceshipServices.DetailAsync(id);

            // Kui kosmoselaeva ei leitud, tagastame 404 vastuse.
            if (spaceship == null)
            {
                return NotFound();
            }

            // Täidame UpdateViewModeli olemasolevate andmetega.
            var vm = new SpaceshipUpdateViewModel
            {
                Id = spaceship.Id,
                Name = spaceship.Name,
                ShipType = spaceship.ShipType,
                Crew = spaceship.Crew,
                EnginePower = spaceship.EnginePower
            };

            // Avame Update vaate koos olemasolevate andmetega.
            return View(vm);
        }

        // UPDATE POST - salvestab kasutaja tehtud muudatused.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(SpaceshipUpdateViewModel vm)
        {
            // Kontrollime, et vormi andmed oleksid korrektsed.
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            // Kontrollime enne muutmist, kas kosmoselaev on veel andmebaasis olemas.
            var existing = await _spaceshipServices.DetailAsync(vm.Id);
            if (existing == null)
            {
                return NotFound();
            }

            // Teisendame vormi andmed DTO-ks.
            var dto = new SpaceshipDto
            {
                Id = vm.Id,
                Name = vm.Name,
                ShipType = vm.ShipType,
                Crew = vm.Crew,
                EnginePower = vm.EnginePower
            };

            // Kutsume teenuse Update meetodi ja salvestame muudatused.
            await _spaceshipServices.Update(dto);

            // Pärast muutmist läheme tagasi nimekirja.
            return RedirectToAction(nameof(Index));
        }

        // DELETE GET - avab kustutamise kinnitamise lehe.
        [HttpGet]
        public async Task<IActionResult> Delete(Guid id)
        {
            // Otsime kustutatava kosmoselaeva ID järgi.
            var spaceship = await _spaceshipServices.DetailAsync(id);

            // Kui kosmoselaeva ei leitud, tagastame 404 vastuse.
            if (spaceship == null)
            {
                return NotFound();
            }

            // Anname kosmoselaeva andmed Delete vaatele.
            var vm = ToIndexViewModel(spaceship);
            return View(vm);
        }

        // DELETE POST - kustutab kosmoselaeva pärast kasutaja kinnitust.
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            // Kutsume teenuse Delete meetodi.
            var deleted = await _spaceshipServices.Delete(id);

            // Kui kirjet enam ei eksisteeri, tagastame 404 vastuse.
            if (deleted == null)
            {
                return NotFound();
            }

            // Pärast kustutamist läheme tagasi nimekirja.
            return RedirectToAction(nameof(Index));
        }

        // Abimeetod teisendab Domain objekti IndexViewModeliks.
        private static SpaceshipIndexViewModel ToIndexViewModel(Spaceship spaceship)
        {
            return new SpaceshipIndexViewModel
            {
                Id = spaceship.Id,
                Name = spaceship.Name,
                ShipType = spaceship.ShipType,
                Crew = spaceship.Crew,
                EnginePower = spaceship.EnginePower,
                CreatedAt = spaceship.CreatedAt,
                UpdatedAt = spaceship.UpdatedAt
            };
        }
    }
}
