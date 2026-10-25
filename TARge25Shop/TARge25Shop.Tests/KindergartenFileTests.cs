using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using TARge25Shop.Core.Dto;
using TARge25Shop.Core.ServiceInterface;
using TARge25Shop.Data;
using Xunit;

namespace TARge25Shop.Tests;

public sealed class KindergartenFileTests : TestBase
{
    public static TheoryData<string, byte[], string> SupportedImages => new()
    {
        { "pilt.PNG", Png().Data, "image/png" },
        { "pilt.jpg", [255, 216, 255, 224], "image/jpeg" },
        { "pilt.jpeg", [255, 216, 255, 224], "image/jpeg" },
        { "pilt.gif", "GIF87a"u8.ToArray(), "image/gif" },
        { "pilt.gif", "GIF89a"u8.ToArray(), "image/gif" },
        { "pilt.webp", "RIFF\0\0\0\0WEBP"u8.ToArray(), "image/webp" }
    };

    [Theory, MemberData(nameof(SupportedImages))]
    // Test kontrollib toetatud pildi sisu, failitüüpi, ID-d ja lisamise aega.
    public void Should_PrepareSupportedImage_WhenSignatureMatches(string name, byte[] bytes, string mime)
    {
        var before = DateTime.Now;

        var image = Assert.Single(Svc<IFileService>().PrepareImages(
            [new FileUploadDto { FileName = name, Data = bytes }]));

        Assert.NotEqual(Guid.Empty, image.Id);
        Assert.Equal(name, image.FileName);
        Assert.Equal(bytes, image.Data);
        Assert.Equal(mime, image.ContentType);
        Assert.InRange(image.CreatedAt, before, DateTime.Now);
        Assert.Empty(Svc<TARge25ShopContext>().ChangeTracker.Entries());
    }

    [Theory]
    [InlineData("pilt.png", "vale")]
    [InlineData("pilt.jpg", "vale")]
    [InlineData("pilt.gif", "GIF90a")]
    [InlineData("pilt.webp", "RIFF")]
    [InlineData("pilt.webp", "RIFF0000NOPE")]
    [InlineData("pilt.svg", "<svg/>")]
    // Test kontrollib vale sisu või lubamatu laiendiga faili tagasilükkamist.
    public void Should_RejectFile_WhenSignatureOrExtensionIsInvalid(string name, string content)
    {
        var dto = new FileUploadDto { FileName = name, Data = System.Text.Encoding.UTF8.GetBytes(content) };

        Assert.Throws<ValidationException>(() => Svc<IFileService>().PrepareImages([dto]));
    }

    [Fact]
    // Test kontrollib, et failideta sisend annab tühja piltide nimekirja.
    public void Should_ReturnEmptyList_WhenNoImagesAreProvided()
    {
        Assert.Empty(Svc<IFileService>().PrepareImages([]));
    }

    [Fact]
    // Test kontrollib tühja pildifaili tagasilükkamist.
    public void Should_RejectEmptyFile_WhenImageHasNoBytes()
    {
        Assert.Throws<ValidationException>(() => Svc<IFileService>().PrepareImages([Png(size: 0)]));
    }

    [Fact]
    // Test kontrollib üle 5 MB suuruse pildifaili tagasilükkamist.
    public void Should_RejectOversizedImage_WhenSingleFileLimitIsExceeded()
    {
        Assert.Throws<ValidationException>(() => Svc<IFileService>().PrepareImages(
            [Png(size: FileUploadDto.MaxFileSize + 1)]));
    }

    [Fact]
    // Test kontrollib täpselt 5 MB suuruse pildifaili lubamist.
    public void Should_AcceptImage_WhenSizeEqualsSingleFileLimit()
    {
        var image = Assert.Single(Svc<IFileService>().PrepareImages([Png(size: FileUploadDto.MaxFileSize)]));

        Assert.Equal(FileUploadDto.MaxFileSize, image.Data.Length);
    }

    [Fact]
    // Test kontrollib rohkem kui kümne pildi korraga lisamise keelamist.
    public void Should_RejectTooManyImages_WhenFileCountLimitIsExceeded()
    {
        var files = Enumerable.Range(0, FileUploadDto.MaxFileCount + 1).Select(_ => Png()).ToList();

        Assert.Throws<ValidationException>(() => Svc<IFileService>().PrepareImages(files));
    }

    [Fact]
    // Test kontrollib kümne pildi lubamist ja nende ID-de erinevust.
    public void Should_AcceptTenImages_WhenFileCountEqualsLimit()
    {
        var files = Enumerable.Range(0, FileUploadDto.MaxFileCount).Select(_ => Png()).ToList();

        var images = Svc<IFileService>().PrepareImages(files);

        Assert.Equal(FileUploadDto.MaxFileCount, images.Count);
        Assert.Equal(images.Count, images.Select(x => x.Id).Distinct().Count());
    }

    [Fact]
    // Test kontrollib piltide tagasilükkamist kogumahu piiri ületamisel ühe baidi võrra.
    public void Should_RejectBatch_WhenTotalSizeExceedsLimitByOneByte()
    {
        var files = Enumerable.Range(0, 4).Select(_ => Png(size: FileUploadDto.MaxFileSize)).ToList();
        files.Add(new FileUploadDto { FileName = "extra.png", Data = [137] });

        Assert.Throws<ValidationException>(() => Svc<IFileService>().PrepareImages(files));
    }

    [Fact]
    // Test kontrollib täpselt 20 MB kogumahuga piltide lubamist.
    public void Should_AcceptBatch_WhenTotalSizeEqualsLimit()
    {
        var files = Enumerable.Range(0, 4).Select(_ => Png(size: FileUploadDto.MaxFileSize)).ToList();

        var images = Svc<IFileService>().PrepareImages(files);

        Assert.Equal(4, images.Count);
        Assert.Equal(FileUploadDto.MaxTotalSize, images.Sum(x => x.Data.Length));
    }

    [Theory]
    [InlineData("C:\\fakepath\\foto.PNG", "foto.PNG")]
    [InlineData("../../foto.png", "foto.png")]
    // Test kontrollib kaustatee eemaldamist pildi failinimest.
    public void Should_RemoveDirectoryFromName_WhenPreparingImage(string suppliedName, string expectedName)
    {
        var image = Assert.Single(Svc<IFileService>().PrepareImages([Png(suppliedName)]));

        Assert.Equal(expectedName, image.FileName);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("folder/")]
    // Test kontrollib puuduva või tühja failinime tagasilükkamist.
    public void Should_RejectName_WhenFileNameIsMissing(string name)
    {
        Assert.Throws<ValidationException>(() => Svc<IFileService>().PrepareImages([Png(name)]));
    }

    [Theory]
    [InlineData(255, true)]
    [InlineData(256, false)]
    // Test kontrollib 255 märgi pikkuse failinime lubamist ja pikema nime keelamist.
    public void Should_CheckFileNameLength_WhenPreparingImage(int length, bool valid)
    {
        var file = Png(new string('a', length - 4) + ".png");
        if (valid)
            Assert.Equal(length, Assert.Single(Svc<IFileService>().PrepareImages([file])).FileName.Length);
        else
            Assert.Throws<ValidationException>(() => Svc<IFileService>().PrepareImages([file]));
    }

    [Fact]
    // Test kontrollib ainult valitud rühma piltide tagastamist loomise aja ja ID järjekorras.
    public async Task Should_ListOnlyOwnersImages_InCreationOrderThenId()
    {
        var owner = await Svc<IKindergartenServices>().Create(Group(Png("a.png"), Png("b.png"), Png("c.png")));
        await Svc<IKindergartenServices>().Create(Group(Png("võõras.png")));
        var images = owner.Images.OrderBy(x => x.Id).ToArray();
        images[0].CreatedAt = new DateTime(2022, 1, 1);
        images[1].CreatedAt = images[2].CreatedAt = new DateTime(2021, 1, 1);
        await Svc<TARge25ShopContext>().SaveChangesAsync();
        var expected = images.OrderBy(x => x.CreatedAt).ThenBy(x => x.Id).Select(x => x.Id).ToArray();
        Svc<TARge25ShopContext>().ChangeTracker.Clear();

        var result = await Svc<IFileService>().GetImagesAsync(owner.Id);

        Assert.Equal(expected, result.Select(x => x.Id));
        Assert.DoesNotContain(result, x => x.FileName == "võõras.png");
        Assert.Empty(Svc<TARge25ShopContext>().ChangeTracker.Entries());
        Assert.Empty(await Svc<IFileService>().GetImagesAsync(Guid.NewGuid()));
    }

    [Fact]
    // Test kontrollib õigele rühmale kuuluva pildi sisu ja failitüübi tagastamist.
    public async Task Should_GetImageBytes_WhenOwnerAndImageIdMatch()
    {
        var owner = await Svc<IKindergartenServices>().Create(Group(Png()));
        var imageId = Assert.Single(owner.Images).Id;
        Svc<TARge25ShopContext>().ChangeTracker.Clear();

        var image = await Svc<IFileService>().GetImageAsync(owner.Id, imageId);

        Assert.NotNull(image);
        Assert.Equal(Png().Data, image.Data);
        Assert.Equal("image/png", image.ContentType);
        Assert.Empty(Svc<TARge25ShopContext>().ChangeTracker.Entries());
    }

    [Fact]
    // Test kontrollib, et võõra rühma või puuduva ID-ga pilti ei tagastata.
    public async Task ShouldNot_GetImage_WhenOwnerIsDifferentOrImageIsMissing()
    {
        var owner = await Svc<IKindergartenServices>().Create(Group(Png()));
        var other = await Svc<IKindergartenServices>().Create(Group());
        var imageId = Assert.Single(owner.Images).Id;

        Assert.Null(await Svc<IFileService>().GetImageAsync(other.Id, imageId));
        Assert.Null(await Svc<IFileService>().GetImageAsync(owner.Id, Guid.NewGuid()));
    }

    [Fact]
    // Test kontrollib ainult valitud pildi kustutamist ja selle rühma muutmise aja uuendamist.
    public async Task Should_DeleteOnlySelectedImage_AndUpdateOwnersTimestamp()
    {
        var owner = await Svc<IKindergartenServices>().Create(Group(Png("a.png"), Png("b.png")));
        var other = await Svc<IKindergartenServices>().Create(Group(Png("other.png")));
        var originalCreatedAt = owner.CreatedAt;
        owner.UpdatedAt = other.UpdatedAt = new DateTime(2020, 1, 1);
        var imageId = owner.Images.First().Id;
        await Svc<TARge25ShopContext>().SaveChangesAsync();
        Svc<TARge25ShopContext>().ChangeTracker.Clear();
        var before = DateTime.Now;

        Assert.True(await Svc<IFileService>().DeleteImageAsync(owner.Id, imageId));

        using var verify = NewContext();
        Assert.False(await verify.KindergartenImages.AnyAsync(x => x.Id == imageId));
        Assert.Equal(2, await verify.KindergartenImages.CountAsync());
        Assert.Equal(2, await verify.Kindergartens.CountAsync());
        var saved = await verify.Kindergartens.SingleAsync(x => x.Id == owner.Id);
        Assert.Equal(originalCreatedAt, saved.CreatedAt);
        Assert.InRange(saved.UpdatedAt, before, DateTime.Now);
        Assert.Equal(new DateTime(2020, 1, 1), (await verify.Kindergartens.SingleAsync(x => x.Id == other.Id)).UpdatedAt);
    }

    [Fact]
    // Test kontrollib, et vale rühma või puuduva ID-ga kustutamine jätab pildi ja muutmise aja alles.
    public async Task ShouldNot_DeleteImage_WhenOwnerIsDifferentOrImageIsMissing()
    {
        var owner = await Svc<IKindergartenServices>().Create(Group(Png()));
        var imageId = Assert.Single(owner.Images).Id;
        var originalDate = owner.UpdatedAt;

        Assert.False(await Svc<IFileService>().DeleteImageAsync(Guid.NewGuid(), imageId));
        Assert.False(await Svc<IFileService>().DeleteImageAsync(owner.Id, Guid.NewGuid()));

        using var verify = NewContext();
        Assert.Equal(imageId, (await verify.KindergartenImages.SingleAsync()).Id);
        Assert.Equal(originalDate, (await verify.Kindergartens.SingleAsync()).UpdatedAt);
    }
}
