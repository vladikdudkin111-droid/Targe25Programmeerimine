namespace TARge25Shop.Core.Domain
{
    // Domain klass kirjeldab andmebaasi salvestatavat lasteaiarühma.
    public class Kindergarten
    {
        public Guid Id { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public int ChildrenCount { get; set; }
        public string KindergartenName { get; set; } = string.Empty;
        public string TeacherName { get; set; } = string.Empty;

        // Loomise ja viimase muutmise aja määrab teenus.
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public ICollection<KindergartenImage> Images { get; set; } = new List<KindergartenImage>();
    }
}
