using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using TARge25Shop.Core.Dto;
using Xunit;

namespace TARge25Shop.KindergartenTest;

public class AdditionalKindergartenTest : TestBase
{
    // 1. Erinevad ID-d.
    [Fact]
    public async Task Should_AssignDifferentIds_WhenTwoGroupsAreCreated()
    {
        var firstDto = MockKindergartenData();
        var secondDto = MockKindergartenData();

        secondDto.GroupName = "Tahekesed";
        secondDto.ChildrenCount = 12;
        secondDto.KindergartenName = "Taheke";
        secondDto.TeacherName = "Kati";

        var first = await Service.Create(firstDto);
        var second = await Service.Create(secondDto);

        Context.ChangeTracker.Clear();

        Assert.NotEqual(Guid.Empty, first.Id);
        Assert.NotEqual(Guid.Empty, second.Id);
        Assert.NotEqual(first.Id, second.Id);
        Assert.Equal(2, Context.Kindergartens.Count());

        Assert.Equal(firstDto.GroupName,(await Service.DetailAsync(first.Id))!.GroupName);

        Assert.Equal(secondDto.GroupName,(await Service.DetailAsync(second.Id))!.GroupName);
    }

    // 2. ID määramine loomisel.
    [Fact]
    public async Task Should_IgnoreDtoId_WhenCreatingKindergarten()
    {
        var dto = MockKindergartenData();

        dto.Id = Guid.Parse(
            "11111111-1111-1111-1111-111111111111");

        var created = await Service.Create(dto);

        Context.ChangeTracker.Clear();

        Assert.NotEqual(Guid.Empty, created.Id);
        Assert.NotEqual(dto.Id, created.Id);
        Assert.NotNull(await Service.DetailAsync(created.Id));
        Assert.Null(await Service.DetailAsync(dto.Id));
    }

    // 3. Loomise ja muutmise kuupäevad.
    [Fact]
    public async Task Should_SetEqualDates_WhenKindergartenIsCreated()
    {
        var before = DateTime.Now;

        var created = await Service.Create(MockKindergartenData());

        var after = DateTime.Now;

        Context.ChangeTracker.Clear();

        var saved = await Service.DetailAsync(created.Id);

        Assert.NotNull(saved);
        Assert.NotEqual(default(DateTime), saved.CreatedAt);
        Assert.NotEqual(default(DateTime), saved.UpdatedAt);
        Assert.Equal(saved.CreatedAt, saved.UpdatedAt);
        Assert.InRange(saved.CreatedAt, before, after);
    }

    // 4. CreatedAt säilitamine.
    [Fact]
    public async Task Should_PreserveCreatedAt_WhenKindergartenIsUpdated()
    {
        var created = await Service.Create(MockKindergartenData());

        var originalCreatedAt = created.CreatedAt;
        var oldUpdatedAt = new DateTime(2000, 1, 1);

        created.UpdatedAt = oldUpdatedAt;

        await Context.SaveChangesAsync();

        Context.ChangeTracker.Clear();

        var dto = MockKindergartenData();

        dto.Id = created.Id;
        dto.GroupName = "Updated group";
        dto.CreatedAt = new DateTime(1990, 1, 1);

        await Service.Update(dto);

        Context.ChangeTracker.Clear();

        var saved = await Service.DetailAsync(created.Id);

        Assert.NotNull(saved);
        Assert.Equal(originalCreatedAt, saved.CreatedAt);
        Assert.True(saved.UpdatedAt > oldUpdatedAt);
    }

    // 5. Õpetaja nime muutmine.
    [Fact]
    public async Task Should_ChangeOnlyTeacherName_WhenTeacherIsUpdated()
    {
        var dto = MockKindergartenData();

        var created = await Service.Create(dto);

        Context.ChangeTracker.Clear();

        dto.Id = created.Id;
        dto.TeacherName = "New teacher";

        await Service.Update(dto);

        Context.ChangeTracker.Clear();

        var saved = await Service.DetailAsync(created.Id);

        Assert.NotNull(saved);
        Assert.Equal("New teacher", saved.TeacherName);
        Assert.Equal(created.GroupName, saved.GroupName);
        Assert.Equal(created.ChildrenCount, saved.ChildrenCount);
        Assert.Equal(created.KindergartenName, saved.KindergartenName);
        Assert.Equal(created.CreatedAt, saved.CreatedAt);
    }

    // 6. Teise ankeedi säilitamine.
    [Fact]
    public async Task Should_PreserveSecondGroup_WhenFirstGroupIsDeleted()
    {
        var first = await Service.Create(MockKindergartenData());

        var dto = MockKindergartenData();

        dto.GroupName = "Keep this group";
        dto.ChildrenCount = 9;
        dto.KindergartenName = "Other kindergarten";
        dto.TeacherName = "Other teacher";

        var second = await Service.Create(dto);

        var createdAt = second.CreatedAt;
        var updatedAt = second.UpdatedAt;

        Context.ChangeTracker.Clear();

        await Service.Delete(first.Id);

        Context.ChangeTracker.Clear();

        var saved = await Service.DetailAsync(second.Id);

        Assert.Null(await Service.DetailAsync(first.Id));
        Assert.NotNull(saved);
        Assert.Equal(dto.GroupName, saved.GroupName);
        Assert.Equal(dto.ChildrenCount, saved.ChildrenCount);
        Assert.Equal(dto.KindergartenName, saved.KindergartenName);
        Assert.Equal(dto.TeacherName, saved.TeacherName);
        Assert.Equal(createdAt, saved.CreatedAt);
        Assert.Equal(updatedAt, saved.UpdatedAt);
    }

    // 7. Kahe pildi salvestamine.
    [Fact]
    public async Task Should_SaveTwoDifferentImages_ForSameGroup()
    {
        var dto = MockKindergartenData(withImage: true);

        dto.Files.Add(GifImage());

        var created = await Service.Create(dto);

        Context.ChangeTracker.Clear();

        var stored = await Context.KindergartenImages
            .Where(x => x.KindergartenId == created.Id)
            .ToListAsync();

        var gallery = await Files.GetImagesAsync(created.Id);

        Assert.Equal(2, stored.Count);
        Assert.Equal(2, gallery.Count);

        Assert.Equal(2, stored.Select(x => x.Id).Distinct().Count());

        foreach (var file in dto.Files)
        {
            var image = Assert.Single(stored, x => x.FileName == file.FileName);

            Assert.Equal(created.Id, image.KindergartenId);
            Assert.Equal(file.Data, image.Data);
        }

        Assert.Contains(stored, x => x.ContentType == "image/png");

        Assert.Contains(stored, x => x.ContentType == "image/gif");
    }

    // 8. Vana pildi säilitamine.
    [Fact]
    public async Task Should_KeepOriginalImage_WhenNewImageIsAddedOnUpdate()
    {
        var created = await Service.Create(MockKindergartenData(withImage: true));

        var original = created.Images.Single();

        var oldImageId = original.Id;
        var oldImageData = original.Data.ToArray();

        Context.ChangeTracker.Clear();

        var dto = MockKindergartenData();

        dto.Id = created.Id;
        dto.Files.Add(GifImage());

        await Service.Update(dto);

        Context.ChangeTracker.Clear();

        var stored = await Context.KindergartenImages
            .Where(x => x.KindergartenId == created.Id)
            .ToListAsync();

        Assert.Equal(2, stored.Count);

        var oldImage = Assert.Single(stored, x => x.Id == oldImageId);

        Assert.Equal(oldImageData, oldImage.Data);

        var newImage = Assert.Single(stored, x => x.FileName == "second.gif");

        Assert.Equal(dto.Files[0].Data, newImage.Data);
    }

    // 9. Vale pildisignatuur.
    [Fact]
    public void Should_RejectPng_WhenSignatureIsInvalid()
    {
        var file = new FileUploadDto
        {
            FileName = "fake.png",
            Data = [1, 2, 3, 4]
        };

        Assert.Throws<ValidationException>(
            () => Files.PrepareImages([file]));

        Assert.Empty(Context.KindergartenImages);
    }

    // 10. Tühi pildifail.
    [Fact]
    public void Should_RejectImage_WhenDataIsEmpty()
    {
        var file = new FileUploadDto
        {
            FileName = "empty.png",
            Data = []
        };

        Assert.Throws<ValidationException>(
            () => Files.PrepareImages([file]));
    }

    // 11. Liiga suur pildifail.
    [Fact]
    public void Should_RejectImage_WhenSizeExceedsFiveMegabytes()
    {
        var file = SizedImage(FileUploadDto.MaxFileSize + 1);

        Assert.Throws<ValidationException>(
            () => Files.PrepareImages([file]));
    }

    // 12. Liiga palju pilte.
    [Fact]
    public void Should_RejectBatch_WhenElevenImagesAreProvided()
    {
        var files = Enumerable.Range(0, 11)
            .Select(i => MockImage($"image-{i}.png"))
            .ToList();

        Assert.Throws<ValidationException>(
            () => Files.PrepareImages(files));
    }

    // 13. Piltide kogumahu piirang.
    [Fact]
    public void Should_RejectBatch_WhenTotalSizeExceedsTwentyMegabytes()
    {
        var files = Enumerable.Range(0, 5)
            .Select(_ => SizedImage(FileUploadDto.MaxFileSize))
            .ToList();

        // Iga fail eraldi vastab nõuetele.
        foreach (var file in files)
        {
            Assert.Single(Files.PrepareImages([file]));
        }

        // Kõigi failide kogumaht on 25 MB.
        Assert.Throws<ValidationException>(
            () => Files.PrepareImages(files));
    }

    // 14. Teise ankeedi pildi kaitse.
    [Fact]
    public async Task Should_ProtectImage_WhenAnotherGroupIdIsUsed()
    {
        var first = await Service.Create(MockKindergartenData(withImage: true));

        var second = await Service.Create(MockKindergartenData(withImage: true));

        var imageId = first.Images.Single().Id;
        var secondImageId = second.Images.Single().Id;

        Context.ChangeTracker.Clear();

        var wrongRead = await Files.GetImageAsync(second.Id, imageId);

        var wrongDelete = await Files.DeleteImageAsync(second.Id,imageId);

        Context.ChangeTracker.Clear();

        Assert.Null(wrongRead);
        Assert.False(wrongDelete);

        Assert.NotNull(await Files.GetImageAsync(first.Id, imageId));

        Assert.NotNull(await Files.GetImageAsync(second.Id, secondImageId));

        Assert.Equal(2, Context.KindergartenImages.Count());
    }

    // 15. Ankeedi kustutamine koos piltidega.
    [Fact]
    public async Task Should_RemoveGroupAndAllLoadedImages_WhenGroupIsDeleted()
    {
        var dto = MockKindergartenData(withImage: true);

        dto.Files.Add(GifImage());
        dto.Files.Add(MockImage("third.png"));

        var created = await Service.Create(dto);

        Context.ChangeTracker.Clear();

        // InMemory ei kontrolli SQL-i kaskaadi:
        // laadime EF-i seosed.
        await Context.Kindergartens
            .Include(x => x.Images)
            .SingleAsync(x => x.Id == created.Id);

        await Service.Delete(created.Id);

        Context.ChangeTracker.Clear();

        Assert.Null(await Service.DetailAsync(created.Id));
        Assert.Empty(Context.KindergartenImages);

        Assert.Empty(await Files.GetImagesAsync(created.Id));
    }

    private static FileUploadDto GifImage() => new()
    {
        FileName = "second.gif",

        Data = Convert.FromBase64String("R0lGODlhAQABAIAAAAAAAP///yH5BAEAAAAALAAAAAABAAEAAAICRAEAOw==")
    };

    private static FileUploadDto SizedImage(int size)
    {
        var file = MockImage();
        var data = new byte[size];

        file.Data.CopyTo(data, 0);
        file.Data = data;

        return file;
    }
}