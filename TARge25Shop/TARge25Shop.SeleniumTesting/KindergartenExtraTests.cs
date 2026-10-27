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
}
