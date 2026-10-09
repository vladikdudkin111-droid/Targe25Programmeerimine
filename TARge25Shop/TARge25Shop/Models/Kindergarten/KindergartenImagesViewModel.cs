namespace TARge25Shop.Models.Kindergarten
{
    public class KindergartenImagesViewModel
    {
        public Guid KindergartenId { get; set; }
        public bool AllowDelete { get; set; }
        public List<KindergartenImageViewModel> Images { get; set; } = [];
    }

    public class KindergartenImageViewModel
    {
        public Guid Id { get; set; }
        public string FileName { get; set; } = string.Empty;
    }
}
