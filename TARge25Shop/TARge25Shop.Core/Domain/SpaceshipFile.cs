namespace TARge25Shop.Core.Domain
{
    // Andmebaasi salvestatav kosmoselaeva manus.
    public class SpaceshipFile
    {
        public Guid Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = "application/octet-stream";
        public byte[] Data { get; set; } = Array.Empty<byte>();
        public DateTime CreatedAt { get; set; }

        public Guid SpaceshipId { get; set; }
        public Spaceship Spaceship { get; set; } = null!;
    }
}
