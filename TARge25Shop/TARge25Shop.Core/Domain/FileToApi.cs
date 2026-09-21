namespace TARge25Shop.Core.Domain
{
    // Domain klass kirjeldab kettale salvestatud faili kirjet andmebaasis.
    public class FileToApi
    {
        // Id on faili andmebaasikirje unikaalne primaarvõti.
        public Guid Id { get; set; }

        // ExistingFilePath sisaldab kettale salvestatud unikaalset failinime.
        public string ExistingFilePath { get; set; } = string.Empty;

        // SpaceshipId seob faili selle kosmoselaevaga, mille juurde fail kuulub.
        public Guid SpaceshipId { get; set; }
    }
}
