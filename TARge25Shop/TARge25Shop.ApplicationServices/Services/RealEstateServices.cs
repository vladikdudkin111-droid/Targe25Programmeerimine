using Microsoft.EntityFrameworkCore;
using TARge25Shop.Core.Domain;
using TARge25Shop.Core.Dto;
using TARge25Shop.Core.ServiceInterface;
using TARge25Shop.Data;

namespace TARge25Shop.ApplicationServices.Services
{
    // Teenus juhib andmete ja seotud piltide loomist, muutmist ning kustutamist.
    public class RealEstateServices : IRealEstateServices
    {
        private readonly TARge25ShopContext _context;
        private readonly IFileServices _fileServices;

        public RealEstateServices(TARge25ShopContext context, IFileServices fileServices)
        {
            _context = context;
            _fileServices = fileServices;
        }

        public async Task<RealEstate> Create(RealEstateDto dto)
        {
            // Uue objekti ID ja kuupäevad määrab server.
            RealEstate realEstate = new();
            realEstate.Id = Guid.NewGuid();
            realEstate.Area = dto.Area;
            realEstate.Location = dto.Location.Trim();
            realEstate.RoomNumber = dto.RoomNumber;
            realEstate.BuildingType = dto.BuildingType.Trim();
            realEstate.CreatedAt = DateTime.Now;
            realEstate.ModifiedAt = realEstate.CreatedAt;
            _fileServices.UploadFilesToDatabase(dto, realEstate);
            _context.RealEstates.Add(realEstate);
            await _context.SaveChangesAsync();
            return realEstate;
        }

        public async Task<RealEstate?> Update(RealEstateDto dto)
        {
            if (!dto.Id.HasValue || dto.Id.Value == Guid.Empty) return null;
            var realEstate = await _context.RealEstates.FirstOrDefaultAsync(x => x.Id == dto.Id.Value);
            if (realEstate == null) return null;

            // Muudame jälgitavat kirjet; CreatedAt jääb andmebaasis säilinud väärtuseks.
            realEstate.Area = dto.Area;
            realEstate.Location = dto.Location.Trim();
            realEstate.RoomNumber = dto.RoomNumber;
            realEstate.BuildingType = dto.BuildingType.Trim();
            realEstate.ModifiedAt = DateTime.Now;
            // Uued pildid lisanduvad olemasolevatele.
            _fileServices.UploadFilesToDatabase(dto, realEstate);
            await _context.SaveChangesAsync();
            return realEstate;
        }

        public async Task<RealEstate?> DetailAsync(Guid id)
        {
            return await _context.RealEstates.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<RealEstate?> Delete(Guid id)
        {
            var realEstate = await _context.RealEstates.FirstOrDefaultAsync(x => x.Id == id);
            if (realEstate == null) return null;
            // Eemaldame andmebaasist kõik selle kinnisvara pildid koos omanikuga.
            var images = await _context.FileToDatabases.Where(x => x.RealEstateId == id).ToListAsync();
            _context.FileToDatabases.RemoveRange(images);
            _context.RealEstates.Remove(realEstate);
            await _context.SaveChangesAsync();
            return realEstate;
        }
    }
}
