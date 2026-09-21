using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using TARge25Shop.Core.Domain;
using TARge25Shop.Core.Dto;
using TARge25Shop.Core.ServiceInterface;
using TARge25Shop.Data;

namespace TARge25Shop.ApplicationServices.Services
{
    // FileServices haldab failide salvestamist, lugemist ja kustutamist.
    public class FileServices : IFileServices
    {
        // Konstanti kasutame selleks, et üleslaadimise kausta nimi oleks kogu klassis sama.
        private const string UploadFolderName = "multipleFileUpload";

        // IWebHostEnvironment annab rakenduse juurkausta füüsilise asukoha.
        private readonly IWebHostEnvironment _webHost;

        // DbContext annab ligipääsu FileToApis andmebaasitabelile.
        private readonly TARge25ShopContext _context;

        // Konstruktor saab vajalikud sõltuvused dependency injection konteinerist.
        public FileServices(IWebHostEnvironment webHost, TARge25ShopContext context)
        {
            _webHost = webHost;
            _context = context;
        }

        // Meetod salvestab vormilt saadud failid kettale ja nende andmed andmebaasi.
        public void FilesToApi(SpaceshipDto dto, Spaceship domain)
        {
            // Kontrollime, kas kasutaja valis vähemalt ühe faili.
            if (dto.Files != null && dto.Files.Count > 0)
            {
                // Koostame absoluutse tee kausta wwwroot\multipleFileUpload.
                string uploadsFolder = Path.Combine(
                    _webHost.ContentRootPath,
                    "wwwroot",
                    UploadFolderName);

                // Kui üleslaadimise kausta ei ole olemas, loome selle automaatselt.
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Töötleme kõik kasutaja valitud mittetühjad failid ükshaaval.
                foreach (var file in dto.Files.Where(file => file.Length > 0))
                {
                    // Path.GetFileName eemaldab võimalikud kaustanimed ja kaitseb faili teekonda.
                    var safeFileName = Path.GetFileName(file.FileName);

                    // Guid muudab failinime unikaalseks, et sama nimega faile üle ei kirjutataks.
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + safeFileName;

                    // Ühendame üleslaadimise kausta ja unikaalse failinime üheks füüsiliseks teeks.
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    // FileStream loob kettale uue faili; using sulgeb voo automaatselt.
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        // CopyTo kopeerib IFormFile sisu loodud faili täpselt nagu õpetaja näites.
                        file.CopyTo(fileStream);
                    }

                    // Loome andmebaasi jaoks FileToApi Domain objekti.
                    FileToApi path = new FileToApi
                    {
                        // Iga failikirje saab oma unikaalse ID.
                        Id = Guid.NewGuid(),

                        // Andmebaasi salvestame kettal oleva unikaalse failinime.
                        ExistingFilePath = uniqueFileName,

                        // SpaceshipId seob faili parajasti loodava või muudetava kosmoselaevaga.
                        SpaceshipId = domain.Id
                    };

                    // Lisame failikirje DbContexti; tegelik INSERT tehakse SaveChanges ajal.
                    _context.FileToApis.Add(path);
                }

                // Salvestame kõik uued FileToApi kirjed ühe andmebaasipäringuga.
                _context.SaveChanges();
            }
        }

        // Meetod tagastab ühe kosmoselaevaga seotud ja kettal olemasolevad failid.
        public IReadOnlyList<FileToApiDto> FilesFromApi(Guid spaceshipId)
        {
            // AsNoTracking sobib lugemiseks, sest me ei muuda saadud kirjeid.
            return _context.FileToApis
                .AsNoTracking()
                .Where(file => file.SpaceshipId == spaceshipId)
                .OrderBy(file => file.ExistingFilePath)
                .ToList()
                // Teisendame andmebaasikirjed veebikihile sobivateks DTO-deks.
                .Select(ToDto)
                // Kui fail on kettalt käsitsi kustutatud, jätame puuduva faili nimekirjast välja.
                .OfType<FileToApiDto>()
                .OrderBy(file => file.FileName)
                .ToList();
        }

        // Meetod otsib ühe kindla faili kosmoselaeva ID ja salvestatud nime järgi.
        public FileToApiDto? FileFromApi(Guid spaceshipId, string storedFileName)
        {
            // Lubame ainult failinime, mitte kasutaja saadetud kaustateed.
            var safeName = Path.GetFileName(storedFileName);
            if (!string.Equals(safeName, storedFileName, StringComparison.Ordinal))
            {
                return null;
            }

            // Kontrollime andmebaasist, et fail kuulub tõesti etteantud kosmoselaevale.
            var fileRecord = _context.FileToApis
                .AsNoTracking()
                .FirstOrDefault(file =>
                    file.SpaceshipId == spaceshipId &&
                    file.ExistingFilePath == safeName);

            // Tagastame null, kui andmebaasikirjet või füüsilist faili ei leitud.
            return fileRecord == null ? null : ToDto(fileRecord);
        }

        // Meetod kustutab kasutaja valitud failid nii kettalt kui ka andmebaasist.
        public void DeleteFilesFromApi(Guid spaceshipId, IEnumerable<string> storedFileNames)
        {
            // Puhastame failinimed ja eemaldame nimekirjast kordused.
            var safeNames = storedFileNames
                .Select(Path.GetFileName)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Distinct()
                .ToList();

            // Otsime ainult sellele kosmoselaevale kuuluvad valitud failikirjed.
            var fileRecords = _context.FileToApis
                .Where(file =>
                    file.SpaceshipId == spaceshipId &&
                    safeNames.Contains(file.ExistingFilePath))
                .ToList();

            foreach (var fileRecord in fileRecords)
            {
                // Koostame kustutatava faili täieliku füüsilise tee.
                var filePath = Path.Combine(GetUploadDirectory(), fileRecord.ExistingFilePath);

                // Kustutame füüsilise faili ainult siis, kui see kettal eksisteerib.
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }

            // Eemaldame samade failide kirjed andmebaasist.
            _context.FileToApis.RemoveRange(fileRecords);
            _context.SaveChanges();
        }

        // Meetod kustutab kõik ühe kosmoselaevaga seotud failid ja andmebaasikirjed.
        public void DeleteDirectoryFromApi(Guid spaceshipId)
        {
            // Loeme andmebaasist kõik kustutatava kosmoselaeva failid.
            var fileRecords = _context.FileToApis
                .Where(file => file.SpaceshipId == spaceshipId)
                .ToList();

            foreach (var fileRecord in fileRecords)
            {
                // Failid asuvad ühises multipleFileUpload kaustas ja neil on unikaalsed nimed.
                var filePath = Path.Combine(GetUploadDirectory(), fileRecord.ExistingFilePath);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }

            // Pärast füüsiliste failide kustutamist eemaldame ka nende andmebaasikirjed.
            _context.FileToApis.RemoveRange(fileRecords);
            _context.SaveChanges();
        }

        // Abimeetod tagastab üleslaadimise kausta absoluutse füüsilise tee.
        private string GetUploadDirectory()
        {
            return Path.Combine(
                _webHost.ContentRootPath,
                "wwwroot",
                UploadFolderName);
        }

        // Abimeetod teisendab FileToApi andmebaasikirje FileToApiDto objektiks.
        private FileToApiDto? ToDto(FileToApi fileRecord)
        {
            // Koostame andmebaasis oleva failinime põhjal faili täieliku tee.
            var filePath = Path.Combine(GetUploadDirectory(), fileRecord.ExistingFilePath);

            // Kui füüsiline fail puudub, ei saa selle metaandmeid tagastada.
            if (!File.Exists(filePath))
            {
                return null;
            }

            // FileInfo annab faili suuruse ja loomise aja.
            var file = new FileInfo(filePath);

            // Eraldame Guid prefiksi algsest failinimest esimese alakriipsu järgi.
            var separatorIndex = file.Name.IndexOf('_');
            var originalName = separatorIndex >= 0
                ? file.Name[(separatorIndex + 1)..]
                : file.Name;

            // DTO sisaldab kõiki andmeid, mida Controller ja View faili kuvamiseks vajavad.
            return new FileToApiDto
            {
                FileName = originalName,
                StoredFileName = file.Name,
                RelativePath = $"/{UploadFolderName}/{file.Name}",
                FilePath = file.FullName,
                FileSize = file.Length,
                CreatedAt = file.CreationTime
            };
        }
    }
}
