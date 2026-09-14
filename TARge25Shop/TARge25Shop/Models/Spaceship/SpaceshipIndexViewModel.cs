namespace TARge25Shop.Models.Spaceship
{
    // ViewModel, mida kasutame nimekirjas, detailides ja kustutamise kinnitamisel.
    public class SpaceshipIndexViewModel
    {
        // ID-d kasutavad Details, Update ja Delete nupud õige kirje leidmiseks.
        public Guid Id { get; set; }

        // Kosmoselaeva põhiandmed.
        public string Name { get; set; } = string.Empty;
        public string ShipType { get; set; } = string.Empty;
        public int Crew { get; set; }
        public int EnginePower { get; set; }

        // Kirje loomise ja viimase muutmise aeg.
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
