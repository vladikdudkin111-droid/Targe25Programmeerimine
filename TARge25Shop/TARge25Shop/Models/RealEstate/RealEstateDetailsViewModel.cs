namespace TARge25Shop.Models.RealEstate
{
    public class RealEstateDetailsViewModel
    {
        public Guid? Id { get; set; }
        public double? Area { get; set; }
        public string Location { get; set; } = string.Empty;
        public int RoomNumber { get; set; }
        public string BuildingType { get; set; } = string.Empty;

        // Seotud pildid kuvatakse selle objekti galeriis.
        public List<RealEstateImageViewModel> Image { get; set; } = new();

        public DateTime? CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
    }
}
