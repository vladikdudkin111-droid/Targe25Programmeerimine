using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace TARge25Shop.Models.Spaceship
{
    // Sama ViewModel teenindab õpetaja näite järgi nii Create kui ka Update vormi.
    public class SpaceshipCreateUpdateViewModel
    {
        // Tühi Id tähendab loomist ja olemasolev Id muutmist.
        public Guid? Id { get; set; }

        // Nimi ja kosmoselaeva tüüp on kohustuslikud.
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Ship type")]
        [StringLength(100)]
        public string ShipType { get; set; } = string.Empty;

        // Meeskonna arv ja mootori võimsus ei tohi olla negatiivsed.
        [Range(0, int.MaxValue)]
        public int Crew { get; set; }

        [Display(Name = "Engine power")]
        [Range(0, int.MaxValue)]
        public int EnginePower { get; set; }

        // Files sisaldab vormil valitud uusi üleslaaditavaid faile.
        public List<IFormFile> Files { get; set; } = new();

        // Image sisaldab kosmoselaevaga juba seotud pilte.
        public List<ImageViewModel> Image { get; set; } = new();

        // Kuupäevad säilitavad kirje loomise ja muutmise info vormi vahel.
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
