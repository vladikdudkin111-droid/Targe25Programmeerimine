namespace TARge25Shop.Core.Dto
{
    // Veebirakenduse wwwroot kaustas oleva faili andmed.
    public class FileToApiDto
    {
        // FileName on kasutajale kuvatav algne failinimi ilma Guid prefiksita.
        public string FileName { get; set; } = string.Empty;

        // StoredFileName on kettal ja andmebaasis olev unikaalne failinimi.
        public string StoredFileName { get; set; } = string.Empty;

        // RelativePath on brauseris kasutatav suhteline URL failini.
        public string RelativePath { get; set; } = string.Empty;

        // FilePath on serveri kettal olev faili täielik füüsiline tee.
        public string FilePath { get; set; } = string.Empty;

        // FileSize näitab faili suurust baitides.
        public long FileSize { get; set; }

        // CreatedAt näitab, millal füüsiline fail kettale loodi.
        public DateTime CreatedAt { get; set; }
    }
}
