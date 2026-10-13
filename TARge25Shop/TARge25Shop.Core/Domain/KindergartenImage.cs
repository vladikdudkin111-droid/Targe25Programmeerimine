namespace TARge25Shop.Core.Domain
{
    // Pildi sisu ja metaandmed salvestatakse koos andmebaasi.
    public class KindergartenImage
    {
        public Guid Id { get; set; }
        // Pildi unikaalne primaarvõti.
        public Guid KindergartenId { get; set; }
        // Välisvõti, mis näitab, millisele ankeedile pilt kuulub.
        public string FileName { get; set; } = string.Empty;
        // Pildi nimi galeriis kuvamiseks.
        public string ContentType { get; set; } = string.Empty;
        // MIME tüüp, näiteks image/png, mille järgi brauser käsitleb vastust pildina.
        public byte[] Data { get; set; } = [];
        // Pildi tegelik sisu baitide massiivina; see salvestatakse andmebaasi.
        public DateTime CreatedAt { get; set; }
        // Pildi lisamise aeg.
        public Kindergarten Kindergarten { get; set; } = null!;
        // Navigatsiooniomadus seotud ankeedile; Include võimaldab selle päringuga kaasa laadida.
    }
}
