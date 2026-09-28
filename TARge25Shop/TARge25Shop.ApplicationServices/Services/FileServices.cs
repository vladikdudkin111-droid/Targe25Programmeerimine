using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using TARge25Shop.Core.Domain;
using TARge25Shop.Core.Dto;
using TARge25Shop.Core.ServiceInterface;
using TARge25Shop.Data;

namespace TARge25Shop.ApplicationServices.Services
{
    // FileServices vastutab failide füüsilise salvestamise ja kustutamise eest.
    public class FileServices : IFileServices
    {
        // Kõik üleslaaditud failid asuvad selles wwwroot alamkaustas.
        private const string UploadFolderName = "multipleFileUpload";

        // IHostEnvironment annab rakenduse juurkausta füüsilise asukoha.
        private readonly IHostEnvironment _webHost;

        // DbContexti kasutame FileToApi kirjete lisamiseks ja eemaldamiseks.
        private readonly TARge25ShopContext _context;

        // Konstruktor saab sõltuvused dependency injection konteinerist.
        public FileServices(IHostEnvironment webHost, TARge25ShopContext context)
        {
            _webHost = webHost;
            _context = context;
        }

        // Meetod salvestab kõik DTO-ga saadud failid õpetaja näite järgi.
        public void FilesToApi(SpaceshipDto dto, Spaceship domain)
        {
            // Kui kasutaja ei valinud faile, ei ole midagi salvestada.
            if (dto.Files == null || dto.Files.Count == 0)
            {
                return;
            }

            // Koostame absoluutse tee kausta wwwroot\multipleFileUpload.
            var uploadsFolder = GetUploadsFolder();

            // Kui kausta veel ei ole, loome selle automaatselt.
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // Töötleme kõik mittetühjad failid ükshaaval.
            foreach (var file in dto.Files.Where(file => file.Length > 0))
            {
                // Path.GetFileName eemaldab kasutaja failinimest võimalikud kaustateed.
                var safeFileName = Path.GetFileName(file.FileName);

                // Guid muudab nime unikaalseks ja väldib sama nimega faili ülekirjutamist.
                var uniqueFileName = Guid.NewGuid() + "_" + safeFileName;

                // Ühendame üleslaadimise kausta ja unikaalse failinime.
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // FileStream loob faili ning CopyTo kopeerib IFormFile sisu kettale.
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyTo(fileStream);
                }

                // Loome andmebaasi jaoks faili ja kosmoselaeva vahelise seose.
                var path = new FileToApi
                {
                    Id = Guid.NewGuid(),
                    ExistingFilePath = uniqueFileName,
                    SpaceshipId = domain.Id
                };

                // Add on siin sünkroonne, seega ei jää AddAsync ilma await-ita pooleli.
                _context.FileToApis.Add(path);
            }

            // SaveChanges kutsutakse SpaceshipServices klassis üks kord kogu tehingu jaoks.
        }

        // Õpetaja RemoveImageFromApi meetod kustutab ainult ühe valitud pildi.
        public async Task<bool> RemoveImageFromApi(FileToApiDto dto)
        {
            // Nii pildi kui ka kosmoselaeva ID peavad olema määratud.
            if (dto.Id == Guid.Empty || !dto.SpaceshipId.HasValue
                || dto.SpaceshipId.Value == Guid.Empty)
            {
                return false;
            }

            // Mõlema ID kontroll takistab teise kosmoselaeva pildi kustutamist.
            var image = await _context.FileToApis.FirstOrDefaultAsync(file =>
                file.Id == dto.Id && file.SpaceshipId == dto.SpaceshipId.Value);

            // Kui pilt puudub või ei kuulu sellele kosmoselaevale, ei muudeta midagi.
            if (image == null)
            {
                return false;
            }

            // Usaldame ainult andmebaasist loetud failinime, mitte vormi failiteed.
            var fileName = image.ExistingFilePath;
            if (!string.IsNullOrWhiteSpace(fileName))
            {
                // Failinimi ei tohi viidata üleslaadimise kaustast väljapoole.
                if (Path.IsPathRooted(fileName) || fileName != Path.GetFileName(fileName)
                    || fileName.Contains('/') || fileName.Contains('\\')
                    || fileName == "." || fileName == "..")
                {
                    throw new InvalidOperationException("Pildi failinimi ei ole lubatud.");
                }

                var filePath = Path.Combine(GetUploadsFolder(), fileName);

                // Puuduva füüsilise faili korral saab allesjäänud kirje siiski eemaldada.
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }

            // Eemaldame ainult leitud pildi, mitte kosmoselaeva ega teisi pilte.
            _context.FileToApis.Remove(image);

            // Eraldi pildi kustutamine salvestab muudatuse kohe andmebaasi.
            await _context.SaveChangesAsync();
            return true;
        }

        // Meetod eemaldab ühe kosmoselaeva kõik failid ja FileToApi kirjed.
        public void DeleteFiles(Guid spaceshipId)
        {
            // Loeme kustutatava kosmoselaevaga seotud failikirjed.
            var fileRecords = _context.FileToApis
                .Where(file => file.SpaceshipId == spaceshipId)
                .ToList();

            foreach (var fileRecord in fileRecords)
            {
                // Tühja failinime korral ei saa füüsilist teed koostada.
                if (string.IsNullOrWhiteSpace(fileRecord.ExistingFilePath))
                {
                    continue;
                }

                // Koostame füüsilise faili täieliku tee.
                var filePath = Path.Combine(GetUploadsFolder(), fileRecord.ExistingFilePath);

                // Kustutame faili ainult siis, kui see kettal eksisteerib.
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }

            // Märgime kõik leitud FileToApi kirjed andmebaasist eemaldamiseks.
            _context.FileToApis.RemoveRange(fileRecords);

            // SaveChanges kutsutakse SpaceshipServices klassis koos kosmoselaeva kustutamisega.
        }

        // Abimeetod tagastab üleslaadimise kausta absoluutse tee.
        private string GetUploadsFolder()
        {
            return Path.Combine(_webHost.ContentRootPath, "wwwroot", UploadFolderName);
        }
    }
}
