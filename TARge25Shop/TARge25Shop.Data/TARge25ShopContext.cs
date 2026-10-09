using Microsoft.EntityFrameworkCore;
using TARge25Shop.Core.Domain;

namespace TARge25Shop.Data
{
    // DbContext ühendab meie C# mudelid andmebaasiga.
    public class TARge25ShopContext : DbContext
    {
        // Constructor saab andmebaasi seadistused Program.cs failist.
        public TARge25ShopContext(DbContextOptions<TARge25ShopContext> options)
            : base(options)
        {
        }

        // DbSet esindab andmebaasis Spaceships tabelit.
        public DbSet<Spaceship> Spaceships { get; set; } = null!;

        // DbSet esindab andmebaasis Kindergartens tabelit.
        public DbSet<Kindergarten> Kindergartens { get; set; } = null!;

        public DbSet<KindergartenImage> KindergartenImages { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<KindergartenImage>(image =>
            {
                image.Property(x => x.FileName).HasMaxLength(255);
                image.Property(x => x.ContentType).HasMaxLength(100);
                image.Property(x => x.Data).HasColumnType("varbinary(max)");

                // Ankeedi kustutamisel kustutab andmebaas ka kõik selle pildid.
                image.HasOne(x => x.Kindergarten)
                    .WithMany(x => x.Images)
                    .HasForeignKey(x => x.KindergartenId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
