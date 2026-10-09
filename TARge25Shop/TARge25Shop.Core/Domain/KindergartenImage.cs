namespace TARge25Shop.Core.Domain
{
    // Pildi sisu ja metaandmed salvestatakse koos andmebaasi.
    public class KindergartenImage
    {
        public Guid Id { get; set; }
        public Guid KindergartenId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public byte[] Data { get; set; } = [];
        public DateTime CreatedAt { get; set; }
        public Kindergarten Kindergarten { get; set; } = null!;
    }
}
