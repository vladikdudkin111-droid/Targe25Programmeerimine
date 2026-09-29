using System.ComponentModel.DataAnnotations;

namespace TARge25Shop.Models.RealEstate
{
    // Index tabeli ühe rea andmed; eraldi ViewModel hoiab Domain kihi vaatest lahus.
    public class RealEstateIndexViewModel
    {
        // Seda tunnust kasutavad Details, Update ja Delete lingid.
        public Guid Id { get; set; }

        // Kinnisvara põhiandmed, mis kuvatakse nimekirjas.
        [Display(Name = "Address")]
        public string Address { get; set; } = string.Empty;
        [Display(Name = "Area (m²)")]
        public decimal Area { get; set; }
        [Display(Name = "Rooms")]
        public int RoomCount { get; set; }
        [Display(Name = "Price (€)")]
        public decimal Price { get; set; }

        // Loomise ja muutmise ajad määrab teenus, mitte kasutaja vorm.
        [Display(Name = "Created")]
        public DateTime CreatedAt { get; set; }
        [Display(Name = "Updated")]
        public DateTime UpdatedAt { get; set; }
    }
}
