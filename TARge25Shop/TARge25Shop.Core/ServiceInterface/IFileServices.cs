using TARge25Shop.Core.Domain;
using TARge25Shop.Core.Dto;

namespace TARge25Shop.Core.ServiceInterface
{
    // Failiteenus seob üleslaaditud pildid nende omaniku ID-ga.
    public interface IFileServices
    {
        void FilesToApi(SpaceshipDto dto, Spaceship domain);
        Task<FileToApi?> RemoveImageFromApi(FileToApiDto dto);
        // Mitme faili eemaldamise salvestab kutsuv CRUD-teenus koos omanikuga.
        Task<List<FileToApi>> RemoveImagesFromApi(FileToApiDto[] dtos);
        void UploadFilesToDatabase(RealEstateDto dto, RealEstate domain);
        Task<FileToDatabase?> RemoveImageFromDatabase(FileToDatabaseDto dto);
    }
}
