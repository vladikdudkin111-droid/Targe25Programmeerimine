using Microsoft.AspNetCore.Http;

namespace TARge25Shop.Core.Validation
{
    // Ühised piirangud hoiavad vormi ja failiteenuse kontrollid kooskõlas.
    public static class ImageUploadRules
    {
        public const int MaxFiles = 5;
        public const long MaxFileBytes = 5 * 1024 * 1024;

        // Lubame ainult brauseris kuvatavaid rasterpiltide vorminguid.
        public static string? ContentType(string? fileName) =>
            Path.GetExtension(fileName ?? string.Empty).ToLowerInvariant() switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                _ => null
            };

        // Tagastame esimese vea; null tähendab, et uued failid on lubatud.
        public static string? Validate(IReadOnlyCollection<IFormFile>? files)
        {
            if (files == null) return null;
            if (files.Count > MaxFiles) return "Korraga saab lisada kuni 5 pilti.";
            foreach (var file in files)
            {
                if (file.Length == 0) return "Tühja faili ei saa üles laadida.";
                if (file.Length > MaxFileBytes) return "Ühe pildi suurus võib olla kuni 5 MB.";
                if (ContentType(file.FileName) == null)
                    return "Lubatud faililaiendid on JPG, JPEG, PNG, GIF ja WEBP.";
            }
            return null;
        }
    }
}
