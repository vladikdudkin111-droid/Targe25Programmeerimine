namespace TARge25Shop.Models.Kindergarten
{
    // Vaatele vajalikud lasteaiarühma andmed.
    public class KindergartenDetailsViewModel
    {
        public Guid Id { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public int ChildrenCount { get; set; }
        public string KindergartenName { get; set; } = string.Empty;
        public string TeacherName { get; set; } = string.Empty;

        // Loomise ja viimase muutmise aja määrab teenus.
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
