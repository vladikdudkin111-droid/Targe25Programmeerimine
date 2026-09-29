using System.ComponentModel.DataAnnotations;

namespace TARge25Shop.Models.RealEstate
{
    // Nagu Spaceshipil, kasutavad Create ja Update ühist vormi ning ViewModelit.
    public class RealEstateCreateUpdateViewModel
    {
        // Tühi Id tähendab uue objekti loomist, olemasolev Id selle muutmist.
        public Guid? Id { get; set; }

        // Aadress peab olema täidetud ja mahtuma andmebaasi 250 märgi sisse.
        [Required(ErrorMessage = "Sisesta aadress.")]
        [StringLength(250, ErrorMessage = "Aadress võib sisaldada kuni 250 märki.")]
        [Display(Name = "Address")]
        public string Address { get; set; } = string.Empty;

        // Pindala on positiivne kümnendarv ruutmeetrites.
        [Range(typeof(decimal), "0.01", "10000000", ParseLimitsInInvariantCulture = true,
            ErrorMessage = "Pindala peab olema 0,01 kuni 10 000 000 m².")]
        [Display(Name = "Area (m²)")]
        public decimal Area { get; set; }

        // Null tuba on lubatud näiteks krundi või avatud ruumi korral.
        [Range(0, 1000, ErrorMessage = "Tubade arv peab olema 0 kuni 1000.")]
        [Display(Name = "Rooms")]
        public int RoomCount { get; set; }

        // Hind säilitatakse decimal tüübina, et rahalisi väärtusi täpselt hoida.
        [Range(typeof(decimal), "0", "10000000000", ParseLimitsInInvariantCulture = true,
            ErrorMessage = "Hind peab olema 0 kuni 10 000 000 000 eurot.")]
        [Display(Name = "Price (€)")]
        public decimal Price { get; set; }
    }
}
