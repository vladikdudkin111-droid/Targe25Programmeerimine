namespace TARge25Shop.Core.Domain
{
    // Domain klass kirjeldab andmebaasi salvestatavat kosmoselaeva.
    public class Spaceship
    {
        // Id on kosmoselaeva unikaalne primaarvõti.
        public Guid? Id { get; set; }

        // Järgmised väljad sisaldavad kosmoselaeva põhiandmeid.
        public string Name { get; set; } = string.Empty;
        public string ShipType { get; set; } = string.Empty;
        public int Crew { get; set; }
        public int EnginePower { get; set; }

        // Kuupäevad näitavad kirje loomise ja viimase muutmise aega.
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
