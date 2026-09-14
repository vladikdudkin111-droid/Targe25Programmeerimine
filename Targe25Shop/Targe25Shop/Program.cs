using Microsoft.EntityFrameworkCore;
using TARge25Shop.ApplicationServices.Services;
using TARge25Shop.Core.ServiceInterface;
using TARge25Shop.Data;

namespace TARge25Shop
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Loome veebirakenduse builderi.
            var builder = WebApplication.CreateBuilder(args);

            // Lisame MVC toe: Controllerid ja Razor Views.
            builder.Services.AddControllersWithViews();

            // Registreerime Spaceship service dependency injection konteineris.
            // Kui Controller küsib ISpaceshipServices, antakse talle SpaceshipServices objekt.
            builder.Services.AddScoped<ISpaceshipServices, SpaceshipServices>();

            // Registreerime Entity Framework DbContexti ja SQL Server ühenduse.
            // Connection string loetakse appsettings.json failist nimega DefaultConnection.
            builder.Services.AddDbContext<TARge25ShopContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Ehitame valmis veebirakenduse.
            var app = builder.Build();

            // Production keskkonnas kasutame üldist vealehte ja HSTS-i.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            // Suuname HTTP päringud HTTPS peale.
            app.UseHttpsRedirection();

            // Aktiveerime routing süsteemi.
            app.UseRouting();

            // Aktiveerime autoriseerimise middleware'i.
            app.UseAuthorization();

            // Lubame staatilised failid, näiteks CSS ja JavaScript.
            app.MapStaticAssets();

            // Määrame MVC vaikimisi route'i.
            // Näiteks /Spaceship/Index või /Spaceship/Update/{id}.
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            // Käivitame veebirakenduse.
            app.Run();
        }
    }
}
