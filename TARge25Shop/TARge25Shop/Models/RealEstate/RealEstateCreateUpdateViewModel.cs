using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace TARge25Shop.Models.RealEstate
{
    // Ühine mudel teenindab nii loomise kui ka muutmise vormi.
    public class RealEstateCreateUpdateViewModel
    {
        public Guid? Id { get; set; }
        // Vorm kontrollib põhiandmeid enne teenuse väljakutset.
        [Required(ErrorMessage = "Sisesta pindala.")]
        [Range(0.01, 10000000, ErrorMessage = "Pindala peab olema positiivne arv kuni 10 000 000.")]
        public double? Area { get; set; }
        [Required(ErrorMessage = "Sisesta asukoht.")]
        [StringLength(250)]
        public string Location { get; set; } = string.Empty;
        [Range(0, 1000)]
        public int RoomNumber { get; set; }
        [Required(ErrorMessage = "Sisesta hoone liik.")]
        [StringLength(100)]
        public string BuildingType { get; set; } = string.Empty;

        // Tühi faililoend tähendab, et olemasolevaid pilte ei muudeta.
        public List<IFormFile> Files { get; set; } = new();
        // Galerii loetakse serveris, mitte kasutaja POST-andmetest.
        [BindNever, ValidateNever]
        public List<RealEstateImageViewModel> Image { get; set; } = new();
        [BindNever]
        public DateTime? CreatedAt { get; set; }
        [BindNever]
        public DateTime? ModifiedAt { get; set; }
    }
}
