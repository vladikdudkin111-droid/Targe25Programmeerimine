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

        // Loomise ja viimase muutmise aeg.
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
