namespace TARge25Shop.Models.Spaceship
{
    // ViewModel kirjeldab vaates kuvatavat ühte üleslaaditud pilti.
    public class ImageViewModel
    {
        // ImageId on FileToApi andmebaasikirje unikaalne tunnus.
        public Guid ImageId { get; set; }

        // FilePath sisaldab wwwroot/multipleFileUpload kaustas olevat failinime.
        public string? FilePath { get; set; }

        // SpaceshipId näitab, millise kosmoselaeva juurde pilt kuulub.
        public Guid? SpaceshipId { get; set; }
    }
}
