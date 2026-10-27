using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using TARge25Shop.Core.Domain;
using TARge25Shop.Data;

namespace TARge25Shop.SeleniumTesting;

internal sealed class UiApplication : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");

    public UiApplication()
    {
        _connection.Open();
        using var context = CreateContext();
        context.Database.EnsureCreated();
    }

    public Uri Start()
    {
        UseKestrel(0);
        StartServer();
        using var client = CreateClient();
        return client.BaseAddress!;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureLogging(logging => logging.SetMinimumLevel(LogLevel.Error));
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<TARge25ShopContext>();
            services.AddScoped<TARge25ShopContext>(_ => CreateContext());
        });
    }

    private UiDbContext CreateContext() => new(
        new DbContextOptionsBuilder<TARge25ShopContext>().UseSqlite(_connection).Options);

    protected override void Dispose(bool disposing)
    {
        try
        {
            base.Dispose(disposing);
        }
        finally
        {
            if (disposing) _connection.Dispose();
        }
    }

    private sealed class UiDbContext(DbContextOptions<TARge25ShopContext> options)
        : TARge25ShopContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Testandmebaasis vastab SQL Serveri varbinary(max) tüübile BLOB.
            modelBuilder.Entity<KindergartenImage>().Property(x => x.Data).HasColumnType("BLOB");
        }
    }
}
