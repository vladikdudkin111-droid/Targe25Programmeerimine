namespace TARge25Shop.Core.Dto
{
    // DTO viib vormi andmed Controllerist ApplicationServices kihti.
    public class RealEstateDto
    {
        // Loomisel on Id tühi; muutmisel sisaldab see olemasoleva kirje tunnust.
        public Guid? Id { get; set; }

        // Kasutaja muudetavad kinnisvara andmed.
        public string Address { get; set; } = string.Empty;
        public decimal Area { get; set; }
        public int RoomCount { get; set; }
        public decimal Price { get; set; }
    }
}
