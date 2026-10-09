using TARge25Shop.Core.Domain;
using TARge25Shop.Core.Dto;
using TARge25Shop.Core.ServiceInterface;
using TARge25Shop.Data;

namespace TARge25Shop.ApplicationServices.Services
{
    // Teenus sisaldab lasteaiarühmade salvestamise loogikat.
    public class KindergartenServices : IKindergartenServices
    {
        private readonly TARge25ShopContext _context;
        private readonly IFileService _fileService;

        public KindergartenServices(TARge25ShopContext context, IFileService fileService)
        {
            _context = context;
            _fileService = fileService;
        }

        public async Task<Kindergarten> Create(KindergartenDto dto)
        {
            var images = _fileService.PrepareImages(dto.Files);
            // ID ja kuupäevad määrame serveris, mitte vormi saadetud väärtuste põhjal.
            var now = DateTime.Now;
            var kindergarten = new Kindergarten
            {
                Id = Guid.NewGuid(),
                GroupName = dto.GroupName,
                ChildrenCount = dto.ChildrenCount,
                KindergartenName = dto.KindergartenName,
                TeacherName = dto.TeacherName,
                CreatedAt = now,
                UpdatedAt = now,
                Images = images
            };

            _context.Kindergartens.Add(kindergarten);
            await _context.SaveChangesAsync();
            return kindergarten;
        }

        public async Task<Kindergarten?> Update(KindergartenDto dto)
        {
            var kindergarten = await _context.Kindergartens.FindAsync(dto.Id);
            if (kindergarten == null)
            {
                return null;
            }

            var images = _fileService.PrepareImages(dto.Files);
            foreach (var image in images)
            {
                image.KindergartenId = kindergarten.Id;
                _context.KindergartenImages.Add(image);
            }

            // Loomise aeg jääb muutmata; uuendame ainult muudetavaid välju ja muutmise aega.
            kindergarten.GroupName = dto.GroupName;
            kindergarten.ChildrenCount = dto.ChildrenCount;
            kindergarten.KindergartenName = dto.KindergartenName;
            kindergarten.TeacherName = dto.TeacherName;
            kindergarten.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return kindergarten;
        }

        public async Task<Kindergarten?> DetailAsync(Guid id)
        {
            // Puuduva kirje korral tagastame null.
            return await _context.Kindergartens.FindAsync(id);
        }

        public async Task<Kindergarten?> Delete(Guid id)
        {
            var kindergarten = await _context.Kindergartens.FindAsync(id);
            if (kindergarten == null)
            {
                return null;
            }

            // Võõrvõtme kaskaadkustutus eemaldab pildid ka siis, kui neid pole laaditud.
            _context.Kindergartens.Remove(kindergarten);
            await _context.SaveChangesAsync();
            return kindergarten;
        }
    }
}
