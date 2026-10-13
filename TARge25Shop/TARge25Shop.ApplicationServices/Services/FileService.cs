using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using TARge25Shop.Core.Domain;
using TARge25Shop.Core.Dto;
using TARge25Shop.Core.ServiceInterface;
using TARge25Shop.Data;

namespace TARge25Shop.ApplicationServices.Services
{
    public class FileService : IFileService
    {
        private readonly TARge25ShopContext _context;
        // DbContext võimaldab lugeda ja muuta andmebaasi pildikirjeid. readonly väli määratakse konstruktoris.

        public FileService(TARge25ShopContext context)
        {
            _context = context;
            // Dependency injection annab konstruktorile DbContexti; salvestame selle teenuse kasutamiseks.
        }

        public List<KindergartenImage> PrepareImages(IReadOnlyCollection<FileUploadDto> files)
        // Meetod kontrollib üleslaaditud faile ja tagastab salvestamiseks valmis pildiobjektid. Siin ei kutsuta SaveChangesAsync.
        {
            if (files.Count > FileUploadDto.MaxFileCount)
            {
                throw new ValidationException("Korraga saab lisada kuni 10 pilti.");
            }

            if (files.Sum(x => (long)x.Data.Length) > FileUploadDto.MaxTotalSize)
            {
                throw new ValidationException("Piltide kogumaht võib olla kuni 20 MB.");
            }

            var images = new List<KindergartenImage>();
            // Kogume kontrollitud failidest loodud pildiobjektid sellesse nimekirja.
            foreach (var file in files)
            // Kontrollime iga faili eraldi; ühe vigase faili korral meetod katkeb ja nimekirja ei tagastata.
            {
                if (file.Data.Length == 0 || file.Data.Length > FileUploadDto.MaxFileSize)
                {
                    throw new ValidationException("Pilt ei tohi olla tühi ega suurem kui 5 MB.");
                }

                // Failinimi on ainult kuvamiseks; eemaldame sellest kataloogi tee.
                var fileName = Path.GetFileName(file.FileName.Replace('\\', '/'));
                // Asendame Windowsi teeraldaja ja eemaldame kataloogiosa, et alles jääks ainult faili nimi.
                if (string.IsNullOrWhiteSpace(fileName) || fileName.Length > 255)
                {
                    throw new ValidationException("Pildi failinimi peab olema 1–255 märki pikk.");
                }

                var contentType = GetContentType(fileName, file.Data);
                // Määrame MIME tüübi laiendi ja algusbaitide järgi; vale vormingu korral tekib ValidationException.
                images.Add(new KindergartenImage
                {
                    Id = Guid.NewGuid(),
                    // Loome igale pildile eraldi unikaalse ID.
                    FileName = fileName,
                    // Salvestame puhastatud nime, mida kuvatakse galeriis.
                    ContentType = contentType,
                    // Salvestame kontrollitud MIME tüübi, mida kasutatakse pildi tagastamisel brauserile.
                    Data = file.Data,
                    // Salvestame pildi tegelikud baidid; see pole failitee ega ainult faili nimi.
                    CreatedAt = DateTime.Now
                    // Määrame pildi lisamise aja serveris.
                });
            }

            // Kõik failid kontrollitakse enne ankeedi või piltide salvestamist.
            return images;
            // Tagastame valmis objektid KindergartenServices teenusele, mis salvestab need koos ankeedi muudatustega.
        }

        public Task<List<KindergartenImageDto>> GetImagesAsync(Guid kindergartenId)
        // Tagastab galerii jaoks piltide ID-d ja nimed; suuri Data massiive selle päringuga ei laadita.
        {
            return _context.KindergartenImages.AsNoTracking()
            // AsNoTracking jätab muudatuste jälgimise välja, sest see päring ainult loeb andmeid.
                .Where(x => x.KindergartenId == kindergartenId)
                // Valime ainult selle ankeedi pildid.
                .OrderBy(x => x.CreatedAt).ThenBy(x => x.Id)
                // Järjestame lisamise aja järgi ja võrdse aja korral ID järgi.
                .Select(x => new KindergartenImageDto { Id = x.Id, FileName = x.FileName })
                // Projektsioon võtab andmebaasist ainult galerii jaoks vajalikud väljad.
                .ToListAsync();
                // Käivitame päringu asünkroonselt. Kutsuja ootab Taski valmimist await abil.
        }

        public Task<KindergartenImage?> GetImageAsync(Guid kindergartenId, Guid imageId)
        // Loeb ühe pildi koos baitidega. Küsimärk tähendab, et tulemus võib olla null.
        {
            return _context.KindergartenImages.AsNoTracking()
            // AsNoTracking jätab muudatuste jälgimise välja, sest see päring ainult loeb andmeid.
                .SingleOrDefaultAsync(x => x.Id == imageId && x.KindergartenId == kindergartenId);
                // Kontrollime korraga pildi ID-d ja omanikankeedi ID-d. Puuduva vaste korral tagastatakse null.
        }

        public async Task<bool> DeleteImageAsync(Guid kindergartenId, Guid imageId)
        // Kustutab ühe pildi; true tähendab edukat kustutamist ja false seda, et sobivat pilti ei leitud.
        {
            // Kontrollime, et pilt kuulub just aadressis määratud ankeedile.
            var image = await _context.KindergartenImages.Include(x => x.Kindergarten)
            // Include laadib ka seotud ankeedi, et saaksime selle UpdatedAt väärtust muuta.
                .SingleOrDefaultAsync(x => x.Id == imageId && x.KindergartenId == kindergartenId);
                // Kontrollime korraga pildi ID-d ja omanikankeedi ID-d. Puuduva vaste korral tagastatakse null.
            if (image == null)
            {
                return false;
                // Sobivat pilti ei leitud; kontroller saab selle põhjal tagastada NotFound tulemuse.
            }

            image.Kindergarten.UpdatedAt = DateTime.Now;
            // Pildi eemaldamine on samuti ankeedi muudatus, seetõttu uuendame muutmise aega.
            _context.KindergartenImages.Remove(image);
            // Märgime ainult selle pildikirje kustutamiseks; ankeet ja teised pildid jäävad alles.
            await _context.SaveChangesAsync();
            // Salvestame pildi kustutamise ja ankeedi muutmise aja ühe SaveChangesAsync kutsega.
            return true;
            // Kustutamine on salvestatud; kontroller suunab kasutaja tagasi muutmisvormile.
        }

        private static string GetContentType(string fileName, byte[] data)
        // Abimeetod tunneb lubatud vormingu ära laiendi ja signatuuri järgi; see ei dekodeeri kogu pilti.
        {
            // Kontrollime nii laiendit kui ka faili signatuuri, mitte brauseri MIME tüüpi.
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            // Võtame faililaiendi ja muudame selle väiketähtedeks, et näiteks .PNG ja .png oleksid võrdsed.
            ReadOnlySpan<byte> content = data;
            // Loeme algusbaite olemasolevast massiivist ilma uut koopiat tegemata.
            if (extension == ".png" && content.StartsWith(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }))
                return "image/png";
                // PNG korral kontrollitakse kaheksabaidist PNG signatuuri ja .png laiendit.
            if ((extension == ".jpg" || extension == ".jpeg") && content.StartsWith(new byte[] { 255, 216, 255 }))
                return "image/jpeg";
                // JPEG korral peab laiend olema .jpg või .jpeg ning algusbaidid FF D8 FF.
            if (extension == ".gif" && (content.StartsWith("GIF87a"u8) || content.StartsWith("GIF89a"u8)))
                return "image/gif";
                // GIF korral peab fail algama tekstiga GIF87a või GIF89a.
            if (extension == ".webp" && content.Length >= 12 && content.StartsWith("RIFF"u8) && content.Slice(8, 4).SequenceEqual("WEBP"u8))
                return "image/webp";
                // WebP korral kontrollitakse RIFF algust ja WEBP märgist baitidel 8 kuni 11.

            throw new ValidationException("Lubatud on ainult JPG, PNG, GIF ja WebP pildifailid. Faili sisu peab vastama laiendile.");
        }
    }
}
