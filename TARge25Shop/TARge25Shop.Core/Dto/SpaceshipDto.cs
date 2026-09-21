using Microsoft.AspNetCore.Http;

namespace TARge25Shop.Core.Dto
{
    // DTO-d kasutame andmete liigutamiseks Controlleri ja Service kihi vahel.
    public class SpaceshipDto
    {
        // Kosmoselaeva unikaalne ID. Update meetod vajab seda olemasoleva kirje leidmiseks.
        public Guid Id { get; set; }

        // Kosmoselaeva põhiandmed.
        public string Name { get; set; } = string.Empty;
        public string ShipType { get; set; } = string.Empty;
        public int Crew { get; set; }
        public int EnginePower { get; set; }

        // IFormFile objektid sisaldavad veebivormilt üles laaditud failide sisu ja metaandmeid.
        public List<IFormFile> Files { get; set; } = new();

        // Nimekiri ütleb, millised varem salvestatud failid tuleb muutmisel kustutada.
        public List<string> FileNamesToDelete { get; set; } = new();

        // Loomise ja viimase muutmise aeg.
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
