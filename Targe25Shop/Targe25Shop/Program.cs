using Microsoft.EntityFrameworkCore;
using TARge25Shop.ApplicationServices.Services;
using TARge25Shop.Core.ServiceInterface;
using TARge25Shop.Data;

namespace TARge25Shop
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Registreerime MVC ning ühe HTTP päringu piires kasutatavad teenused.
            builder.Services.AddControllersWithViews();
            builder.Services.AddScoped<ISpaceshipServices, SpaceshipServices>();
            builder.Services.AddScoped<IFileServices, FileServices>();
            builder.Services.AddScoped<IRealEstateServices, RealEstateServices>();

            // Ühendusstring määrab nii SQL Serveri eksemplari kui ka andmebaasi nime.
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "ConnectionStrings:DefaultConnection puudub. Kontrolli veebiprojekti appsettings.json faili.");
            }

            // Korduskatsed aitavad ajutise katkestuse korral, näiteks LocalDB käivitumisel.
            // Need ei asenda LocalDB paigaldamist ega andmebaasi migratsioonide rakendamist.
            builder.Services.AddDbContext<TARge25ShopContext>(options =>
                options.UseSqlServer(connectionString, sqlOptions =>
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(2),
                        errorNumbersToAdd: null)));

            var app = builder.Build();

            // Arenduskeskkonnas loome puuduva andmebaasi ja rakendame olemasolevad migratsioonid.
            // Valmistame tabelid ette enne esimese Spaceship või RealEstate lehe avamist.
            if (app.Environment.IsDevelopment())
            {
                await using var scope = app.Services.CreateAsyncScope();
                var context = scope.ServiceProvider.GetRequiredService<TARge25ShopContext>();
                try
                {
                    await context.Database.MigrateAsync();
                }
                catch (Exception exception)
                {
                    // Säilitame algse vea koos InnerExceptioniga, et SQL Serveri tõrke põhjus oleks nähtav.
                    app.Logger.LogCritical(exception,
                        "Andmebaasi ettevalmistamine ebaõnnestus. Kontrolli LocalDB olekut, " +
                        "DefaultConnection ühendust ja sisemist SQL Serveri veateadet. " +
                        "Juhised asuvad failis START_AND_DATABASE_FIX.md.");
                    throw;
                }
            }

            // Tootmiskeskkonnas kasutame üldist vealehte ja HTTPS-i turvapäist.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            // Suuname HTTPS-i ning seome päringud kontrollerite tegevustega.
            app.UseHttpsRedirection();
            // Teenindame ka pärast rakenduse käivitamist üles laaditud pilte.
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();

            // Avaldame staatilised ressursid ja MVC vaikemarsruudi.
            app.MapStaticAssets();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            await app.RunAsync();
        }
    }
}
