using Microsoft.EntityFrameworkCore;
using TARge25Shop.Core.Domain;

namespace TARge25Shop.Data
{
    // DbContext ühendab C# Domain klassid SQL Serveri tabelitega.
    public class TARge25ShopContext : DbContext
    {
        // Konstruktor saab andmebaasi seadistused Program.cs failist.
        public TARge25ShopContext(DbContextOptions<TARge25ShopContext> options)
            : base(options)
        {
        }

        // DbSet esindab andmebaasis Spaceships tabelit.
        public DbSet<Spaceship> Spaceships { get; set; } = null!;

        // DbSet esindab andmebaasis FileToApis tabelit.
        public DbSet<FileToApi> FileToApis { get; set; } = null!;

        // DbSet esindab kinnisvaraobjektide tabelit.
        public DbSet<RealEstate> RealEstates { get; set; } = null!;

        // Määrame kinnisvara tabeli veergude omadused.
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Aadressi pikkus ja kümnendkohtade arv peavad vastama vormi piirangutele.
            modelBuilder.Entity<RealEstate>().Property(item => item.Address)
                .HasMaxLength(250).IsRequired();
            modelBuilder.Entity<RealEstate>().Property(item => item.Area).HasPrecision(18, 2);
            modelBuilder.Entity<RealEstate>().Property(item => item.Price).HasPrecision(18, 2);
        }
    }
}
