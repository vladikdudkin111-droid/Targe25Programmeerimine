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

        public FileService(TARge25ShopContext context)
        {
            _context = context;
        }

        public List<KindergartenImage> PrepareImages(IReadOnlyCollection<FileUploadDto> files)
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
            foreach (var file in files)
            {
                if (file.Data.Length == 0 || file.Data.Length > FileUploadDto.MaxFileSize)
                {
                    throw new ValidationException("Pilt ei tohi olla tühi ega suurem kui 5 MB.");
                }

                // Failinimi on ainult kuvamiseks; eemaldame sellest kataloogi tee.
                var fileName = Path.GetFileName(file.FileName.Replace('\\', '/'));
                if (string.IsNullOrWhiteSpace(fileName) || fileName.Length > 255)
                {
                    throw new ValidationException("Pildi failinimi peab olema 1–255 märki pikk.");
                }

                var contentType = GetContentType(fileName, file.Data);
                images.Add(new KindergartenImage
                {
                    Id = Guid.NewGuid(),
                    FileName = fileName,
                    ContentType = contentType,
                    Data = file.Data,
                    CreatedAt = DateTime.Now
                });
            }

            // Kõik failid kontrollitakse enne ankeedi või piltide salvestamist.
            return images;
        }

        public Task<List<KindergartenImageDto>> GetImagesAsync(Guid kindergartenId)
        {
            return _context.KindergartenImages.AsNoTracking()
                .Where(x => x.KindergartenId == kindergartenId)
                .OrderBy(x => x.CreatedAt).ThenBy(x => x.Id)
                .Select(x => new KindergartenImageDto { Id = x.Id, FileName = x.FileName })
                .ToListAsync();
        }

        public Task<KindergartenImage?> GetImageAsync(Guid kindergartenId, Guid imageId)
        {
            return _context.KindergartenImages.AsNoTracking()
                .SingleOrDefaultAsync(x => x.Id == imageId && x.KindergartenId == kindergartenId);
        }

        public async Task<bool> DeleteImageAsync(Guid kindergartenId, Guid imageId)
        {
            // Kontrollime, et pilt kuulub just aadressis määratud ankeedile.
            var image = await _context.KindergartenImages.Include(x => x.Kindergarten)
                .SingleOrDefaultAsync(x => x.Id == imageId && x.KindergartenId == kindergartenId);
            if (image == null)
            {
                return false;
            }

            image.Kindergarten.UpdatedAt = DateTime.Now;
            _context.KindergartenImages.Remove(image);
            await _context.SaveChangesAsync();
            return true;
        }

        private static string GetContentType(string fileName, byte[] data)
        {
            // Kontrollime nii laiendit kui ka faili signatuuri, mitte brauseri MIME tüüpi.
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            ReadOnlySpan<byte> content = data;
            if (extension == ".png" && content.StartsWith(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }))
                return "image/png";
            if ((extension == ".jpg" || extension == ".jpeg") && content.StartsWith(new byte[] { 255, 216, 255 }))
                return "image/jpeg";
            if (extension == ".gif" && (content.StartsWith("GIF87a"u8) || content.StartsWith("GIF89a"u8)))
                return "image/gif";
            if (extension == ".webp" && content.Length >= 12 && content.StartsWith("RIFF"u8) && content.Slice(8, 4).SequenceEqual("WEBP"u8))
                return "image/webp";

            throw new ValidationException("Lubatud on ainult JPG, PNG, GIF ja WebP pildifailid. Faili sisu peab vastama laiendile.");
        }
    }
}
