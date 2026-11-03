using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace TARge25Shop.KindergartenTest;

public class KindergartenTest : TestBase
{
    // 1. Loomine: tulemus ei tohi olla null.
    [Fact]
    public async Task ShouldNot_ReturnNull_WhenKindergartenIsCreated()
    {
        var result = await Service.Create(MockKindergartenData());

        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
    }

    // 2. Vale ID-ga otsing.
    [Fact]
    public async Task ShouldNot_GetKindergarten_WhenIdDoesNotExist()
    {
        await Service.Create(MockKindergartenData());

        Context.ChangeTracker.Clear();

        var result = await Service.DetailAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    // 3. Olemasoleva ID-ga otsing.
    [Fact]
    public async Task Should_GetKindergarten_WhenIdIsEqual()
    {
        var created = await Service.Create(MockKindergartenData());

        Context.ChangeTracker.Clear();

        var result = await Service.DetailAsync(created.Id);

        Assert.NotNull(result);
        Assert.Equal(created.Id, result.Id);
        Assert.Equal(created.GroupName, result.GroupName);
    }

    // 4. Kustutatud objekti tagastamine.
    [Fact]
    public async Task Should_ReturnDeletedKindergarten_WhenIdIsEqual()
    {
        var created = await Service.Create(MockKindergartenData());

        var deleted = await Service.Delete(created.Id);

        Assert.NotNull(deleted);
        Assert.Equal(created.Id, deleted.Id);
        Assert.Equal(created.GroupName, deleted.GroupName);
    }

    // 5. Tundmatu ID ei tohi olemasolevat ankeeti kustutada.
    [Fact]
    public async Task ShouldNot_DeleteKindergarten_WhenIdDoesNotExist()
    {
        var created = await Service.Create(MockKindergartenData());

        var result = await Service.Delete(Guid.NewGuid());

        Context.ChangeTracker.Clear();

        Assert.Null(result);
        Assert.NotNull(await Service.DetailAsync(created.Id));
        Assert.Equal(1, Context.Kindergartens.Count());
    }

    // 6. Kõigi muudetavate andmete uuendamine.
    [Fact]
    public async Task Should_UpdateKindergarten_WhenDataIsChanged()
    {
        var created = await Service.Create(MockKindergartenData());

        var dto = MockKindergartenData();

        dto.Id = created.Id;
        dto.GroupName = "Tahekesed";
        dto.ChildrenCount = 25;
        dto.KindergartenName = "Taheke";
        dto.TeacherName = "Kati";

        Context.ChangeTracker.Clear();

        var result = await Service.Update(dto);

        Context.ChangeTracker.Clear();

        var saved = await Service.DetailAsync(created.Id);

        Assert.NotNull(result);
        Assert.NotNull(saved);
        Assert.Equal(created.Id, saved.Id);
        Assert.Equal(dto.GroupName, saved.GroupName);
        Assert.Equal(dto.ChildrenCount, saved.ChildrenCount);
        Assert.Equal(dto.KindergartenName, saved.KindergartenName);
        Assert.Equal(dto.TeacherName, saved.TeacherName);
    }

    // 7. Tühja ID-ga uuendamine.
    [Fact]
    public async Task ShouldNot_UpdateKindergarten_WhenIdIsEmpty()
    {
        var created = await Service.Create(MockKindergartenData());

        var dto = MockKindergartenData();

        dto.Id = Guid.Empty;
        dto.GroupName = "Must not be saved";

        var result = await Service.Update(dto);

        Context.ChangeTracker.Clear();

        var saved = await Service.DetailAsync(created.Id);

        Assert.Null(result);
        Assert.NotNull(saved);
        Assert.Equal(created.GroupName, saved.GroupName);
        Assert.Equal(1, Context.Kindergartens.Count());
    }

    // 8. Kindergartenis kontrollime keelatud pildiformaati.
    [Fact]
    public async Task ShouldNot_CreateKindergarten_WhenImageFormatIsUnsupported()
    {
        var dto = MockKindergartenData();

        dto.Files.Add(MockImage("forbidden.svg"));

        await Assert.ThrowsAsync<ValidationException>(
            () => Service.Create(dto));

        Context.ChangeTracker.Clear();

        Assert.Empty(Context.Kindergartens);
        Assert.Empty(Context.KindergartenImages);
    }

    // 9. Vigane fail ei tohi uuendamisel osalisi muudatusi salvestada.
    [Fact]
    public async Task ShouldNot_SaveUpdate_WhenImageIsInvalid()
    {
        var created = await Service.Create(
            MockKindergartenData(withImage: true));

        var originalName = created.GroupName;
        var originalDate = created.UpdatedAt;
        var originalImageId = created.Images.Single().Id;

        Context.ChangeTracker.Clear();

        var dto = MockKindergartenData();

        dto.Id = created.Id;
        dto.GroupName = "Must not be saved";

        dto.Files.Add(MockImage("valid.png"));
        dto.Files.Add(MockImage("forbidden.svg"));

        await Assert.ThrowsAsync<ValidationException>(
            () => Service.Update(dto));

        Context.ChangeTracker.Clear();

        var saved = await Service.DetailAsync(created.Id);

        Assert.NotNull(saved);
        Assert.Equal(originalName, saved.GroupName);
        Assert.Equal(originalDate, saved.UpdatedAt);

        Assert.Equal(
            originalImageId,
            (await Context.KindergartenImages.SingleAsync()).Id);
    }

    // 10. Kustutatud ankeet peab andmebaasist kaduma.
    [Fact]
    public async Task Should_RemoveKindergartenFromDatabase_WhenDeleted()
    {
        var created = await Service.Create(MockKindergartenData());

        await Service.Delete(created.Id);

        Context.ChangeTracker.Clear();

        Assert.Null(await Service.DetailAsync(created.Id));
        Assert.Empty(Context.Kindergartens);
    }

    // 11. Ühe pildi kustutamine peab ankeedi ja teise pildi säilitama.
    [Fact]
    public async Task Should_RemoveOnlySelectedImage_WhenImageIsDeleted()
    {
        var dto = MockKindergartenData(withImage: true);

        dto.Files.Add(MockImage("keep.png"));

        var group = await Service.Create(dto);

        var imageId = group.Images
            .Single(x => x.FileName == "test.png")
            .Id;

        var oldDate = new DateTime(2000, 1, 1);

        group.UpdatedAt = oldDate;

        await Context.SaveChangesAsync();

        Context.ChangeTracker.Clear();

        var result = await Files.DeleteImageAsync(group.Id, imageId);

        Context.ChangeTracker.Clear();

        var saved = await Service.DetailAsync(group.Id);
        var remaining = await Files.GetImagesAsync(group.Id);

        Assert.True(result);
        Assert.NotNull(saved);
        Assert.True(saved.UpdatedAt > oldDate);

        Assert.Null(
            await Files.GetImageAsync(group.Id, imageId));

        Assert.Equal(
            "keep.png",
            Assert.Single(remaining).FileName);
    }
}