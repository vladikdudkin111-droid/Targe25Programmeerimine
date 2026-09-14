using System.ComponentModel.DataAnnotations;

namespace TARge25Shop.Models.Spaceship
{
    // ViewModel sisaldab välju, mida kasutaja täidab uue kosmoselaeva loomise vormis.
    public class SpaceshipCreateViewModel
    {
        // Nimi on kohustuslik ja maksimaalselt 100 märki pikk.
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        // Kosmoselaeva tüüp on samuti kohustuslik.
        [Required]
        [Display(Name = "Ship type")]
        [StringLength(100)]
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
