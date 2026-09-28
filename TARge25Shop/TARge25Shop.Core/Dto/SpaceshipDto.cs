using Microsoft.AspNetCore.Http;

namespace TARge25Shop.Core.Dto
{
    // DTO liigutab kosmoselaeva ja failide andmeid Controlleri ning Service kihi vahel.
    public class SpaceshipDto
    {
        // Id on loomisel tühi ja muutmisel sisaldab olemasoleva kirje tunnust.
        public Guid? Id { get; set; }

        // Kosmoselaeva põhiandmed.
        public string Name { get; set; } = string.Empty;
        public string ShipType { get; set; } = string.Empty;
        public int Crew { get; set; }
        public int EnginePower { get; set; }

        // Files sisaldab vormilt saadud uusi üleslaaditavaid faile.
        public List<IFormFile> Files { get; set; } = new();

        // FileToApiDtos sisaldab olemasolevate failikirjete andmeid.
        public IEnumerable<FileToApiDto> FileToApiDtos { get; set; }
            = new List<FileToApiDto>();

        // Kuupäevad liiguvad muutmise vormi ja teenuse vahel.
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
