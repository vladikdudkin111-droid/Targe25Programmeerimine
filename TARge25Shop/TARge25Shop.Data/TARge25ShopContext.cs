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

        // DbSet esindab andmebaasis FileToApis tabelit ja hoiab failide seoseid.
        public DbSet<FileToApi> FileToApis { get; set; } = null!;

        // Meetod kirjeldab mudeli lisaseadistused, mis peavad migratsiooni snapshotiga kattuma.
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Kutsume kõigepealt DbContext baasklassi vaikimisi mudeli seadistuse.
            base.OnModelCreating(modelBuilder);

            // Indeks vastab AddFileToApi migratsioonile ja kiirendab failide otsimist SpaceshipId järgi.
            modelBuilder.Entity<FileToApi>()
                .HasIndex(file => file.SpaceshipId);
        }
    }
}
