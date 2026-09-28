using TARge25Shop.Core.Domain;
using TARge25Shop.Core.Dto;

namespace TARge25Shop.Core.ServiceInterface
{
    // Liides kirjeldab failiteenuse toimingud, mida kosmoselaevade teenus kasutab.
    public interface IFileServices
    {
        // Salvestab DTO failid kettale ja lisab FileToApi kirjed DbContexti.
        void FilesToApi(SpaceshipDto dto, Spaceship domain);

        // Kustutab ühe pildi faili ja andmebaasikirje; puuduv pilt annab false.
        Task<bool> RemoveImageFromApi(FileToApiDto dto);

        // Eemaldab ühe kosmoselaeva failid kettalt ja DbContextist.
        void DeleteFiles(Guid spaceshipId);
    }
}
