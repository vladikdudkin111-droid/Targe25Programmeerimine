using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TARge25Shop.ApplicationServices.Services;
using TARge25Shop.Core.ServiceInterface;
using TARge25Shop.Data;
using TARge25Shop.SpaceshipTest.Macros;
using TARge25Shop.SpaceshipTest.Mock;

namespace TARge25Shop.SpaceshipTest
{
    // Ühine alusklass loob iga testi jaoks eraldi teenused ja mälus oleva andmebaasi.
    public abstract class TestBase : IDisposable
    {
        // Nimi ja juurobjekt on ühe testi piires ühised, eri testides erinevad.
        private readonly string _databaseName = "SpaceshipTest_" + Guid.NewGuid().ToString("N");
        private readonly InMemoryDatabaseRoot _databaseRoot = new();
        private readonly MockIHostEnvironment _environment = new();

        // Juurpakkuja loob teenuseskoobid; test kasutab alati oma skoopi.
        private readonly ServiceProvider _rootProvider;
        private readonly IServiceScope _testScope;
        private bool _disposed;

        // Õpetaja serviceProvider annab ligipääsu selle testi skoobi teenustele.
        protected IServiceProvider serviceProvider => _testScope.ServiceProvider;

        protected TestBase()
        {
            var services = new ServiceCollection();
            SetupServices(services);
            _rootProvider = services.BuildServiceProvider(new ServiceProviderOptions
            {
                ValidateScopes = true,
                ValidateOnBuild = true
            });
            _testScope = _rootProvider.CreateScope();
        }

        // Registreerime päris Spaceshipi teenused ja SQL Serveri asemel InMemory pakkuja.
        public virtual void SetupServices(ServiceCollection services)
        {
            services.AddScoped<ISpaceshipServices, SpaceshipServices>();
            services.AddScoped<IFileServices, FileServices>();
            services.AddSingleton<IHostEnvironment>(_environment);

            // Sama testi eri kontekstid jagavad andmeid, eri testid üksteise andmeid ei näe.
            services.AddDbContext<TARge25ShopContext>(options =>
                options.UseInMemoryDatabase(_databaseName, _databaseRoot));

            RegisterMacros(services);
        }

        // Puuduv registreering annab kohe selge vea, mitte hilisema null-viite vea.
        protected T Svc<T>() where T : notnull
        {
            return serviceProvider.GetRequiredService<T>();
        }

        // Uus skoop annab uue DbContexti, et kontrollida salvestatud andmeid jälgijast sõltumatult.
        protected IServiceScope CreateScope()
        {
            return _rootProvider.CreateScope();
        }

        // Registreerime õpetaja markerliidese konkreetsed makroklassid.
        private static void RegisterMacros(ServiceCollection services)
        {
            var macroBaseType = typeof(IMacros);
            var macros = macroBaseType.Assembly.GetTypes()
                .Where(type => macroBaseType.IsAssignableFrom(type)
                    && !type.IsInterface && !type.IsAbstract);

            foreach (var macro in macros)
            {
                // Makro võib kasutada sama testi scoped teenuseid, näiteks DbContexti.
                services.AddScoped(macro);
            }
        }

        // xUnit kutsub IDisposable meetodi pärast iga testi, ka ebaõnnestumise korral.
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            try
            {
                _testScope.Dispose();
            }
            finally
            {
                try
                {
                    _rootProvider.Dispose();
                }
                finally
                {
                    // Ajutine kaust kuulub ainult sellele testile.
                    _environment.Dispose();
                }
            }

            GC.SuppressFinalize(this);
        }
    }
}
