using Microsoft.EntityFrameworkCore;
using TARge25Shop.ApplicationServices.Services;
using TARge25Shop.Core.ServiceInterface;
using TARge25Shop.Data;

namespace TARge25Shop
{
    // Program seadistab teenused, andmebaasi ja HTTP päringute töötlusahela.
    public class Program
    {
        public static void Main(string[] args)
        {
            // Builder loeb konfiguratsiooni ja valmistab ette veebirakenduse.
            var builder = WebApplication.CreateBuilder(args);

            // MVC teenused võimaldavad kasutada Controllereid ja Razor vaateid.
            builder.Services.AddControllersWithViews();

            // Registreerime rakenduse teenused dependency injection konteineris.
            builder.Services.AddScoped<ISpaceshipServices, SpaceshipServices>();
            builder.Services.AddScoped<IFileServices, FileServices>();

            // Ühenduse string peab olema appsettings.json failis määratud.
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException(
                    "Connection string 'DefaultConnection' was not found.");

            // Registreerime Entity Framework DbContexti ja SQL Serveri ühenduse.
            builder.Services.AddDbContext<TARge25ShopContext>(options =>
                options.UseSqlServer(
                    connectionString,
                    sqlServerOptions => sqlServerOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(10),
                        errorNumbersToAdd: null)));

            // Ehitame valmis veebirakenduse.
            var app = builder.Build();

            // Rakendame olemasolevad migratsioonid automaatselt enne esimest päringut.
            // Mudel ja migratsioonid on omavahel kooskõlas, seega ei teki PendingModelChanges viga.
            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<TARge25ShopContext>();
                context.Database.Migrate();
            }

            // Production keskkonnas kasutame üldist vealehte ja HSTS-i.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            // Suuname HTTP päringud turvalisele HTTPS ühendusele.
            app.UseHttpsRedirection();

            // UseStaticFiles teenindab ka rakenduse töö ajal üleslaaditud pilte.
            app.UseStaticFiles();

            // Aktiveerime routing süsteemi.
            app.UseRouting();

            // Aktiveerime autoriseerimise middleware'i.
            app.UseAuthorization();

            // MapStaticAssets teenindab buildi ajal teadaolevaid staatilisi faile.
            app.MapStaticAssets();

            // Määrame MVC vaikimisi route'i.
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            // Käivitame rakenduse ja alustame HTTP päringute vastuvõtmist.
            app.Run();
        }
    }
}
