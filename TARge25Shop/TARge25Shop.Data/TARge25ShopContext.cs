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
    }
}
