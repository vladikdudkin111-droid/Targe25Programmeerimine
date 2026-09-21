using TARge25Shop.Core.Domain;
using TARge25Shop.Core.Dto;

namespace TARge25Shop.Core.ServiceInterface
{
    // Liides kirjeldab kõik failiteenuse toimingud, mida teised kihid saavad kasutada.
    public interface IFileServices
    {
        // Salvestab DTO failid kettale ja seob need andmebaasis kosmoselaevaga.
        void FilesToApi(SpaceshipDto dto, Spaceship domain);

        // Tagastab kõik ühe kosmoselaevaga seotud failid.
        IReadOnlyList<FileToApiDto> FilesFromApi(Guid spaceshipId);

        // Tagastab ühe kindla faili või null, kui faili ei leitud.
        FileToApiDto? FileFromApi(Guid spaceshipId, string storedFileName);

        // Kustutab valitud failid kettalt ja andmebaasist.
        void DeleteFilesFromApi(Guid spaceshipId, IEnumerable<string> storedFileNames);

        // Kustutab kõik ühe kosmoselaevaga seotud failid.
        void DeleteDirectoryFromApi(Guid spaceshipId);
    }
}
