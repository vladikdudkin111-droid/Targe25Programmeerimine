namespace TARge25Shop.Models.Spaceship
{
    // ViewModel sisaldab kustutamise kinnitamise lehel kuvatavaid andmeid.
    public class SpaceshipDeleteViewModel
    {
        // Id saadetakse POST päringuga Controllerisse õige kirje kustutamiseks.
        public Guid Id { get; set; }

        // Põhiandmed aitavad kasutajal enne kustutamist õige kirje üle kontrollida.
        public string Name { get; set; } = string.Empty;
        public string ShipType { get; set; } = string.Empty;
        public int Crew { get; set; }
        public int EnginePower { get; set; }

        // ImageCount näitab kosmoselaevaga seotud failide arvu.
        public int ImageCount { get; set; }
    }
}
