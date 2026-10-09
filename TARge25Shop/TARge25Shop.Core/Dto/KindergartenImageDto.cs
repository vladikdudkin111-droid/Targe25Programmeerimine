namespace TARge25Shop.Core.Dto
{
    // Galerii jaoks ei ole vaja kõigi piltide sisu korraga laadida.
    public class KindergartenImageDto
    {
        public Guid Id { get; set; }
        public string FileName { get; set; } = string.Empty;
    }
}
