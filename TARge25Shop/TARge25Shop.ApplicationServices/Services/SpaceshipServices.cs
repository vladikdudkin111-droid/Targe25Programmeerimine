using Microsoft.EntityFrameworkCore;
using TARge25Shop.Core.Domain;
using TARge25Shop.Core.Dto;
using TARge25Shop.Core.ServiceInterface;
using TARge25Shop.Data;

namespace TARge25Shop.ApplicationServices.Services
{
    // Teenus juhib andmete ja seotud piltide loomist, muutmist ning kustutamist.
    public class SpaceshipServices : ISpaceshipServices
    {
        private readonly TARge25ShopContext _context;
        private readonly IFileServices _fileServices;

        public SpaceshipServices(TARge25ShopContext context, IFileServices fileServices)
        {
            _context = context;
            _fileServices = fileServices;
        }

        public async Task<Spaceship> Create(SpaceshipDto dto)
        {
            // Uue objekti ID ja kuupäevad määrab server.
            Spaceship spaceship = new();
            spaceship.Id = Guid.NewGuid();
            spaceship.Name = dto.Name.Trim();
            spaceship.ShipType = dto.ShipType.Trim();
            spaceship.Crew = dto.Crew;
            spaceship.EnginePower = dto.EnginePower;
            spaceship.CreatedAt = DateTime.Now;
            spaceship.UpdatedAt = spaceship.CreatedAt;
            _fileServices.FilesToApi(dto, spaceship);
            _context.Spaceships.Add(spaceship);
            await _context.SaveChangesAsync();
            return spaceship;
        }

        public async Task<Spaceship?> Update(SpaceshipDto dto)
        {
            if (!dto.Id.HasValue || dto.Id.Value == Guid.Empty) return null;
            var spaceship = await _context.Spaceships.FirstOrDefaultAsync(x => x.Id == dto.Id.Value);
            if (spaceship == null) return null;

            // Muudame jälgitavat kirjet; CreatedAt jääb andmebaasis säilinud väärtuseks.
            spaceship.Name = dto.Name.Trim();
            spaceship.ShipType = dto.ShipType.Trim();
            spaceship.Crew = dto.Crew;
            spaceship.EnginePower = dto.EnginePower;
            spaceship.UpdatedAt = DateTime.Now;
            // Uued pildid lisanduvad olemasolevatele.
            _fileServices.FilesToApi(dto, spaceship);
            await _context.SaveChangesAsync();
            return spaceship;
        }

        public async Task<Spaceship?> DetailAsync(Guid id)
        {
            return await _context.Spaceships.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<Spaceship?> Delete(Guid id)
        {
            var spaceship = await _context.Spaceships.FirstOrDefaultAsync(x => x.Id == id);
            if (spaceship == null) return null;
            // Eemaldame kettalt failid ja märgime nende kirjed kustutatuks.
            var images = await _context.FileToApis.Where(x => x.SpaceshipId == id)
                .Select(x => new FileToApiDto
                {
                    Id = x.Id, SpaceshipId = x.SpaceshipId, ExistingFilePath = x.ExistingFilePath
                }).ToArrayAsync();
            await _fileServices.RemoveImagesFromApi(images);
            _context.Spaceships.Remove(spaceship);
            await _context.SaveChangesAsync();
            return spaceship;
        }
    }
}
