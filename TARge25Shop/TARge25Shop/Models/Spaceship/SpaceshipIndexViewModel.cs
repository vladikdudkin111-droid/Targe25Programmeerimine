namespace TARge25Shop.Models.Spaceship
{
    // ViewModel sisaldab Index tabeli ühe rea andmeid.
    public class SpaceshipIndexViewModel
    {
        // ID-d kasutavad Details, CreateUpdate ja Delete nupud õige kirje leidmiseks.
        public Guid Id { get; set; }

        // Kosmoselaeva põhiandmed.
        public string Name { get; set; } = string.Empty;
        public string ShipType { get; set; } = string.Empty;
        public int Crew { get; set; }
        public int EnginePower { get; set; }
        public int FileCount { get; set; }

        // Kirje loomise ja viimase muutmise aeg.
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
