using TARge25Shop.Core.Domain;
using TARge25Shop.Core.Dto;
using TARge25Shop.Core.ServiceInterface;
using TARge25Shop.Data;

namespace TARge25Shop.ApplicationServices.Services
{
    // Service klass sisaldab kosmoselaevadega seotud põhilist äriloogikat.
    public class SpaceshipServices : ISpaceshipServices
    {
        // DbContext annab ligipääsu andmebaasi tabelitele.
        private readonly TARge25ShopContext _context;

        // Failiteenus salvestab ja kustutab kosmoselaevaga seotud faile.
        private readonly IFileServices _fileServices;

        // Constructor saab DbContexti dependency injection kaudu.
        public SpaceshipServices(TARge25ShopContext context, IFileServices fileServices)
        {
            _context = context;
            _fileServices = fileServices;
        }

        // CREATE - loob uue kosmoselaeva ja salvestab selle andmebaasi.
        public async Task<Spaceship> Create(SpaceshipDto dto)
        {
            // Loome uue Domain objekti.
            Spaceship spaceShip = new();

            // Kanname DTO andmed Domain objekti sisse.
            spaceShip.Id = Guid.NewGuid();
            spaceShip.Name = dto.Name;
            spaceShip.ShipType = dto.ShipType;
            spaceShip.Crew = dto.Crew;
            spaceShip.EnginePower = dto.EnginePower;
            spaceShip.CreatedAt = DateTime.Now;
            spaceShip.UpdatedAt = DateTime.Now;

            // Lisame uue objekti DbSeti ja salvestame muudatused andmebaasi.
            _context.Spaceships.Add(spaceShip);
            await _context.SaveChangesAsync();

            // Salvestame valitud failid kettale ja FileToApis tabelisse.
            _fileServices.FilesToApi(dto, spaceShip);

            // Tagastame loodud kosmoselaeva.
            return spaceShip;
        }

        // UPDATE - uuendab olemasoleva kosmoselaeva andmeid.
        public async Task<Spaceship> Update(SpaceshipDto dto)
        {
            // Otsime kosmoselaeva andmebaasist ID järgi.
            var spaceShip = await _context.Spaceships.FindAsync(dto.Id);

            // Kui kosmoselaeva ei leitud, katkestame tegevuse veaga.
            if (spaceShip == null)
            {
                throw new InvalidOperationException("Spaceship not found");
            }

            // Uuendame olemasoleva objekti väljad DTO andmetega.
            spaceShip.Name = dto.Name;
            spaceShip.ShipType = dto.ShipType;
            spaceShip.Crew = dto.Crew;
            spaceShip.EnginePower = dto.EnginePower;
            spaceShip.UpdatedAt = DateTime.Now;

            // Salvestame muudatused andmebaasi.
            await _context.SaveChangesAsync();

            // Kustutame märgitud failid ja salvestame juurde lisatud uued failid.
            _fileServices.DeleteFilesFromApi(spaceShip.Id, dto.FileNamesToDelete);
            _fileServices.FilesToApi(dto, spaceShip);

            // Tagastame uuendatud kosmoselaeva.
            return spaceShip;
        }

        // DETAILS - otsib ja tagastab ühe kosmoselaeva ID järgi.
        public async Task<Spaceship?> DetailAsync(Guid id)
        {
            return await _context.Spaceships.FindAsync(id);
        }

        // DELETE - kustutab kosmoselaeva ID järgi.
        public async Task<Spaceship?> Delete(Guid id)
        {
            // Otsime kõigepealt kustutatava kosmoselaeva.
            var spaceShip = await _context.Spaceships.FindAsync(id);

            // Kui objekti ei leitud, ei ole midagi kustutada.
            if (spaceShip == null)
            {
                return null;
            }

            // Eemaldame objekti DbSetist ja salvestame muudatuse andmebaasi.
            _context.Spaceships.Remove(spaceShip);
            await _context.SaveChangesAsync();

            // Kustutame kosmoselaevaga seotud füüsilised failid ja FileToApi kirjed.
            _fileServices.DeleteDirectoryFromApi(spaceShip.Id);

            // Tagastame kustutatud objekti.
            return spaceShip;
        }
    }
}
