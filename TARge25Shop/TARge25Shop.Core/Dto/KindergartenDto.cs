namespace TARge25Shop.Core.Dto
{
    // DTO kannab andmeid Controlleri ja teenuse vahel.
    public class KindergartenDto
    {
        public Guid Id { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public int ChildrenCount { get; set; }
        public string KindergartenName { get; set; } = string.Empty;
        public string TeacherName { get; set; } = string.Empty;

        // Loomise ja viimase muutmise aja määrab teenus.
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public List<FileUploadDto> Files { get; set; } = [];
    }
}
