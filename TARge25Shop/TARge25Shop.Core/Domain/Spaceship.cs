namespace TARge25Shop.Core.Domain
{
    // Domain klass kirjeldab kosmoselaeva objekti, mida salvestatakse andmebaasi.
    public class Spaceship
    {
        // Primary key ehk kosmoselaeva unikaalne ID.
        public Guid Id { get; set; }

        // Kosmoselaeva põhiandmed.
        public string Name { get; set; } = string.Empty;
        public string ShipType { get; set; } = string.Empty;
        public int Crew { get; set; }
        public int EnginePower { get; set; }

        // Kuupäevad näitavad, millal kirje loodi ja viimati muudeti.
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
