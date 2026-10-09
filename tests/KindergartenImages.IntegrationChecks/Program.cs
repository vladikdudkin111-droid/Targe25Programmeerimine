using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using TARge25Shop.ApplicationServices.Services;
using TARge25Shop.Core.Domain;
using TARge25Shop.Core.Dto;
using TARge25Shop.Data;

var repo = args.Length > 0 ? Path.GetFullPath(args[0]) : Directory.GetCurrentDirectory();
var webRoot = Path.Combine(repo, "TARge25Shop", "TARge25Shop");
var webDll = Path.Combine(webRoot, "bin", "Debug", "net10.0", "TARge25Shop.dll");
Check(File.Exists(webDll), "Build the web solution before running these checks.");

// Iga kontroll kasutab oma andmebaasi; rakenduse andmeid ei muudeta.
var database = "TARge25Shop_ImageChecks_" + Guid.NewGuid().ToString("N");
var connection = $"Server=(localdb)\\MSSQLLocalDB;Database={database};Trusted_Connection=true;MultipleActiveResultSets=true";
var options = new DbContextOptionsBuilder<TARge25ShopContext>().UseSqlServer(connection).Options;
await using var context = new TARge25ShopContext(options);
var listener = new TcpListener(IPAddress.Loopback, 0);
listener.Start();
var port = ((IPEndPoint)listener.LocalEndpoint).Port;
listener.Stop();
using var client = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false })
{
    BaseAddress = new Uri($"http://127.0.0.1:{port}"),
    Timeout = TimeSpan.FromSeconds(30)
};
Process? server = null;
Task<string>? standardOutput = null;
Task<string>? standardError = null;
var png = Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNk+A8AAQUBAScY42YAAAAASUVORK5CYII=");
var gif = Convert.FromBase64String("R0lGODlhAQABAIAAAAAAAP///yH5BAEAAAAALAAAAAABAAEAAAICRAEAOw==");
try
{
    await context.GetService<IMigrator>().MigrateAsync("20260920120000_AddKindergarten");
    var existing = new Kindergarten
    {
        Id = Guid.NewGuid(), GroupName = "Existing group", ChildrenCount = 12,
        KindergartenName = "Existing kindergarten", TeacherName = "Mari",
        CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now
    };
    context.Kindergartens.Add(existing);
    context.Spaceships.Add(new Spaceship
    {
        Id = Guid.NewGuid(), Name = "Existing ship", ShipType = "Research", Crew = 3,
        EnginePower = 50, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now
    });
    await context.SaveChangesAsync();
    context.ChangeTracker.Clear();
    await context.Database.MigrateAsync();
    Check(!context.Database.HasPendingModelChanges(), "Model and migration differ.");
    Check(await context.Kindergartens.CountAsync() == 1 && await context.Spaceships.CountAsync() == 1,
        "Migration lost existing records.");
    Pass("Migration preserves existing Kindergarten and Spaceship records");

    var files = new FileService(context);
    Reject(() => files.PrepareImages([new() { FileName = "empty.png" }]));
    Reject(() => files.PrepareImages([new() { FileName = "fake.png", Data = Encoding.UTF8.GetBytes("<html>bad</html>") }]));
    Reject(() => files.PrepareImages([new() { FileName = "picture.svg", Data = png }]));
    Reject(() => files.PrepareImages([new() { FileName = "large.png", Data = new byte[FileUploadDto.MaxFileSize + 1] }]));
    Reject(() => files.PrepareImages(Enumerable.Range(0, 11).Select(_ => new FileUploadDto { FileName = "image.png", Data = png }).ToList()));
    Reject(() => files.PrepareImages(Enumerable.Range(0, 5).Select(_ => new FileUploadDto { FileName = "image.png", Data = new byte[FileUploadDto.MaxFileSize] }).ToList()));
    var safeName = files.PrepareImages([new() { FileName = "C:\\fakepath\\photo.PNG", Data = png }]).Single();
    Check(safeName.FileName == "photo.PNG" && safeName.ContentType == "image/png", "Unsafe filename or MIME type.");
    Pass("File validation: format, signature, empty file, count, sizes, safe filename");

    await StartServer();
    var createPage = await GetHtml("/Kindergarten/Create");
    Check(createPage.Contains("multipart/form-data") && createPage.Contains("multiple"), "Upload input missing.");
    var fields = KindergartenFields("Image check");
    using (var response = await Post("/Kindergarten/Create", "/Kindergarten/Create", fields,
        [("first.png", png), ("second.gif", gif)]))
        Check(response.StatusCode == HttpStatusCode.Found, "Create with images failed.");

    var group = await context.Kindergartens.AsNoTracking().SingleAsync(x => x.GroupName == "Image check");
    var id = group.Id;
    var images = await context.KindergartenImages.AsNoTracking().Where(x => x.KindergartenId == id).ToListAsync();
    Check(images.Count == 2, "Images not stored.");
    Check(images.Single(x => x.FileName == "first.png").Data.SequenceEqual(png), "PNG bytes not persisted.");
    Check(images.Single(x => x.FileName == "second.gif").Data.SequenceEqual(gif), "GIF bytes not persisted.");
    var first = images.Single(x => x.FileName == "first.png");
    var details = await GetHtml($"/Kindergarten/Details/{id}");
    Check(details.Contains("first.png") && details.Contains("second.gif"), "Gallery missing.");
    await CheckImage(first.Id, "image/png", png);
    Pass("Create with multiple images, database bytes, Details gallery, image response");

    await StopServer();
    await StartServer();
    await CheckImage(first.Id, "image/png", png);
    Pass("Images remain available after the web server restarts");

    fields["Id"] = id.ToString();
    fields["ChildrenCount"] = "19";
    using (var response = await Post($"/Kindergarten/Update/{id}", $"/Kindergarten/Update/{id}", fields))
        Check(response.StatusCode == HttpStatusCode.Found, "Update without files failed.");
    Check(await context.KindergartenImages.CountAsync(x => x.KindergartenId == id) == 2, "Update removed existing images.");
    using (var response = await Post($"/Kindergarten/Update/{id}", $"/Kindergarten/Update/{id}", fields, [("third.png", png)]))
        Check(response.StatusCode == HttpStatusCode.Found, "Adding an image on update failed.");
    var updated = await context.Kindergartens.AsNoTracking().SingleAsync(x => x.Id == id);
    Check(updated.CreatedAt == group.CreatedAt && updated.UpdatedAt > group.UpdatedAt, "Incorrect timestamps.");
    Check(await context.KindergartenImages.CountAsync(x => x.KindergartenId == id) == 3, "Append did not keep existing images.");
    Pass("Update appends images, no-file update preserves them, CreatedAt remains unchanged");

    fields["GroupName"] = "Should not save";
    using (var response = await Post($"/Kindergarten/Update/{id}", $"/Kindergarten/Update/{id}", fields,
        [("valid.png", png), ("fake.png", Encoding.UTF8.GetBytes("not an image"))]))
    {
        var html = await response.Content.ReadAsStringAsync();
        Check(response.StatusCode == HttpStatusCode.OK && html.Contains("field-validation-error"), "Invalid image accepted.");
        Check(html.Contains("first.png") && html.Contains("third.png"), "Validation lost the existing gallery.");
    }
    Check(await context.KindergartenImages.CountAsync(x => x.KindergartenId == id) == 3, "Invalid batch partially persisted.");
    var unchanged = await context.Kindergartens.AsNoTracking().SingleAsync(x => x.Id == id);
    Check(unchanged.GroupName == "Image check" && unchanged.UpdatedAt == updated.UpdatedAt, "Failed upload changed group fields.");
    fields["GroupName"] = "";
    using (var response = await Post($"/Kindergarten/Update/{id}", $"/Kindergarten/Update/{id}", fields, [("fourth.png", png)]))
    {
        var html = await response.Content.ReadAsStringAsync();
        Check(response.StatusCode == HttpStatusCode.OK && html.Contains("first.png"), "Invalid group lost its gallery.");
    }
    using (var response = await Post("/Kindergarten/Create", "/Kindergarten/Create", KindergartenFields("Invalid batch"),
        [("valid.png", png), ("bad.png", Encoding.UTF8.GetBytes("bad"))]))
        Check(response.StatusCode == HttpStatusCode.OK, "Invalid create should show validation.");
    Check(!await context.Kindergartens.AnyAsync(x => x.GroupName == "Invalid batch"), "Invalid create persisted a group.");
    using (var response = await Post("/Kindergarten/Create", "/Kindergarten/Create", KindergartenFields("Too large"),
        [("large.png", new byte[FileUploadDto.MaxFileSize + 1])]))
        Check(response.StatusCode == HttpStatusCode.OK, "Oversized image should show validation.");
    Check(!await context.Kindergartens.AnyAsync(x => x.GroupName == "Too large"), "Oversized file created a group.");
    Pass("Invalid uploads and invalid forms do not save partial changes; gallery remains visible");

    var other = await new KindergartenServices(context, files).Create(new KindergartenDto
    {
        GroupName = "Other group", KindergartenName = "Other kindergarten", TeacherName = "Mari",
        Files = [new() { FileName = "other.png", Data = png }]
    });
    context.ChangeTracker.Clear();
    using (var response = await client.GetAsync($"/Kindergarten/Image/{other.Id}?imageId={first.Id}"))
        Check(response.StatusCode == HttpStatusCode.NotFound, "Image readable under the wrong group.");
    using (var response = await Post($"/Kindergarten/DeleteImage/{other.Id}", $"/Kindergarten/Update/{other.Id}", new() { ["imageId"] = first.Id.ToString() }))
        Check(response.StatusCode == HttpStatusCode.NotFound, "Image deleted under the wrong group.");
    using (var response = await client.GetAsync($"/Kindergarten/DeleteImage/{id}?imageId={first.Id}"))
        Check(response.StatusCode is HttpStatusCode.MethodNotAllowed or HttpStatusCode.NotFound,
            $"GET deletion endpoint unexpectedly responds with {response.StatusCode}.");
    using (var response = await client.PostAsync($"/Kindergarten/DeleteImage/{id}", new FormUrlEncodedContent(new Dictionary<string, string> { ["imageId"] = first.Id.ToString() })))
        Check(response.StatusCode == HttpStatusCode.BadRequest, "Deletion accepts a missing antiforgery token.");
    Check(await context.KindergartenImages.AnyAsync(x => x.Id == first.Id), "Rejected deletion removed the image.");
    Pass("Image ownership, POST-only deletion, antiforgery protection");

    using (var response = await Post($"/Kindergarten/DeleteImage/{id}", $"/Kindergarten/Update/{id}", new() { ["imageId"] = first.Id.ToString() }))
        Check(response.StatusCode == HttpStatusCode.Found, "Single-image deletion failed.");
    Check(await context.Kindergartens.AnyAsync(x => x.Id == id) && await context.KindergartenImages.CountAsync(x => x.KindergartenId == id) == 2,
        "Single deletion removed the group or other images.");
    using (var response = await client.GetAsync($"/Kindergarten/Image/{id}?imageId={first.Id}"))
        Check(response.StatusCode == HttpStatusCode.NotFound, "Deleted image still readable.");
    Pass("Single-image deletion preserves the group and the remaining images");

    var deletePage = await GetHtml($"/Kindergarten/Delete/{id}");
    Check(deletePage.Contains("All images") && deletePage.Contains("third.png"), "Delete confirmation lacks images.");
    Check(await context.KindergartenImages.CountAsync(x => x.KindergartenId == id) == 2, "GET confirmation deletes images.");
    using (var response = await Post("/Kindergarten/DeleteConfirmation", $"/Kindergarten/Delete/{id}", new() { ["Id"] = id.ToString() }))
        Check(response.StatusCode == HttpStatusCode.Found, "Group deletion failed.");
    Check(!await context.Kindergartens.AnyAsync(x => x.Id == id), "Deleted group remains.");
    Check(!await context.KindergartenImages.AnyAsync(x => x.KindergartenId == id), "Orphaned image rows remain.");
    Check(await context.KindergartenImages.CountAsync(x => x.KindergartenId == other.Id) == 1, "Deletion affected another group.");
    Check(await context.Kindergartens.AnyAsync(x => x.Id == existing.Id), "Deletion affected an existing group.");
    Pass("Deleting a group cascades to its image bytes and leaves unrelated groups intact");

    // Kontrollime ka ankeedi loomist ilma piltideta.
    using (var response = await Post("/Kindergarten/Create", "/Kindergarten/Create", KindergartenFields("No images")))
        Check(response.StatusCode == HttpStatusCode.Found, "Creation without images failed.");
    var plain = await context.Kindergartens.AsNoTracking().SingleAsync(x => x.GroupName == "No images");
    Check(await context.KindergartenImages.CountAsync(x => x.KindergartenId == plain.Id) == 0, "Unexpected image created.");
    var shipFields = new Dictionary<string, string> { ["Name"] = "Smoke ship", ["ShipType"] = "Research", ["Crew"] = "5", ["EnginePower"] = "100" };
    using (var response = await Post("/Spaceship/Create", "/Spaceship/Create", shipFields))
        Check(response.StatusCode == HttpStatusCode.Found, "Spaceship create failed.");
    var ship = await context.Spaceships.AsNoTracking().SingleAsync(x => x.Name == "Smoke ship");
    Check((await GetHtml($"/Spaceship/Details/{ship.Id}")).Contains("Smoke ship"), "Spaceship details failed.");
    shipFields["Id"] = ship.Id.ToString();
    shipFields["Name"] = "Updated ship";
    using (var response = await Post($"/Spaceship/Update/{ship.Id}", $"/Spaceship/Update/{ship.Id}", shipFields))
        Check(response.StatusCode == HttpStatusCode.Found, "Spaceship update failed.");
    Check(await context.Spaceships.AnyAsync(x => x.Id == ship.Id && x.Name == "Updated ship"), "Spaceship update not saved.");
    using (var response = await Post($"/Spaceship/Delete/{ship.Id}", $"/Spaceship/Delete/{ship.Id}", new() { ["Id"] = ship.Id.ToString() }))
        Check(response.StatusCode == HttpStatusCode.Found, "Spaceship delete failed.");
    Check(await context.Spaceships.CountAsync() == 1, "Spaceship deletion affected existing records.");
    Pass("Kindergarten without images and Spaceship CRUD regression checks");
    Console.WriteLine("ALL CHECKS PASSED");
}
catch
{
    await StopServer();
    if (standardOutput != null) Console.Error.WriteLine(await standardOutput);
    if (standardError != null) Console.Error.WriteLine(await standardError);
    throw;
}
finally
{
    await StopServer();
    await context.Database.EnsureDeletedAsync();
    Console.WriteLine("Removed isolated test database: " + database);
}

async Task StartServer()
{
    var start = new ProcessStartInfo("dotnet")
    {
        WorkingDirectory = webRoot,
        UseShellExecute = false,
        CreateNoWindow = true,
        WindowStyle = ProcessWindowStyle.Hidden,
        RedirectStandardOutput = true,
        RedirectStandardError = true
    };
    start.ArgumentList.Add(webDll);
    start.ArgumentList.Add("--urls");
    start.ArgumentList.Add(client.BaseAddress!.ToString());
    start.Environment["ConnectionStrings__DefaultConnection"] = connection;
    start.Environment["ASPNETCORE_ENVIRONMENT"] = "Development";
    start.Environment["Logging__LogLevel__Default"] = "Warning";
    start.Environment["Logging__LogLevel__Microsoft.EntityFrameworkCore.Database.Command"] = "Warning";
    server = Process.Start(start) ?? throw new InvalidOperationException("Server did not start.");
    standardOutput = server.StandardOutput.ReadToEndAsync();
    standardError = server.StandardError.ReadToEndAsync();
    for (var attempt = 0; attempt < 60; attempt++)
    {
        if (server.HasExited) throw new InvalidOperationException("Web server exited during startup.");
        try
        {
            using var response = await client.GetAsync("/");
            if (response.IsSuccessStatusCode) return;
        }
        catch (HttpRequestException) { }
        await Task.Delay(200);
    }
    throw new TimeoutException("Web server did not become ready.");
}

async Task StopServer()
{
    if (server == null) return;
    if (!server.HasExited) server.Kill(entireProcessTree: true);
    await server.WaitForExitAsync();
    server.Dispose();
    server = null;
}

async Task<string> GetHtml(string path)
{
    using var response = await client.GetAsync(path);
    Check(response.StatusCode == HttpStatusCode.OK, $"GET {path}: {response.StatusCode}");
    return await response.Content.ReadAsStringAsync();
}

async Task<HttpResponseMessage> Post(string path, string formPath, Dictionary<string, string> fields,
    (string Name, byte[] Data)[]? uploads = null)
{
    var html = await GetHtml(formPath);
    var token = Regex.Match(html, "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"");
    Check(token.Success, "Missing antiforgery token.");
    using var content = new MultipartFormDataContent();
    foreach (var field in fields) content.Add(new StringContent(field.Value), field.Key);
    content.Add(new StringContent(WebUtility.HtmlDecode(token.Groups[1].Value)), "__RequestVerificationToken");
    foreach (var upload in uploads ?? [])
    {
        var data = new ByteArrayContent(upload.Data);
        // Brauseri saadetud MIME tüüp ei määra salvestatud pildi tüüpi.
        data.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        content.Add(data, "Files", upload.Name);
    }
    return await client.PostAsync(path, content);
}

async Task CheckImage(Guid imageId, string contentType, byte[] data)
{
    var parentId = await context.KindergartenImages.Where(x => x.Id == imageId).Select(x => x.KindergartenId).SingleAsync();
    using var response = await client.GetAsync($"/Kindergarten/Image/{parentId}?imageId={imageId}");
    Check(response.StatusCode == HttpStatusCode.OK && response.Content.Headers.ContentType?.MediaType == contentType, "Image response has the wrong MIME type.");
    Check((await response.Content.ReadAsByteArrayAsync()).SequenceEqual(data), "Image response differs from the upload.");
    Check(response.Headers.GetValues("X-Content-Type-Options").Single() == "nosniff", "Missing nosniff header.");
}

static Dictionary<string, string> KindergartenFields(string name) => new()
{
    ["GroupName"] = name, ["ChildrenCount"] = "18", ["KindergartenName"] = "Paikese", ["TeacherName"] = "Mari"
};
static void Check(bool condition, string message)
{
    if (!condition) throw new InvalidOperationException(message);
}
static void Reject(Action action)
{
    try { action(); }
    catch (ValidationException) { return; }
    throw new InvalidOperationException("Invalid upload was accepted.");
}
static void Pass(string message) => Console.WriteLine("PASS: " + message);
