namespace TARge25Shop.Models.Spaceship
{
    // ViewModel sisaldab kustutamise kinnitamise lehel kuvatavaid andmeid.
    public class SpaceshipDeleteViewModel
    {
        // Id määrab kustutatava kosmoselaeva.
        public Guid? Id { get; set; }

        // Kosmoselaeva põhiandmed.
        public string Name { get; set; } = string.Empty;
        public string ShipType { get; set; } = string.Empty;
        public int Crew { get; set; }
        public int EnginePower { get; set; }

        // Image sisaldab kustutatava kosmoselaevaga seotud pilte.
        public List<ImageViewModel> Image { get; set; } = new();

        // Kuupäevad näitavad kirje loomise ja viimase muutmise aega.
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
