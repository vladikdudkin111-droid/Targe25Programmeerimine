using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using TARge25Shop.Core.ServiceInterface;
using TARge25Shop.Data;
using Xunit;

namespace TARge25Shop.Tests;

public sealed class KindergartenTest : TestBase
{
    // SpaceShip näite järgi: loomine, ID järgi otsimine, muutmine ja kustutamine.
    [Fact]
    // Test kontrollib rühma loomist ja kõigi põhiandmete salvestamist.
    public async Task Should_CreateKindergarten_WhenDataIsProvided()
    {
        var dto = Group();

        var result = await Svc<IKindergartenServices>().Create(dto);

        Assert.NotEqual(Guid.Empty, result.Id);
        using var verify = NewContext();
        var saved = await verify.Kindergartens.SingleAsync();
        Assert.Equal(dto.GroupName, saved.GroupName);
        Assert.Equal(dto.KindergartenName, saved.KindergartenName);
        Assert.Equal(dto.TeacherName, saved.TeacherName);
        Assert.Equal(dto.ChildrenCount, saved.ChildrenCount);
        Assert.Equal(result.Id, saved.Id);
    }

    [Fact]
    // Test kontrollib, et vale ID järgi rühma ei leita.
    public async Task ShouldNot_FindKindergarten_WhenIdIsDifferent()
    {
        await Svc<IKindergartenServices>().Create(Group());

        var result = await Svc<IKindergartenServices>().DetailAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    // Test kontrollib rühma leidmist ja selle andmete õigsust.
    public async Task Should_FindKindergarten_WhenIdExists()
    {
        var created = await Svc<IKindergartenServices>().Create(Group());
        Svc<TARge25ShopContext>().ChangeTracker.Clear();

        var result = await Svc<IKindergartenServices>().DetailAsync(created.Id);

        Assert.NotNull(result);
        Assert.Equal(created.Id, result.Id);
        Assert.Equal(created.GroupName, result.GroupName);
        Assert.Equal(created.ChildrenCount, result.ChildrenCount);
        Assert.Equal(created.KindergartenName, result.KindergartenName);
        Assert.Equal(created.TeacherName, result.TeacherName);
    }

    [Fact]
    // Test kontrollib, et kustutamine tagastab kustutatud rühma andmed.
    public async Task Should_ReturnDeletedKindergarten_WhenIdExists()
    {
        var created = await Svc<IKindergartenServices>().Create(Group());

        var result = await Svc<IKindergartenServices>().Delete(created.Id);

        Assert.NotNull(result);
        Assert.Equal(created.Id, result.Id);
        Assert.Equal(created.GroupName, result.GroupName);
    }

    [Fact]
    // Test kontrollib, et ühe rühma kustutamisel jääb teine rühm alles.
    public async Task Should_KeepOtherKindergarten_WhenDeletingOneGroup()
    {
        var first = await Svc<IKindergartenServices>().Create(Group());
        var second = await Svc<IKindergartenServices>().Create(Group());

        await Svc<IKindergartenServices>().Delete(second.Id);

        using var verify = NewContext();
        Assert.Equal(first.Id, (await verify.Kindergartens.SingleAsync()).Id);
    }

    [Fact]
    // Test kontrollib kõigi muudetavate väljade salvestamist.
    public async Task Should_UpdateAllFields_WhenNewDataIsProvided()
    {
        var created = await Svc<IKindergartenServices>().Create(Group());
        var dto = Group();
        dto.Id = created.Id;
        dto.GroupName = "Tähekesed";
        dto.KindergartenName = "Metsatuka lasteaed";
        dto.TeacherName = "Liis Kask";
        dto.ChildrenCount = 23;

        var result = await Svc<IKindergartenServices>().Update(dto);

        Assert.NotNull(result);
        using var verify = NewContext();
        var saved = await verify.Kindergartens.SingleAsync();
        Assert.Equal(dto.Id, saved.Id);
        Assert.Equal(dto.GroupName, saved.GroupName);
        Assert.Equal(dto.KindergartenName, saved.KindergartenName);
        Assert.Equal(dto.TeacherName, saved.TeacherName);
        Assert.Equal(dto.ChildrenCount, saved.ChildrenCount);
    }

    [Fact]
    // Test kontrollib, et puuduva ID-ga uuendamine ei muuda olemasolevat rühma.
    public async Task ShouldNot_UpdateKindergarten_WhenIdDoesNotExist()
    {
        var created = await Svc<IKindergartenServices>().Create(Group());
        var dto = Group();
        dto.Id = Guid.NewGuid();
        dto.GroupName = "Puuduv rühm";

        var result = await Svc<IKindergartenServices>().Update(dto);

        Assert.Null(result);
        using var verify = NewContext();
        var saved = await verify.Kindergartens.SingleAsync();
        Assert.Equal(created.Id, saved.Id);
        Assert.Equal(created.GroupName, saved.GroupName);
    }

    [Fact]
    // Test kontrollib, et loomise ja muutmise aja määrab server.
    public async Task Should_SetServerDates_WhenCreatingKindergarten()
    {
        var dto = Group();
        dto.CreatedAt = new DateTime(1995, 5, 5);
        dto.UpdatedAt = DateTime.MaxValue;
        var before = DateTime.Now;

        var result = await Svc<IKindergartenServices>().Create(dto);

        Assert.InRange(result.CreatedAt, before, DateTime.Now);
        Assert.Equal(result.CreatedAt, result.UpdatedAt);
        using var verify = NewContext();
        Assert.Equal(result.CreatedAt, (await verify.Kindergartens.SingleAsync()).CreatedAt);
    }

    [Fact]
    // Test kontrollib, et samadest sisendandmetest loodud rühmad saavad erinevad ID-d.
    public async Task Should_GenerateDifferentIds_WhenCreatingFromSameDto()
    {
        var dto = Group();
        dto.Id = Guid.NewGuid();

        var first = await Svc<IKindergartenServices>().Create(dto);
        var second = await Svc<IKindergartenServices>().Create(dto);

        Assert.NotEqual(first.Id, second.Id);
        Assert.NotEqual(dto.Id, first.Id);
        Assert.NotEqual(dto.Id, second.Id);
        using var verify = NewContext();
        Assert.Equal(2, await verify.Kindergartens.CountAsync());
    }

    [Fact]
    // Test kontrollib, et kustutatud rühma enam ei leita.
    public async Task ShouldNot_FindKindergarten_WhenItHasBeenDeleted()
    {
        var created = await Svc<IKindergartenServices>().Create(Group());
        await Svc<IKindergartenServices>().Delete(created.Id);
        Svc<TARge25ShopContext>().ChangeTracker.Clear();

        var result = await Svc<IKindergartenServices>().DetailAsync(created.Id);

        Assert.Null(result);
        using var verify = NewContext();
        Assert.Empty(await verify.Kindergartens.ToListAsync());
    }

    [Fact]
    // Test kontrollib, et tühja ID-ga uuendamine ei muuda olemasolevat rühma.
    public async Task ShouldNot_UpdateKindergarten_WhenIdIsEmpty()
    {
        var created = await Svc<IKindergartenServices>().Create(Group());
        var dto = Group();
        dto.GroupName = "Ei salvestata";

        var result = await Svc<IKindergartenServices>().Update(dto);

        Assert.Null(result);
        using var verify = NewContext();
        Assert.Equal(created.GroupName, (await verify.Kindergartens.SingleAsync()).GroupName);
    }

    // 15 enda väljamõeldud lisatesti.
    [Fact, Trait("Origin", "Original")]
    // Test kontrollib rühma loomist ilma piltideta.
    public async Task Should_CreateWithoutImages_WhenNoFilesAreProvided()
    {
        var created = await Svc<IKindergartenServices>().Create(Group());

        using var verify = NewContext();
        Assert.True(await verify.Kindergartens.AnyAsync(x => x.Id == created.Id));
        Assert.Empty(await verify.KindergartenImages.ToListAsync());
    }

    [Fact, Trait("Origin", "Original")]
    // Test kontrollib piltide sisu salvestamist ja piltide seost loodud rühmaga.
    public async Task Should_SaveImageBytesAndOwner_WhenCreatingWithFiles()
    {
        var first = Png("esimene.png");
        var second = Png("teine.png");

        var created = await Svc<IKindergartenServices>().Create(Group(first, second));

        using var verify = NewContext();
        var images = await verify.KindergartenImages.ToListAsync();
        Assert.Equal(2, images.Count);
        Assert.All(images, x => Assert.Equal(created.Id, x.KindergartenId));
        Assert.Equal(first.Data, Assert.Single(images, x => x.FileName == first.FileName).Data);
        Assert.Equal(second.Data, Assert.Single(images, x => x.FileName == second.FileName).Data);
    }

    [Fact, Trait("Origin", "Original")]
    // Test kontrollib, et vigase pildi korral ei salvestata rühma ega teisi pilte.
    public async Task ShouldNot_SavePartialCreate_WhenOneImageIsInvalid()
    {
        var bad = Png("vale.txt");

        await Assert.ThrowsAsync<ValidationException>(() =>
            Svc<IKindergartenServices>().Create(Group(Png(), bad)));

        using var verify = NewContext();
        Assert.Empty(await verify.Kindergartens.ToListAsync());
        Assert.Empty(await verify.KindergartenImages.ToListAsync());
    }

    [Fact, Trait("Origin", "Original")]
    // Test kontrollib, et uuendamine ei muuda rühma loomise aega.
    public async Task Should_PreserveCreatedAt_WhenUpdateSuppliesAnotherDate()
    {
        var created = await Svc<IKindergartenServices>().Create(Group());
        var originalDate = created.CreatedAt;
        var dto = Group();
        dto.Id = created.Id;
        dto.CreatedAt = DateTime.MaxValue;

        await Svc<IKindergartenServices>().Update(dto);

        using var verify = NewContext();
        Assert.Equal(originalDate, (await verify.Kindergartens.SingleAsync()).CreatedAt);
    }

    [Fact, Trait("Origin", "Original")]
    // Test kontrollib, et uuendamisel määrab server uue muutmise aja.
    public async Task Should_AdvanceUpdatedAt_WhenUpdatingExistingGroup()
    {
        var created = await Svc<IKindergartenServices>().Create(Group());
        created.UpdatedAt = new DateTime(2020, 1, 1);
        await Svc<TARge25ShopContext>().SaveChangesAsync();
        var dto = Group();
        dto.Id = created.Id;
        dto.UpdatedAt = DateTime.MaxValue;
        var before = DateTime.Now;

        await Svc<IKindergartenServices>().Update(dto);

        using var verify = NewContext();
        Assert.InRange((await verify.Kindergartens.SingleAsync()).UpdatedAt, before, DateTime.Now);
    }

    [Fact, Trait("Origin", "Original")]
    // Test kontrollib, et failideta uuendamisel jäävad vanad pildid alles.
    public async Task Should_KeepImages_WhenUpdatingWithoutNewFiles()
    {
        var created = await Svc<IKindergartenServices>().Create(Group(Png()));
        var imageId = Assert.Single(created.Images).Id;
        var dto = Group();
        dto.Id = created.Id;
        dto.ChildrenCount = 21;

        await Svc<IKindergartenServices>().Update(dto);

        using var verify = NewContext();
        Assert.Equal(imageId, (await verify.KindergartenImages.SingleAsync()).Id);
        Assert.Equal(21, (await verify.Kindergartens.SingleAsync()).ChildrenCount);
    }

    [Fact, Trait("Origin", "Original")]
    // Test kontrollib uue pildi lisamist olemasoleva pildi kõrvale.
    public async Task Should_AppendImages_WhenUpdatingWithNewFiles()
    {
        var created = await Svc<IKindergartenServices>().Create(Group(Png("vana.png")));
        var originalImageId = Assert.Single(created.Images).Id;
        var file = Png("uus.png");
        var dto = Group(file);
        dto.Id = created.Id;

        await Svc<IKindergartenServices>().Update(dto);

        using var verify = NewContext();
        var images = await verify.KindergartenImages.ToListAsync();
        Assert.Equal(2, images.Count);
        Assert.Contains(images, x => x.Id == originalImageId);
        Assert.Equal(file.Data, Assert.Single(images, x => x.FileName == file.FileName).Data);
        Assert.All(images, x => Assert.Equal(created.Id, x.KindergartenId));
    }

    [Fact, Trait("Origin", "Original")]
    // Test kontrollib, et vigase pildiga uuendamine jätab andmed ja vanad pildid muutmata.
    public async Task ShouldNot_ChangeStoredData_WhenUpdateContainsInvalidImage()
    {
        var created = await Svc<IKindergartenServices>().Create(Group(Png("vana.png")));
        var originalDate = created.UpdatedAt;
        var dto = Group(Png("uus.png"), Png("vale.exe"));
        dto.Id = created.Id;
        dto.GroupName = "Ei salvestata";
        dto.ChildrenCount = 99;

        await Assert.ThrowsAsync<ValidationException>(() => Svc<IKindergartenServices>().Update(dto));

        using var verify = NewContext();
        var saved = await verify.Kindergartens.SingleAsync();
        Assert.Equal(created.GroupName, saved.GroupName);
        Assert.Equal(created.ChildrenCount, saved.ChildrenCount);
        Assert.Equal(originalDate, saved.UpdatedAt);
        Assert.Equal("vana.png", (await verify.KindergartenImages.SingleAsync()).FileName);
    }

    [Fact, Trait("Origin", "Original")]
    // Test kontrollib, et puuduva rühma uuendamisel ei salvestata pilte.
    public async Task ShouldNot_AddOrphanImages_WhenUpdatingMissingGroup()
    {
        var dto = Group(Png());
        dto.Id = Guid.NewGuid();

        Assert.Null(await Svc<IKindergartenServices>().Update(dto));

        using var verify = NewContext();
        Assert.Empty(await verify.Kindergartens.ToListAsync());
        Assert.Empty(await verify.KindergartenImages.ToListAsync());
    }

    [Fact, Trait("Origin", "Original")]
    // Test kontrollib, et tühja ID-ga otsing ei tagasta olemasolevat rühma.
    public async Task ShouldNot_ReturnExistingGroup_WhenLookingUpEmptyGuid()
    {
        var created = await Svc<IKindergartenServices>().Create(Group());

        Assert.Null(await Svc<IKindergartenServices>().DetailAsync(Guid.Empty));

        using var verify = NewContext();
        Assert.Equal(created.Id, (await verify.Kindergartens.SingleAsync()).Id);
    }

    [Fact, Trait("Origin", "Original")]
    // Test kontrollib, et tundmatu ID-ga kustutamine jätab olemasoleva rühma ja pildi alles.
    public async Task ShouldNot_DeleteExistingData_WhenDeleteIdIsUnknown()
    {
        var created = await Svc<IKindergartenServices>().Create(Group(Png()));

        Assert.Null(await Svc<IKindergartenServices>().Delete(Guid.NewGuid()));

        using var verify = NewContext();
        Assert.Equal(created.Id, (await verify.Kindergartens.SingleAsync()).Id);
        Assert.Equal(created.Id, (await verify.KindergartenImages.SingleAsync()).KindergartenId);
    }

    [Fact, Trait("Origin", "Original")]
    // Test kontrollib, et sama rühma teistkordne kustutamine tagastab nulli.
    public async Task Should_ReturnNull_WhenDeletingSameGroupTwice()
    {
        var created = await Svc<IKindergartenServices>().Create(Group());
        Assert.NotNull(await Svc<IKindergartenServices>().Delete(created.Id));

        Assert.Null(await Svc<IKindergartenServices>().Delete(created.Id));

        using var verify = NewContext();
        Assert.Empty(await verify.Kindergartens.ToListAsync());
    }

    [Fact, Trait("Origin", "Original")]
    // Test kontrollib, et rühma kustutamine eemaldab ka mällu laadimata pildid.
    public async Task Should_CascadeDeleteImages_WhenImagesAreNotTracked()
    {
        var created = await Svc<IKindergartenServices>().Create(Group(Png("a.png"), Png("b.png")));
        Svc<TARge25ShopContext>().ChangeTracker.Clear();

        await Svc<IKindergartenServices>().Delete(created.Id);

        using var verify = NewContext();
        Assert.Empty(await verify.Kindergartens.ToListAsync());
        Assert.Empty(await verify.KindergartenImages.ToListAsync());
    }

    [Fact, Trait("Origin", "Original")]
    // Test kontrollib, et rühma kustutamine ei eemalda teise rühma pilte.
    public async Task Should_KeepOtherGroupsImages_WhenDeletingGroupWithImages()
    {
        var first = await Svc<IKindergartenServices>().Create(Group(Png("esimene.png")));
        var second = await Svc<IKindergartenServices>().Create(Group(Png("teine.png")));
        Svc<TARge25ShopContext>().ChangeTracker.Clear();

        await Svc<IKindergartenServices>().Delete(first.Id);

        using var verify = NewContext();
        Assert.Equal(second.Id, (await verify.Kindergartens.SingleAsync()).Id);
        var saved = await verify.KindergartenImages.SingleAsync();
        Assert.Equal(second.Id, saved.KindergartenId);
        Assert.Equal("teine.png", saved.FileName);
        Assert.Equal(Png().Data, saved.Data);
    }

    [Fact, Trait("Origin", "Original")]
    // Test kontrollib, et samade andmetega uuendamine ei loo uut rühma.
    public async Task Should_NotDuplicateGroup_WhenUpdatingWithUnchangedFields()
    {
        var created = await Svc<IKindergartenServices>().Create(Group());
        var dto = Group();
        dto.Id = created.Id;

        var result = await Svc<IKindergartenServices>().Update(dto);

        Assert.NotNull(result);
        using var verify = NewContext();
        var saved = await verify.Kindergartens.SingleAsync();
        Assert.Equal(created.Id, saved.Id);
        Assert.Equal(dto.GroupName, saved.GroupName);
        Assert.Equal(dto.ChildrenCount, saved.ChildrenCount);
        Assert.Equal(dto.TeacherName, saved.TeacherName);
        Assert.Equal(dto.KindergartenName, saved.KindergartenName);
    }
}
