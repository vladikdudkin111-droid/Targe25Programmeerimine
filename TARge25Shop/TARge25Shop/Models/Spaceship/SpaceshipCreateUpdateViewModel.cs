using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace TARge25Shop.Models.Spaceship
{
    // Ühine mudel teenindab nii loomise kui ka muutmise vormi.
    public class SpaceshipCreateUpdateViewModel
    {
        public Guid? Id { get; set; }
        // Nimi ja tüüp on kohustuslikud; arvulised väärtused ei tohi olla negatiivsed.
        [Required(ErrorMessage = "Sisesta nimi.")]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;
        [Required(ErrorMessage = "Sisesta laeva tüüp.")]
        [StringLength(200)]
        public string ShipType { get; set; } = string.Empty;
        [Range(0, 1000000)]
        public int Crew { get; set; }
        [Range(0, int.MaxValue)]
        public int EnginePower { get; set; }

        // Tühi faililoend tähendab, et olemasolevaid pilte ei muudeta.
        public List<IFormFile> Files { get; set; } = new();
        // Galerii loetakse serveris, mitte kasutaja POST-andmetest.
        [BindNever, ValidateNever]
        public List<ImageViewModel> Image { get; set; } = new();
        [BindNever]
        public DateTime CreatedAt { get; set; }
        [BindNever]
        public DateTime UpdatedAt { get; set; }
    }
}
