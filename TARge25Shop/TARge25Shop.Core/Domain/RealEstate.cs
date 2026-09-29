namespace TARge25Shop.Core.Domain
{
    // Domain klass kirjeldab ühte andmebaasi salvestatavat kinnisvaraobjekti.
    public class RealEstate
    {
        // Id on kinnisvaraobjekti unikaalne primaarvõti.
        public Guid Id { get; set; }

        // Address sisaldab kinnisvara aadressi.
        public string Address { get; set; } = string.Empty;

        // Area näitab pindala ruutmeetrites; decimal säilitab sajandikud täpselt.
        public decimal Area { get; set; }

        // RoomCount näitab tubade arvu.
        public int RoomCount { get; set; }

        // Price näitab kinnisvara hinda eurodes.
        public decimal Price { get; set; }

        // Teenus määrab loomise aja ja uuendab viimase muutmise aega.
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
