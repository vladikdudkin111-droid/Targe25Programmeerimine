using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace TARge25Shop.Models.Spaceship
{
    // Sama ViewModel teenindab nii kosmoselaeva loomise kui ka muutmise vormi.
    public class SpaceshipCreateUpdateViewModel
    {
        // Tühi Id tähendab loomist; olemasolev Id tähendab kirje muutmist.
        public Guid? Id { get; set; }

        // Nimi on kohustuslik ja võib olla kuni 100 märki pikk.
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        // Kosmoselaeva tüüp on kohustuslik ja võib olla kuni 100 märki pikk.
        [Required]
        [Display(Name = "Ship type")]
        [StringLength(100)]
        public string ShipType { get; set; } = string.Empty;

        // Meeskonnaliikmete arv ei tohi olla negatiivne.
        [Range(0, int.MaxValue)]
        public int Crew { get; set; }

        // Mootori võimsus ei tohi olla negatiivne.
        [Display(Name = "Engine power")]
        [Range(0, int.MaxValue)]
        public int EnginePower { get; set; }

        // Files sisaldab vormil valitud uusi üleslaaditavaid faile.
        [Display(Name = "Files")]
        public List<IFormFile> Files { get; set; } = new();

        // ExistingImages sisaldab muutmisel juba salvestatud failide nimekirja.
        public List<ImageViewModel> ExistingImages { get; set; } = new();

        // FileNamesToDelete sisaldab kasutaja kustutamiseks märgitud failinimesid.
        public List<string> FileNamesToDelete { get; set; } = new();
    }
}
