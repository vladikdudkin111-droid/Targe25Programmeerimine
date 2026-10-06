using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using TARge25Shop.ApplicationServices.Services;
using TARge25Shop.Controllers;
using TARge25Shop.Core.Dto;
using TARge25Shop.Data;
using TARge25Shop.Models.RealEstate;
using Xunit;

namespace TARge25Shop.Tests
{
    // Igal testil on oma andmebaas ja ajutine kaust; SQL Serverit ei käivitata.
    public sealed class ProjectTests : IDisposable
    {
        private readonly string _databaseName = Guid.NewGuid().ToString("N");
        private readonly InMemoryDatabaseRoot _databaseRoot = new();
        private readonly TestEnvironment _environment = new();
        private readonly List<MemoryStream> _streams = new();

        private TARge25ShopContext Context() => new(
            new DbContextOptionsBuilder<TARge25ShopContext>()
                .UseInMemoryDatabase(_databaseName, _databaseRoot).Options);
        private FileServices Files(TARge25ShopContext context) => new(_environment, context);
        private RealEstateServices Estates(TARge25ShopContext context) => new(context, Files(context));
        private SpaceshipServices Ships(TARge25ShopContext context) => new(context, Files(context));
        private RealEstateController Controller(TARge25ShopContext context)
        {
            var http = new DefaultHttpContext();
            return new RealEstateController(Estates(context), context, Files(context))
            {
                ControllerContext = new ControllerContext { HttpContext = http },
                TempData = new TempDataDictionary(http, new TestTempDataProvider())
            };
        }

        private IFormFile Image(string name, params byte[] bytes)
        {
            var stream = new MemoryStream(bytes);
            _streams.Add(stream);
            return new FormFile(stream, 0, stream.Length, "Files", name);
        }
        private static RealEstateDto Estate(params IFormFile[] files) => new()
        {
            Area = 42.5, Location = "Tallinn 1", RoomNumber = 2,
            BuildingType = "Korter", Files = files.ToList()
        };
        private static SpaceshipDto Ship(params IFormFile[] files) => new()
        {
            Name = "Testlaev", ShipType = "Explorer", Crew = 4,
            EnginePower = 100, Files = files.ToList()
        };

        [Fact]
        public async Task RealEstate_Update_PreservesCreatedAtAndAppendsFiles()
        {
            Guid id;
            var createdAt = new DateTime(2020, 1, 2);
            using (var setup = Context())
            {
                var entity = await Estates(setup).Create(Estate(Image("first.png", 1, 2, 3)));
                id = entity.Id.GetValueOrDefault();
                entity.CreatedAt = createdAt;
                await setup.SaveChangesAsync();
            }
            using (var update = Context())
            {
                var dto = Estate(Image("second.jpg", 4, 5, 6));
                dto.Id = id;
                dto.Location = "Tartu 2";
                dto.CreatedAt = new DateTime(2099, 1, 1);
                Assert.NotNull(await Estates(update).Update(dto));
            }
            // Uus kontekst kontrollib salvestatud andmeid, mitte vana jälgitavat objekti.
            using var verify = Context();
            var saved = await verify.RealEstates.SingleAsync();
            Assert.Equal(id, saved.Id.GetValueOrDefault());
            Assert.Equal("Tartu 2", saved.Location);
            Assert.Equal(createdAt, saved.CreatedAt.GetValueOrDefault());
            var files = await verify.FileToDatabases.ToListAsync();
            Assert.Equal(2, files.Count);
            Assert.All(files, x => Assert.Equal(id, x.RealEstateId.GetValueOrDefault()));
            Assert.Equal(new byte[] { 1, 2, 3 }, Assert.Single(files, x => x.ImageTitle == "first.png").ImageData);
            Assert.Equal(new byte[] { 4, 5, 6 }, Assert.Single(files, x => x.ImageTitle == "second.jpg").ImageData);
        }

        [Fact]
        public async Task RealEstate_Views_LoadAllImagesWithMatchingMimeTypes()
        {
            Guid id;
            using (var setup = Context())
            {
                var entity = await Estates(setup).Create(Estate(
                    Image("a.png", 1, 2), Image("b.jpg", 3, 4), Image("c.gif", 5, 6)));
                id = entity.Id.GetValueOrDefault();
            }
            using var context = Context();
            var controller = Controller(context);
            var update = Assert.IsType<ViewResult>(await controller.Update(id));
            var vm = Assert.IsType<RealEstateCreateUpdateViewModel>(update.Model);
            Assert.Equal(3, vm.Image.Count);
            Assert.Equal("data:image/png;base64,AQI=", Assert.Single(vm.Image, x => x.ImageTitle == "a.png").Image);
            Assert.Equal("data:image/jpeg;base64,AwQ=", Assert.Single(vm.Image, x => x.ImageTitle == "b.jpg").Image);
            Assert.Equal("data:image/gif;base64,BQY=", Assert.Single(vm.Image, x => x.ImageTitle == "c.gif").Image);
            var details = Assert.IsType<ViewResult>(await controller.Details(id));
            Assert.Equal(3, Assert.IsType<RealEstateDetailsViewModel>(details.Model).Image.Count);
            var delete = Assert.IsType<ViewResult>(await controller.Delete(id));
            Assert.Equal(3, Assert.IsType<RealEstateDeleteViewModel>(delete.Model).Image.Count);
            var index = Assert.IsType<ViewResult>(await controller.Index());
            Assert.Single(Assert.IsType<List<RealEstateIndexViewModel>>(index.Model));
        }

        [Fact]
        public async Task RealEstate_RemoveImage_ChecksOwnerAndKeepsOtherImages()
        {
            Guid firstId, otherId, firstImage, otherImage;
            using (var setup = Context())
            {
                firstId = (await Estates(setup).Create(Estate(Image("a.png", 1), Image("b.png", 2)))).Id.GetValueOrDefault();
                otherId = (await Estates(setup).Create(Estate(Image("c.png", 3)))).Id.GetValueOrDefault();
                firstImage = await setup.FileToDatabases.Where(x => x.RealEstateId == firstId).Select(x => x.Id).FirstAsync();
                otherImage = await setup.FileToDatabases.Where(x => x.RealEstateId == otherId).Select(x => x.Id).SingleAsync();
            }
            using (var action = Context())
            {
                var controller = Controller(action);
                Assert.IsType<NotFoundResult>(await controller.RemoveImage(firstId, otherImage));
                var result = Assert.IsType<RedirectToActionResult>(await controller.RemoveImage(firstId, firstImage));
                Assert.Equal("Update", result.ActionName);
                Assert.Equal(firstId, Assert.IsType<Guid>(result.RouteValues!["id"]));
            }
            using var verify = Context();
            Assert.Equal(2, await verify.RealEstates.CountAsync());
            Assert.Equal(2, await verify.FileToDatabases.CountAsync());
            Assert.False(await verify.FileToDatabases.AnyAsync(x => x.Id == firstImage));
            Assert.True(await verify.FileToDatabases.AnyAsync(x => x.Id == otherImage));
        }

        [Fact]
        public async Task RealEstate_Delete_RemovesOnlyOwnedImagesAndHandlesMissingIds()
        {
            Guid firstId, otherId;
            using (var setup = Context())
            {
                firstId = (await Estates(setup).Create(Estate(Image("a.png", 1)))).Id.GetValueOrDefault();
                otherId = (await Estates(setup).Create(Estate(Image("b.png", 2)))).Id.GetValueOrDefault();
            }
            using (var action = Context())
            {
                Assert.NotNull(await Estates(action).Delete(firstId));
                Assert.Null(await Estates(action).Delete(firstId));
                Assert.Null(await Estates(action).DetailAsync(firstId));
                var missing = Estate(); missing.Id = firstId;
                Assert.Null(await Estates(action).Update(missing));
            }
            using var verify = Context();
            Assert.Equal(otherId, (await verify.RealEstates.SingleAsync()).Id.GetValueOrDefault());
            Assert.Equal(otherId, (await verify.FileToDatabases.SingleAsync()).RealEstateId.GetValueOrDefault());
        }

        [Fact]
        public async Task Spaceship_UpdateAndDelete_HandleDiskFilesAndPreserveCreatedAt()
        {
            Guid id, otherId;
            var createdAt = new DateTime(2020, 2, 3);
            using (var setup = Context())
            {
                var entity = await Ships(setup).Create(Ship(Image("a.png", 1), Image("b.jpg", 2)));
                id = entity.Id.GetValueOrDefault();
                entity.CreatedAt = createdAt;
                await setup.SaveChangesAsync();
                otherId = (await Ships(setup).Create(Ship(Image("other.png", 3)))).Id.GetValueOrDefault();
            }
            using (var update = Context())
            {
                var dto = Ship(Image("added.gif", 4)); dto.Id = id; dto.CreatedAt = DateTime.MaxValue;
                var result = await Ships(update).Update(dto);
                Assert.NotNull(result);
                Assert.Equal(createdAt, result.CreatedAt);
                var ownImage = await update.FileToApis.FirstAsync(x => x.SpaceshipId == id);
                var ownPath = Path.Combine(_environment.ContentRootPath, "wwwroot", "multipleFileUpload", ownImage.ExistingFilePath!);
                Assert.True(File.Exists(ownPath));
                Assert.Null(await Files(update).RemoveImageFromApi(new FileToApiDto { Id = ownImage.Id, SpaceshipId = otherId }));
                Assert.NotNull(await Files(update).RemoveImageFromApi(new FileToApiDto { Id = ownImage.Id, SpaceshipId = id }));
                Assert.False(File.Exists(ownPath));
            }
            using (var delete = Context())
            {
                Assert.NotNull(await Ships(delete).Delete(id));
                Assert.Null(await Ships(delete).Delete(id));
            }
            using var verify = Context();
            Assert.Equal(otherId, (await verify.Spaceships.SingleAsync()).Id.GetValueOrDefault());
            Assert.Equal(otherId, (await verify.FileToApis.SingleAsync()).SpaceshipId.GetValueOrDefault());
            Assert.Single(Directory.GetFiles(Path.Combine(_environment.ContentRootPath, "wwwroot", "multipleFileUpload")));
        }

        [Fact]
        public async Task InvalidUpload_ReturnsFormWithoutChangingStoredData()
        {
            Guid id;
            using (var setup = Context())
                id = (await Estates(setup).Create(Estate(Image("a.png", 1)))).Id.GetValueOrDefault();
            using (var action = Context())
            {
                var controller = Controller(action);
                var vm = new RealEstateCreateUpdateViewModel
                {
                    Id = id, Location = "Not saved", Area = 10, RoomNumber = 1, BuildingType = "Maja",
                    Files = new() { Image("page.html", 1, 2) }
                };
                var result = Assert.IsType<ViewResult>(await controller.Update(vm));
                Assert.Equal("CreateUpdate", result.ViewName);
                Assert.False(controller.ModelState.IsValid);
                Assert.Single(Assert.IsType<RealEstateCreateUpdateViewModel>(result.Model).Image);
            }
            using var verify = Context();
            Assert.Equal("Tallinn 1", (await verify.RealEstates.SingleAsync()).Location);
            Assert.Equal(1, await verify.FileToDatabases.CountAsync());
        }

        [Fact]
        public void SqlServerModel_MatchesMigrationSnapshot()
        {
            // Mudeli võrdlus ei ava SQL Serveri ühendust ega muuda ühtegi andmebaasi.
            using var context = new TARge25ShopContext(new DbContextOptionsBuilder<TARge25ShopContext>()
                .UseSqlServer("Server=localhost;Database=ModelCheck;Integrated Security=true;TrustServerCertificate=true")
                .Options);
            Assert.Equal(4, context.Database.GetMigrations().Count());
            Assert.False(context.Database.HasPendingModelChanges());
        }

        public void Dispose()
        {
            foreach (var stream in _streams) stream.Dispose();
            if (Directory.Exists(_environment.ContentRootPath)) Directory.Delete(_environment.ContentRootPath, true);
        }

        private sealed class TestEnvironment : IHostEnvironment
        {
            public string EnvironmentName { get; set; } = "Testing";
            public string ApplicationName { get; set; } = "TARge25Shop.Tests";
            public string ContentRootPath { get; set; } = Path.Combine(Path.GetTempPath(), "TARge25ShopTests", Guid.NewGuid().ToString("N"));
            public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
        }
        private sealed class TestTempDataProvider : ITempDataProvider
        {
            public IDictionary<string, object> LoadTempData(HttpContext context) => new Dictionary<string, object>();
            public void SaveTempData(HttpContext context, IDictionary<string, object> values) { }
        }
    }
}
