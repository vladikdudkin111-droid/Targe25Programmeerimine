using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TARge25Shop.Controllers;
using TARge25Shop.Core.Dto;
using TARge25Shop.Core.ServiceInterface;
using TARge25Shop.Data;
using TARge25Shop.Models.Kindergarten;
using Xunit;

namespace TARge25Shop.Tests;

public sealed class KindergartenControllerTests : TestBase
{
    private KindergartenController Controller() => new(
        Svc<IKindergartenServices>(), Svc<TARge25ShopContext>(), Svc<IFileService>())
    {
        ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
    };

    private static KindergartenCreateUpdateViewModel Form(Guid id = default) => new()
    {
        Id = id, GroupName = "Õunakesed", KindergartenName = "Sõpruse lasteaed",
        TeacherName = "Maarja Saar", ChildrenCount = 16
    };

    private static void ValidateForm(KindergartenController controller, KindergartenCreateUpdateViewModel vm)
    {
        var errors = new List<ValidationResult>();
        Validator.TryValidateObject(vm, new ValidationContext(vm), errors, validateAllProperties: true);
        foreach (var error in errors)
            foreach (var member in error.MemberNames)
                controller.ModelState.AddModelError(member, error.ErrorMessage!);
    }

    private static FormFile Upload(MemoryStream stream, string name) => new(stream, 0, stream.Length, "Files", name);

    [Fact]
    public async Task Should_ListGroupsInCreationOrder_WhenOpeningIndex()
    {
        var first = await Svc<IKindergartenServices>().Create(Group());
        var second = await Svc<IKindergartenServices>().Create(Group());
        first.CreatedAt = new DateTime(2022, 1, 1);
        second.CreatedAt = new DateTime(2021, 1, 1);
        await Svc<TARge25ShopContext>().SaveChangesAsync();

        var result = Assert.IsType<ViewResult>(await Controller().Index());

        var groups = Assert.IsType<List<KindergartenIndexViewModel>>(result.Model);
        Assert.Equal(new[] { second.Id, first.Id }, groups.Select(x => x.Id));
        Assert.Equal(second.GroupName, groups[0].GroupName);
        Assert.Equal(second.TeacherName, groups[0].TeacherName);
        Assert.Equal(second.KindergartenName, groups[0].KindergartenName);
        Assert.Equal(second.ChildrenCount, groups[0].ChildrenCount);
        Assert.Equal(second.CreatedAt, groups[0].CreatedAt);
        Assert.Equal(second.UpdatedAt, groups[0].UpdatedAt);
    }

    [Fact]
    public async Task Should_ShowEmptyList_WhenNoGroupsExist()
    {
        var result = Assert.IsType<ViewResult>(await Controller().Index());

        Assert.Empty(Assert.IsType<List<KindergartenIndexViewModel>>(result.Model));
    }

    [Fact]
    public void Should_ShowBlankForm_WhenOpeningCreate()
    {
        var result = Assert.IsType<ViewResult>(Controller().Create());

        Assert.Equal("CreateUpdate", result.ViewName);
        var vm = Assert.IsType<KindergartenCreateUpdateViewModel>(result.Model);
        Assert.Equal(Guid.Empty, vm.Id);
        Assert.Empty(vm.GroupName);
        Assert.Empty(vm.Files);
    }

    [Fact]
    public async Task Should_SaveFormAndImage_WhenCreatingValidGroup()
    {
        var controller = Controller();
        var vm = Form(Guid.NewGuid());
        var suppliedId = vm.Id;
        using var stream = new MemoryStream(Png().Data);
        vm.Files.Add(Upload(stream, "vorm.png"));
        ValidateForm(controller, vm);

        var result = Assert.IsType<RedirectToActionResult>(await controller.Create(vm));

        Assert.Equal("Index", result.ActionName);
        using var verify = NewContext();
        var saved = await verify.Kindergartens.SingleAsync();
        Assert.NotEqual(suppliedId, saved.Id);
        Assert.Equal(vm.GroupName, saved.GroupName);
        Assert.Equal(vm.KindergartenName, saved.KindergartenName);
        Assert.Equal(vm.TeacherName, saved.TeacherName);
        Assert.Equal(vm.ChildrenCount, saved.ChildrenCount);
        var image = await verify.KindergartenImages.SingleAsync();
        Assert.Equal(saved.Id, image.KindergartenId);
        Assert.Equal(Png().Data, image.Data);
    }

    [Theory]
    [InlineData("GroupName", null)]
    [InlineData("GroupName", "")]
    [InlineData("GroupName", " ")]
    [InlineData("KindergartenName", null)]
    [InlineData("KindergartenName", "")]
    [InlineData("KindergartenName", " ")]
    [InlineData("TeacherName", null)]
    [InlineData("TeacherName", "")]
    [InlineData("TeacherName", " ")]
    public async Task ShouldNot_CreateGroup_WhenRequiredFieldIsMissing(string field, string? value)
    {
        var controller = Controller();
        var vm = Form();
        switch (field)
        {
            case "GroupName": vm.GroupName = value!; break;
            case "KindergartenName": vm.KindergartenName = value!; break;
            case "TeacherName": vm.TeacherName = value!; break;
        }
        ValidateForm(controller, vm);

        var result = Assert.IsType<ViewResult>(await controller.Create(vm));

        Assert.Equal("CreateUpdate", result.ViewName);
        Assert.Same(vm, result.Model);
        Assert.False(controller.ModelState.IsValid);
        Assert.Contains(field, controller.ModelState.Keys);
        using var verify = NewContext();
        Assert.Empty(await verify.Kindergartens.ToListAsync());
    }

    [Theory]
    [InlineData(-1, false)]
    [InlineData(0, true)]
    [InlineData(31, true)]
    public async Task Should_ValidateChildrenCount_UsingProjectsActualRules(int count, bool valid)
    {
        var controller = Controller();
        var vm = Form();
        vm.ChildrenCount = count;
        ValidateForm(controller, vm);

        var result = await controller.Create(vm);

        using var verify = NewContext();
        if (valid)
        {
            Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(count, (await verify.Kindergartens.SingleAsync()).ChildrenCount);
        }
        else
        {
            Assert.IsType<ViewResult>(result);
            Assert.False(controller.ModelState.IsValid);
            Assert.Empty(await verify.Kindergartens.ToListAsync());
        }
    }

    [Theory]
    [InlineData(100, true)]
    [InlineData(101, false)]
    public async Task Should_CheckStringLength_WhenSubmittingForm(int length, bool valid)
    {
        var controller = Controller();
        var vm = Form();
        vm.GroupName = vm.KindergartenName = vm.TeacherName = new string('a', length);
        ValidateForm(controller, vm);

        var result = await controller.Create(vm);

        Assert.Equal(valid, controller.ModelState.IsValid);
        using var verify = NewContext();
        if (valid)
        {
            Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(length, (await verify.Kindergartens.SingleAsync()).GroupName.Length);
        }
        else
        {
            Assert.IsType<ViewResult>(result);
            Assert.Equal(3, controller.ModelState.ErrorCount);
            Assert.Empty(await verify.Kindergartens.ToListAsync());
        }
    }

    [Fact]
    public async Task Should_ShowDetailsAndGallery_WhenGroupExists()
    {
        var created = await Svc<IKindergartenServices>().Create(Group(Png()));

        var result = Assert.IsType<ViewResult>(await Controller().Details(created.Id));

        var vm = Assert.IsType<KindergartenDetailsViewModel>(result.Model);
        Assert.Equal(created.Id, vm.Id);
        Assert.Equal(created.GroupName, vm.GroupName);
        Assert.Equal(created.TeacherName, vm.TeacherName);
        Assert.Equal(created.KindergartenName, vm.KindergartenName);
        Assert.Equal(created.ChildrenCount, vm.ChildrenCount);
        Assert.Equal(created.CreatedAt, vm.CreatedAt);
        Assert.Equal(created.UpdatedAt, vm.UpdatedAt);
        Assert.Equal(created.Id, vm.Gallery.KindergartenId);
        Assert.False(vm.Gallery.AllowDelete);
        Assert.Equal(Assert.Single(created.Images).Id, Assert.Single(vm.Gallery.Images).Id);
    }

    [Fact]
    public async Task Should_ShowEditableGallery_WhenOpeningUpdate()
    {
        var created = await Svc<IKindergartenServices>().Create(Group(Png()));

        var result = Assert.IsType<ViewResult>(await Controller().Update(created.Id));

        Assert.Equal("CreateUpdate", result.ViewName);
        var vm = Assert.IsType<KindergartenCreateUpdateViewModel>(result.Model);
        Assert.Equal(created.Id, vm.Id);
        Assert.Equal(created.GroupName, vm.GroupName);
        Assert.Equal(created.TeacherName, vm.TeacherName);
        Assert.Equal(created.KindergartenName, vm.KindergartenName);
        Assert.Equal(created.ChildrenCount, vm.ChildrenCount);
        Assert.True(vm.Gallery.AllowDelete);
        Assert.Single(vm.Gallery.Images);
    }

    [Fact]
    public async Task Should_SaveChanges_WhenSubmittingValidUpdate()
    {
        var created = await Svc<IKindergartenServices>().Create(Group());
        var vm = Form(created.Id);
        var controller = Controller();
        ValidateForm(controller, vm);

        var result = Assert.IsType<RedirectToActionResult>(await controller.Update(created.Id, vm));

        Assert.Equal("Index", result.ActionName);
        using var verify = NewContext();
        var saved = await verify.Kindergartens.SingleAsync();
        Assert.Equal(vm.Id, saved.Id);
        Assert.Equal(vm.GroupName, saved.GroupName);
        Assert.Equal(vm.KindergartenName, saved.KindergartenName);
        Assert.Equal(vm.TeacherName, saved.TeacherName);
        Assert.Equal(vm.ChildrenCount, saved.ChildrenCount);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Should_RejectUpdate_WhenRouteIdIsDifferentOrEmpty(bool emptyId)
    {
        var created = await Svc<IKindergartenServices>().Create(Group());
        var vm = Form(created.Id);

        Assert.IsType<BadRequestResult>(await Controller().Update(emptyId ? Guid.Empty : Guid.NewGuid(), vm));

        using var verify = NewContext();
        Assert.Equal(created.GroupName, (await verify.Kindergartens.SingleAsync()).GroupName);
    }

    [Fact]
    public async Task Should_RestoreGalleryAndKeepData_WhenUpdateFormIsInvalid()
    {
        var created = await Svc<IKindergartenServices>().Create(Group(Png()));
        var controller = Controller();
        var vm = Form(created.Id);
        vm.ChildrenCount = -1;
        ValidateForm(controller, vm);

        var result = Assert.IsType<ViewResult>(await controller.Update(created.Id, vm));

        Assert.Equal("CreateUpdate", result.ViewName);
        var returned = Assert.IsType<KindergartenCreateUpdateViewModel>(result.Model);
        Assert.Single(returned.Gallery.Images);
        Assert.True(returned.Gallery.AllowDelete);
        using var verify = NewContext();
        Assert.Equal(created.ChildrenCount, (await verify.Kindergartens.SingleAsync()).ChildrenCount);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Should_NotSaveChanges_WhenUploadedImageHasWrongFormat(bool update)
    {
        var created = await Svc<IKindergartenServices>().Create(Group(Png("vana.png")));
        var controller = Controller();
        var vm = Form(update ? created.Id : Guid.Empty);
        using var stream = new MemoryStream("vale sisu"u8.ToArray());
        vm.Files.Add(Upload(stream, "vale.png"));

        var result = Assert.IsType<ViewResult>(update
            ? await controller.Update(created.Id, vm)
            : await controller.Create(vm));

        Assert.Equal("CreateUpdate", result.ViewName);
        Assert.False(controller.ModelState.IsValid);
        Assert.Contains(nameof(vm.Files), controller.ModelState.Keys);
        if (update) Assert.Single(Assert.IsType<KindergartenCreateUpdateViewModel>(result.Model).Gallery.Images);
        using var verify = NewContext();
        Assert.Equal(created.GroupName, (await verify.Kindergartens.SingleAsync()).GroupName);
        Assert.Equal("vana.png", (await verify.KindergartenImages.SingleAsync()).FileName);
    }

    [Theory]
    [InlineData("empty")]
    [InlineData("large")]
    [InlineData("count")]
    [InlineData("total")]
    public async Task Should_ReturnForm_WhenUploadLimitsAreExceeded(string scenario)
    {
        var controller = Controller();
        var vm = Form();
        var size = scenario switch
        {
            "large" => FileUploadDto.MaxFileSize + 1,
            "total" => FileUploadDto.MaxFileSize,
            "empty" => 0,
            _ => Png().Data.Length
        };
        using var stream = new MemoryStream(new byte[size]);
        var count = scenario switch { "count" => 11, "total" => 5, _ => 1 };
        vm.Files = Enumerable.Range(0, count).Select(_ => (IFormFile)Upload(stream, "pilt.png")).ToList();

        Assert.IsType<ViewResult>(await controller.Create(vm));

        Assert.False(controller.ModelState.IsValid);
        using var verify = NewContext();
        Assert.Empty(await verify.Kindergartens.ToListAsync());
        Assert.Empty(await verify.KindergartenImages.ToListAsync());
    }

    [Fact]
    public async Task Should_ShowConfirmationWithoutDeleting_WhenOpeningDelete()
    {
        var created = await Svc<IKindergartenServices>().Create(Group(Png()));

        var result = Assert.IsType<ViewResult>(await Controller().Delete(created.Id));

        var vm = Assert.IsType<KindergartenDeleteViewModel>(result.Model);
        Assert.Equal(created.Id, vm.Id);
        Assert.Equal(created.GroupName, vm.GroupName);
        Assert.Equal(created.TeacherName, vm.TeacherName);
        Assert.Equal(created.KindergartenName, vm.KindergartenName);
        Assert.Equal(created.ChildrenCount, vm.ChildrenCount);
        Assert.Equal(created.CreatedAt, vm.CreatedAt);
        Assert.Equal(created.UpdatedAt, vm.UpdatedAt);
        Assert.False(vm.Gallery.AllowDelete);
        Assert.Single(vm.Gallery.Images);
        using var verify = NewContext();
        Assert.Single(await verify.Kindergartens.ToListAsync());
    }

    [Fact]
    public async Task Should_DeleteGroupAndRedirect_WhenDeletionIsConfirmed()
    {
        var created = await Svc<IKindergartenServices>().Create(Group(Png()));
        Svc<TARge25ShopContext>().ChangeTracker.Clear();

        var result = Assert.IsType<RedirectToActionResult>(await Controller().DeleteConfirmation(created.Id));

        Assert.Equal("Index", result.ActionName);
        using var verify = NewContext();
        Assert.Empty(await verify.Kindergartens.ToListAsync());
        Assert.Empty(await verify.KindergartenImages.ToListAsync());
    }

    [Fact]
    public async Task Should_ReturnImageWithMimeType_WhenOwnerMatches()
    {
        var created = await Svc<IKindergartenServices>().Create(Group(Png()));
        var controller = Controller();

        var result = Assert.IsType<FileContentResult>(await controller.Image(created.Id, Assert.Single(created.Images).Id));

        Assert.Equal(Png().Data, result.FileContents);
        Assert.Equal("image/png", result.ContentType);
        Assert.Equal("nosniff", controller.Response.Headers["X-Content-Type-Options"].ToString());
    }

    [Fact]
    public async Task Should_DeleteImageAndReturnToUpdate_WhenOwnerMatches()
    {
        var created = await Svc<IKindergartenServices>().Create(Group(Png()));

        var result = Assert.IsType<RedirectToActionResult>(await Controller().DeleteImage(created.Id, Assert.Single(created.Images).Id));

        Assert.Equal("Update", result.ActionName);
        Assert.NotNull(result.RouteValues);
        Assert.Equal(created.Id, Assert.IsType<Guid>(result.RouteValues["id"]));
        using var verify = NewContext();
        Assert.Single(await verify.Kindergartens.ToListAsync());
        Assert.Empty(await verify.KindergartenImages.ToListAsync());
    }

    [Theory]
    [InlineData("Details")]
    [InlineData("UpdateGet")]
    [InlineData("UpdatePost")]
    [InlineData("Delete")]
    [InlineData("DeleteConfirmation")]
    [InlineData("Image")]
    [InlineData("DeleteImage")]
    public async Task Should_ReturnNotFound_WhenRequestedDataDoesNotExist(string action)
    {
        var controller = Controller();
        var id = Guid.NewGuid();

        var result = action switch
        {
            "Details" => await controller.Details(id),
            "UpdateGet" => await controller.Update(id),
            "UpdatePost" => await controller.Update(id, Form(id)),
            "Delete" => await controller.Delete(id),
            "DeleteConfirmation" => await controller.DeleteConfirmation(id),
            "Image" => await controller.Image(id, Guid.NewGuid()),
            "DeleteImage" => await controller.DeleteImage(id, Guid.NewGuid()),
            _ => throw new ArgumentOutOfRangeException(nameof(action))
        };

        Assert.IsType<NotFoundResult>(result);
    }
}
