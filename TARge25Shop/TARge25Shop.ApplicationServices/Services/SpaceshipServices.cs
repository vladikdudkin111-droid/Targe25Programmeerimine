using Microsoft.EntityFrameworkCore;
using TARge25Shop.Core.Domain;
using TARge25Shop.Core.Dto;
using TARge25Shop.Core.ServiceInterface;
using TARge25Shop.Data;

namespace TARge25Shop.ApplicationServices.Services
{
    // SpaceshipServices sisaldab kosmoselaevade põhilist äriloogikat.
    public class SpaceshipServices : ISpaceshipServices
    {
        // DbContext annab ligipääsu andmebaasi tabelitele.
        private readonly TARge25ShopContext _context;

        // Failiteenus salvestab ja kustutab kosmoselaevaga seotud failid.
        private readonly IFileServices _fileServices;

        // Konstruktor saab vajalikud sõltuvused dependency injection konteinerist.
        public SpaceshipServices(TARge25ShopContext context, IFileServices fileServices)
        {
            _context = context;
            _fileServices = fileServices;
        }

        // CREATE - loob uue kosmoselaeva ja salvestab selle koos failidega.
        public async Task<Spaceship> Create(SpaceshipDto dto)
        {
            // Teisendame DTO andmed uueks Domain objektiks.
            var spaceship = new Spaceship
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                ShipType = dto.ShipType,
                Crew = dto.Crew,
                EnginePower = dto.EnginePower,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            // Lisame failid samasse DbContexti, et üks SaveChanges salvestaks kõik kirjed.
            _fileServices.FilesToApi(dto, spaceship);
            _context.Spaceships.Add(spaceship);
            await _context.SaveChangesAsync();

            return spaceship;
        }

        // UPDATE - muudab olemasolevat kosmoselaeva ja lisab uued failid.
        public async Task<Spaceship> Update(SpaceshipDto dto)
        {
            // Update vajab olemasoleva kirje ID-d.
            if (!dto.Id.HasValue)
            {
                throw new InvalidOperationException("Spaceship ID is required for update.");
            }

            // Loeme olemasoleva jälgitava objekti andmebaasist.
            var spaceship = await _context.Spaceships
                .FirstOrDefaultAsync(item => item.Id == dto.Id.Value);

            if (spaceship == null)
            {
                throw new InvalidOperationException("Spaceship not found.");
            }

            // Muudame ainult kasutaja poolt redigeeritavaid välju.
            spaceship.Name = dto.Name;
            spaceship.ShipType = dto.ShipType;
            spaceship.Crew = dto.Crew;
            spaceship.EnginePower = dto.EnginePower;
            spaceship.UpdatedAt = DateTime.Now;

            // Lisame muutmise vormil valitud uued failid.
            _fileServices.FilesToApi(dto, spaceship);
            await _context.SaveChangesAsync();

            return spaceship;
        }

        // DETAILS - tagastab ühe kosmoselaeva ID järgi.
        public async Task<Spaceship?> DetailAsync(Guid id)
        {
            return await _context.Spaceships
                .FirstOrDefaultAsync(item => item.Id == id);
        }

        // DELETE - kustutab kosmoselaeva, selle failikirjed ja füüsilised failid.
        public async Task<Spaceship?> Delete(Guid id)
        {
            // Otsime kustutatava kosmoselaeva.
            var spaceship = await _context.Spaceships
                .FirstOrDefaultAsync(item => item.Id == id);

            // Puuduva kirje korral tagastame null ja väldime Remove(null) viga.
            if (spaceship == null)
            {
                return null;
            }

            // Eemaldame seotud failid enne kosmoselaeva andmebaasikirjet.
            _fileServices.DeleteFiles(id);
            _context.Spaceships.Remove(spaceship);
            await _context.SaveChangesAsync();

            return spaceship;
        }
    }
}
