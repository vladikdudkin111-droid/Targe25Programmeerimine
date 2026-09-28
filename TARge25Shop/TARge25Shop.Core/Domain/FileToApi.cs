namespace TARge25Shop.Core.Domain
{
    // Domain klass kirjeldab andmebaasis ühe üleslaaditud faili kirjet.
    public class FileToApi
    {
        // Id on failikirje unikaalne primaarvõti.
        public Guid Id { get; set; }

        // ExistingFilePath sisaldab wwwroot kausta salvestatud unikaalset failinime.
        public string? ExistingFilePath { get; set; }

        // SpaceshipId seob faili vastava kosmoselaevaga.
        public Guid? SpaceshipId { get; set; }
    }
}
