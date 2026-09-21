namespace TARge25Shop.Models.Spaceship
{
    // ViewModel sisaldab vaates ühe üleslaaditud faili kohta kuvatavaid andmeid.
    public class ImageViewModel
    {
        // SpaceshipId näitab, millise kosmoselaeva juurde fail kuulub.
        public Guid SpaceshipId { get; set; }

        // FilePath sisaldab wwwroot/multipleFileUpload kaustas olevat failinime.
        public string? FilePath { get; set; }

        // FileName on kasutajale kuvatav algne failinimi.
        public string FileName { get; set; } = string.Empty;

        // StoredFileName on kettal olev Guid prefiksiga unikaalne failinimi.
        public string StoredFileName { get; set; } = string.Empty;

        // RelativePath võimaldab faili veebiaadressi kaudu avada.
        public string RelativePath { get; set; } = string.Empty;

        // FileSize näitab faili suurust baitides.
        public long FileSize { get; set; }

        // CreatedAt näitab faili kettale salvestamise aega.
        public DateTime CreatedAt { get; set; }
    }
}
