# TARge25Programmeerimine

ASP.NET Core MVC project for managing spaceships. The application supports the
full CRUD flow and file attachments stored under `wwwroot/multipleFileUpload`,
following the classroom `FilesToApi` example. `FileServices` receives
`IWebHostEnvironment` and builds the upload path from `ContentRootPath`.
Each file is saved through `FileStream` and `IFormFile.CopyTo`, while its
`FileToApi` record is stored in SQL Server with `Id`, `ExistingFilePath`, and
`SpaceshipId`.

The MVC structure follows the classroom layout: `ImageViewModel`, a shared
`SpaceshipCreateUpdateViewModel`, separate Details/Delete/Index models, and one
shared `CreateUpdate.cshtml` form for creating and editing spaceships. The
`_Images.cshtml` partial renders an uploaded image from
`wwwroot/multipleFileUpload` inside the Details page, matching the classroom
example.

`TARge25ShopContext.OnModelCreating` declares the `FileToApi.SpaceshipId`
index so the runtime EF Core model exactly matches the migration snapshot.

## Run

1. Install the .NET 10 SDK and SQL Server LocalDB (or change `DefaultConnection`
   in `TARge25Shop/TARge25Shop/appsettings.json`).
2. Open `TARge25Shop/Targe25Shop.slnx` in Visual Studio.
3. Build and run the `Targe25Shop` web project. Entity Framework migrations
   are applied automatically at startup.

Each attachment may be up to 10 MB; the total uploaded in one request may be up
to 25 MB. Attachments can be downloaded from the details page and removed from
the update page. Deleting a spaceship also deletes its physical files and
`FileToApi` database records.
