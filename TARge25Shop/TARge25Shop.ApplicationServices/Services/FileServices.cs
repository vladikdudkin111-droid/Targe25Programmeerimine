using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using TARge25Shop.Core.Domain;
using TARge25Shop.Core.Dto;
using TARge25Shop.Core.ServiceInterface;
using TARge25Shop.Core.Validation;
using TARge25Shop.Data;

namespace TARge25Shop.ApplicationServices.Services
{
    // Kosmoselaeva pildid asuvad kettal, kinnisvara pildid andmebaasis.
    public class FileServices : IFileServices
    {
        private readonly IHostEnvironment _webHost;
        private readonly TARge25ShopContext _context;

        public FileServices(IHostEnvironment webHost, TARge25ShopContext context)
        {
            _webHost = webHost;
            _context = context;
        }

        private string UploadsFolder => Path.Combine(
            _webHost.ContentRootPath, "wwwroot", "multipleFileUpload");

        public void FilesToApi(SpaceshipDto dto, Spaceship domain)
        {
            // Kontrollime kogu valikut enne esimese faili kirjutamist.
            var error = ImageUploadRules.Validate(dto.Files);
            if (error != null) throw new ArgumentException(error, nameof(dto));
            if (dto.Files == null || dto.Files.Count == 0) return;
            Directory.CreateDirectory(UploadsFolder);

            foreach (var file in dto.Files)
            {
                // Kasutaja failinimest eemaldame kaustad; GUID väldib nimede kattumist.
                var originalName = Path.GetFileName(file.FileName.Replace('\\', '/'));
                var uniqueFileName = Guid.NewGuid().ToString("N") + "_" + originalName;
                var filePath = Path.Combine(UploadsFolder, uniqueFileName);
                using (var stream = new FileStream(filePath, FileMode.CreateNew))
                {
                    file.CopyTo(stream);
                }

                // Add märgib uue kirje; CRUD-teenus salvestab selle koos kosmoselaevaga.
                _context.FileToApis.Add(new FileToApi
                {
                    Id = Guid.NewGuid(),
                    ExistingFilePath = uniqueFileName,
                    SpaceshipId = domain.Id
                });
            }
        }

        public void UploadFilesToDatabase(RealEstateDto dto, RealEstate domain)
        {
            var error = ImageUploadRules.Validate(dto.Files);
            if (error != null) throw new ArgumentException(error, nameof(dto));
            if (dto.Files == null || dto.Files.Count == 0) return;

            foreach (var file in dto.Files)
            {
                // MemoryStream hoiab faili baite kuni andmebaasikirje koostamiseni.
                using (var target = new MemoryStream())
                {
                    FileToDatabase files = new()
                    {
                        Id = Guid.NewGuid(),
                        ImageTitle = Path.GetFileName(file.FileName.Replace('\\', '/')),
                        RealEstateId = domain.Id
                    };
                    file.CopyTo(target);
                    files.ImageData = target.ToArray();
                    _context.FileToDatabases.Add(files);
                }
            }
        }

        public async Task<FileToApi?> RemoveImageFromApi(FileToApiDto dto)
        {
            // Omaniku ID takistab teise kosmoselaeva pildi juhuslikku kustutamist.
            var image = await _context.FileToApis.FirstOrDefaultAsync(x =>
                x.Id == dto.Id && x.SpaceshipId == dto.SpaceshipId);
            if (image == null) return null;
            DeletePhysicalFile(image.ExistingFilePath);
            _context.FileToApis.Remove(image);
            await _context.SaveChangesAsync();
            return image;
        }

        public async Task<List<FileToApi>> RemoveImagesFromApi(FileToApiDto[] dtos)
        {
            var removed = new List<FileToApi>();
            foreach (var dto in dtos.DistinctBy(x => x.Id))
            {
                var image = await _context.FileToApis.FirstOrDefaultAsync(x =>
                    x.Id == dto.Id && x.SpaceshipId == dto.SpaceshipId);
                if (image == null) continue;
                DeletePhysicalFile(image.ExistingFilePath);
                _context.FileToApis.Remove(image);
                removed.Add(image);
            }
            // Kutsuja salvestab kõik eemaldamised ühe SaveChangesAsync-kutsega.
            return removed;
        }

        public async Task<FileToDatabase?> RemoveImageFromDatabase(FileToDatabaseDto dto)
        {
            // Kontrollime nii faili kui ka kinnisvara tunnust.
            var image = await _context.FileToDatabases.FirstOrDefaultAsync(x =>
                x.Id == dto.Id && x.RealEstateId == dto.RealEstateId);
            if (image == null) return null;
            _context.FileToDatabases.Remove(image);
            await _context.SaveChangesAsync();
            return image;
        }

        private void DeletePhysicalFile(string? storedName)
        {
            if (string.IsNullOrWhiteSpace(storedName)) return;
            var safeName = Path.GetFileName(storedName.Replace('\\', '/'));
            // Salvestatud nimi peab olema ainult failinimi, mitte kaustatee.
            if (safeName != storedName) return;
            var path = Path.Combine(UploadsFolder, safeName);
            if (File.Exists(path)) File.Delete(path);
        }
    }
}
