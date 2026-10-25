using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TARge25Shop.ApplicationServices.Services;
using TARge25Shop.Core.Domain;
using TARge25Shop.Core.Dto;
using TARge25Shop.Core.ServiceInterface;
using TARge25Shop.Data;

namespace TARge25Shop.Tests;

public abstract class TestBase : IDisposable
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");
    private readonly ServiceProvider _provider;
    private readonly IServiceScope _scope;

    protected TestBase()
    {
        _connection.Open();
        var services = new ServiceCollection();
        services.AddScoped<TARge25ShopContext>(_ => NewContext());
        services.AddScoped<IKindergartenServices, KindergartenServices>();
        services.AddScoped<IFileService, FileService>();
        _provider = services.BuildServiceProvider(validateScopes: true);
        _scope = _provider.CreateScope();
        Svc<TARge25ShopContext>().Database.EnsureCreated();
    }

    protected T Svc<T>() where T : notnull => _scope.ServiceProvider.GetRequiredService<T>();

    protected TARge25ShopContext NewContext() => new SqliteTestContext(
        new DbContextOptionsBuilder<TARge25ShopContext>().UseSqlite(_connection).Options);

    protected static KindergartenDto Group(params FileUploadDto[] files) => new()
    {
        GroupName = "Päikesekiired",
        KindergartenName = "Vikerkaare lasteaed",
        TeacherName = "Katrin Tamm",
        ChildrenCount = 18,
        Files = files.ToList()
    };

    protected static FileUploadDto Png(string name = "rühm.png", int? size = null)
    {
        var bytes = Convert.FromBase64String(
            "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNk+A8AAQUBAScY42YAAAAASUVORK5CYII=");
        if (size.HasValue) Array.Resize(ref bytes, size.Value);
        return new FileUploadDto { FileName = name, Data = bytes };
    }

    public void Dispose()
    {
        _scope.Dispose();
        _provider.Dispose();
        _connection.Dispose();
    }

    private sealed class SqliteTestContext(DbContextOptions<TARge25ShopContext> options)
        : TARge25ShopContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // SQL Serveri varbinary(max) vaste SQLite testandmebaasis.
            modelBuilder.Entity<KindergartenImage>().Property(x => x.Data).HasColumnType("BLOB");
        }
    }
}
