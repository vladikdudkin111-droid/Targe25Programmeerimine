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

        public KindergartenController(
            IKindergartenServices kindergartenServices,
            TARge25ShopContext context)
        {
            _kindergartenServices = kindergartenServices;
            _context = context;
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
        public async Task<IActionResult> Create(KindergartenCreateUpdateViewModel vm)
        {
            // Loomisel ei kasutata brauserist saadetud ID-d.
            vm.Id = Guid.Empty;
            ModelState.Remove(nameof(vm.Id));
            if (!ModelState.IsValid)
            {
                return View("CreateUpdate", vm);
            }

            await _kindergartenServices.Create(ToDto(vm));
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
                UpdatedAt = kindergarten.UpdatedAt
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
                TeacherName = kindergarten.TeacherName
            };
            return View("CreateUpdate", vm);
        }

        // UPDATE POST - salvestab kehtivad muudatused.
        [HttpPost]
        [ValidateAntiForgeryToken]
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
                return View("CreateUpdate", vm);
            }

            var updated = await _kindergartenServices.Update(ToDto(vm));
            if (updated == null)
            {
                return NotFound();
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
                UpdatedAt = kindergarten.UpdatedAt
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
