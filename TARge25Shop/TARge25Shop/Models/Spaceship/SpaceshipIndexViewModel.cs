namespace TARge25Shop.Models.Spaceship
{
    // ViewModel sisaldab Index tabeli ühe rea andmeid.
    public class SpaceshipIndexViewModel
    {
        // Id-d kasutavad Details, Update ja Delete lingid.
        public Guid? Id { get; set; }

        // Kosmoselaeva põhiandmed.
        public string Name { get; set; } = string.Empty;
        public string ShipType { get; set; } = string.Empty;
        public int Crew { get; set; }
        public int EnginePower { get; set; }

        // Kuupäevad näitavad kirje loomise ja viimase muutmise aega.
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
