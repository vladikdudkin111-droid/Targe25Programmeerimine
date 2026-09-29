using Microsoft.EntityFrameworkCore;
using TARge25Shop.Core.Domain;
using TARge25Shop.Core.Dto;
using TARge25Shop.Core.ServiceInterface;
using TARge25Shop.Data;

namespace TARge25Shop.ApplicationServices.Services
{
    // Teenus sisaldab kinnisvara CRUD loogikat, nagu SpaceshipServices.
    public class RealEstateServices : IRealEstateServices
    {
        // DbContext salvestab kinnisvara andmebaasikirjed.
        private readonly TARge25ShopContext _context;

        // ASP.NET Core annab konstruktorile ühe päringu ühised sõltuvused.
        public RealEstateServices(TARge25ShopContext context)
        {
            _context = context;
        }

        // CREATE - teisendab DTO Domain objektiks ja salvestab selle.
        public async Task<RealEstate> Create(RealEstateDto dto)
        {
            var now = DateTime.Now;
            var realEstate = new RealEstate
            {
                Id = Guid.NewGuid(),
                Address = dto.Address.Trim(),
                Area = dto.Area,
                RoomCount = dto.RoomCount,
                Price = dto.Price,
                CreatedAt = now,
                UpdatedAt = now
            };

            // Add lisab kirje jälgimisse ja SaveChanges kirjutab selle andmebaasi.
            _context.RealEstates.Add(realEstate);
            await _context.SaveChangesAsync();
            return realEstate;
        }

        // UPDATE - uuendab olemasoleva kirje muudetavaid välju.
        public async Task<RealEstate?> Update(RealEstateDto dto)
        {
            if (!dto.Id.HasValue || dto.Id.Value == Guid.Empty)
            {
                return null;
            }

            // Jälgitava objekti muutused tuvastab Entity Framework automaatselt.
            var realEstate = await _context.RealEstates
                .FirstOrDefaultAsync(item => item.Id == dto.Id.Value);
            if (realEstate == null)
            {
                return null;
            }

            // CreatedAt säilib; vorm ei saa serveri määratud loomise aega muuta.
            realEstate.Address = dto.Address.Trim();
            realEstate.Area = dto.Area;
            realEstate.RoomCount = dto.RoomCount;
            realEstate.Price = dto.Price;
            realEstate.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return realEstate;
        }

        // DETAILS - tagastab ühe objekti või null, kui seda enam ei ole.
        public async Task<RealEstate?> DetailAsync(Guid id)
        {
            return await _context.RealEstates.FirstOrDefaultAsync(item => item.Id == id);
        }

        // DELETE - eemaldab valitud kinnisvaraobjekti.
        public async Task<RealEstate?> Delete(Guid id)
        {
            var realEstate = await _context.RealEstates.FirstOrDefaultAsync(item => item.Id == id);
            if (realEstate == null)
            {
                return null;
            }

            // Remove märgib leitud kirje kustutatuks; SaveChanges rakendab muudatuse.
            _context.RealEstates.Remove(realEstate);
            await _context.SaveChangesAsync();
            return realEstate;
        }
    }
}
