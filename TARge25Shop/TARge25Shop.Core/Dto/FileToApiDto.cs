namespace TARge25Shop.Core.Dto
{
    // DTO liigutab failikirje andmeid rakenduse kihtide vahel.
    public class FileToApiDto
    {
        // Id on failikirje unikaalne tunnus.
        public Guid Id { get; set; }

        // ExistingFilePath on kettale salvestatud unikaalne failinimi.
        public string? ExistingFilePath { get; set; }

        // SpaceshipId näitab, millise kosmoselaeva juurde fail kuulub.
        public Guid? SpaceshipId { get; set; }
    }
}
