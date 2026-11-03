using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TARge25Shop.ApplicationServices.Services;
using TARge25Shop.Core.Dto;
using TARge25Shop.Core.ServiceInterface;
using TARge25Shop.Data;

namespace TARge25Shop.KindergartenTest;

public abstract class TestBase : IDisposable
{
    private readonly ServiceProvider _provider;
    private readonly IServiceScope _scope;
    protected TestBase()
    {
        var services = new ServiceCollection();

        // Iga test kasutab eraldi mälus olevat andmebaasi.
        services.AddDbContext<TARge25ShopContext>(options =>
            options.UseInMemoryDatabase(Guid.NewGuid().ToString()));

        services.AddScoped<IKindergartenServices, KindergartenServices>();
        services.AddScoped<IFileService, FileService>();

        _provider = services.BuildServiceProvider(
            new ServiceProviderOptions
            {
                ValidateScopes = true
            });

        _scope = _provider.CreateScope();
    }

    protected T Svc<T>() where T : notnull =>
        _scope.ServiceProvider.GetRequiredService<T>();

    protected TARge25ShopContext Context =>
        Svc<TARge25ShopContext>();

    protected IKindergartenServices Service =>
        Svc<IKindergartenServices>();

    protected IFileService Files =>
        Svc<IFileService>();

    protected static KindergartenDto MockKindergartenData(
        bool withImage = false) => new()
        {
            Id = Guid.NewGuid(),
            GroupName = "Paikesekiired",
            ChildrenCount = 18,
            KindergartenName = "Paikeseke",
            TeacherName = "Mari",
            CreatedAt = new DateTime(1990, 1, 1),
            UpdatedAt = new DateTime(1990, 1, 1),
            Files = withImage ? [MockImage()] : []
        };

    protected static FileUploadDto MockImage(
        string name = "test.png") => new()
        {
            FileName = name,

            // Väike PNG-pilt testimiseks.
            Data = Convert.FromBase64String(
            "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNk+A8AAQUBAScY42YAAAAASUVORK5CYII=")
        };

    public void Dispose()
    {
        _scope.Dispose();
        _provider.Dispose();
    }
}