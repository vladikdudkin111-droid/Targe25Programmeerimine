using Xunit;

namespace TARge25Shop.SeleniumTesting;

public sealed class KindergartenExtraTests : BrowserTestBase
{
    /// <summary>
    /// Sisestab muutmisvormi uued andmed, lahkub salvestamata ja kontrollib vanade andmete säilimist.
    /// </summary>
    [Fact, Trait("Scenario", "Extra")]
    public void Should_KeepOriginalValues_WhenLeavingUpdateWithoutSaving()
    {
        var page = new KindergartenPage(Driver, BaseAddress);
        var original = NewGroup();
        var id = page.Create(original);
        var unsaved = original with
        {
            GroupName = original.GroupName + " salvestamata",
            ChildrenCount = 29,
            KindergartenName = "Teine lasteaed",
            TeacherName = "Teine õpetaja"
        };

        page.OpenUpdate(id);
        page.FillForm(unsaved);
        page.BackToList();
        page.ReloadIndex();

        Assert.Empty(page.IdsWithName(unsaved.GroupName));
        Assert.Equal(id, Assert.Single(page.IdsWithName(original.GroupName)));
        Assert.Equal(new[] { original.GroupName, "17", original.KindergartenName, original.TeacherName }, page.IndexValues(id));
        page.OpenUpdate(id);
        Assert.Equal(original.GroupName, page.FormValue("GroupName"));
        Assert.Equal("17", page.FormValue("ChildrenCount"));
        Assert.Equal(original.KindergartenName, page.FormValue("KindergartenName"));
        Assert.Equal(original.TeacherName, page.FormValue("TeacherName"));
    }
    /// <summary>
    /// Jätab õpetaja nime tühjaks ja kontrollib veateadet ning rühma salvestamata jäämist.
    /// </summary>
    [Fact, Trait("Scenario", "Extra")]
    public void Should_ShowValidationError_WithoutCreatingGroup_WhenTeacherNameIsEmpty()
    {
        var page = new KindergartenPage(Driver, BaseAddress);
        var group = NewGroup() with { TeacherName = "" };
        page.OpenIndex();
        page.OpenCreate();
        page.FillForm(group);

        page.Submit();

        Assert.Equal("Õpetaja nimi on kohustuslik.", page.ValidationMessage("TeacherName"));
        Assert.Equal(group.GroupName, page.FormValue("GroupName"));
        Assert.Equal(group.KindergartenName, page.FormValue("KindergartenName"));
        page.BackToList();
        page.ReloadIndex();
        Assert.Empty(page.IdsWithName(group.GroupName));
    }
    /// <summary>
    /// Loob kaks sama nimega rühma ja kontrollib ainult valitud ID-ga rühma kustutamist.
    /// </summary>
    [Fact, Trait("Scenario", "Extra")]
    public void Should_KeepOtherGroup_WhenDeletingOneOfTwoGroupsWithSameName()
    {
        var page = new KindergartenPage(Driver, BaseAddress);
        var first = NewGroup();
        var second = first with { ChildrenCount = 21, TeacherName = "Anneli Saar" };
        var firstId = page.Create(first);
        var secondId = page.Create(second);
        Assert.NotEqual(firstId, secondId);
        Assert.Equal(2, page.IdsWithName(first.GroupName).Length);

        page.OpenDelete(firstId);
        Assert.Equal(firstId.ToString(), page.Details()["Id"]);
        page.Save();
        page.ReloadIndex();

        Assert.False(page.Contains(firstId));
        Assert.Equal(secondId, Assert.Single(page.IdsWithName(first.GroupName)));
        Assert.Equal(new[] { second.GroupName, "21", second.KindergartenName, "Anneli Saar" }, page.IndexValues(secondId));
        page.OpenDetails(secondId);
        Assert.Equal(secondId.ToString(), page.Details()["Id"]);
        Assert.Equal("Anneli Saar", page.Details()["Teacher name"]);
    }
}
