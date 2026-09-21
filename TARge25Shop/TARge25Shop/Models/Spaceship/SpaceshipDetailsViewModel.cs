namespace TARge25Shop.Models.Spaceship
{
    // ViewModel sisaldab Details lehel kuvatavaid kosmoselaeva andmeid.
    public class SpaceshipDetailsViewModel
    {
        // Id määrab, millise kosmoselaeva andmeid vaade kuvab.
        public Guid Id { get; set; }

        // Järgmised väljad kirjeldavad kosmoselaeva põhiandmeid.
        public string Name { get; set; } = string.Empty;
        public string ShipType { get; set; } = string.Empty;
        public int Crew { get; set; }
        public int EnginePower { get; set; }

        // Images sisaldab kosmoselaevaga seotud üleslaaditud faile.
        public List<ImageViewModel> Images { get; set; } = new();

        // Kuupäevad näitavad kirje loomise ja viimase muutmise aega.
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
