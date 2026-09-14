using System.ComponentModel.DataAnnotations;

namespace TARge25Shop.Models.Spaceship
{
    // ViewModel sisaldab välju, mida kasutaja saab Update vormis muuta.
    public class SpaceshipUpdateViewModel
    {
        // ID hoiab alles, millist kosmoselaeva me muudame.
        public Guid Id { get; set; }

        // Nimi peab olema täidetud.
        [Required]
        public string Name { get; set; } = string.Empty;

        // Kosmoselaeva tüüp peab olema täidetud.
        [Required]
        [Display(Name = "Ship type")]
        public string ShipType { get; set; } = string.Empty;

        // Meeskonna arv ei tohi olla negatiivne.
        [Range(0, int.MaxValue)]
        public int Crew { get; set; }

        // Mootori võimsus ei tohi olla negatiivne.
        [Display(Name = "Engine power")]
        [Range(0, int.MaxValue)]
        public int EnginePower { get; set; }
    }
}
