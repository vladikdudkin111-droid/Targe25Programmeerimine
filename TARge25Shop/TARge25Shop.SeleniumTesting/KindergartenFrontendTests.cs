using Xunit;

namespace TARge25Shop.SeleniumTesting;

public sealed class KindergartenFrontendTests : BrowserTestBase
{
    /// <summary>
    /// Loob vormi kaudu uue rühma ja kontrollib selle andmeid pärast lehe uuesti avamist.
    /// </summary>
    [Fact]
    public void Should_CreateGroup_AndShowSavedValuesInList()
    {
        var page = new KindergartenPage(Driver, BaseAddress);
        var group = NewGroup();

        var id = page.Create(group);
        page.ReloadIndex();

        Assert.NotEqual(Guid.Empty, id);
        Assert.Single(page.IdsWithName(group.GroupName));
        Assert.Equal(new[] { group.GroupName, "17", group.KindergartenName, group.TeacherName }, page.IndexValues(id));
    }

    /// <summary>
    /// Avab loodud rühma detailid ja kontrollib sama ID ning kõigi põhiandmete kuvamist.
    /// </summary>
    [Fact]
    public void Should_ShowCreatedGroupsDetails_WithMatchingIdAndValues()
    {
        var page = new KindergartenPage(Driver, BaseAddress);
        var group = NewGroup();
        var id = page.Create(group);

        page.OpenDetails(id);
        var details = page.Details();

        Assert.Equal(id.ToString(), details["Id"]);
        Assert.Equal(group.GroupName, details["Group name"]);
        Assert.Equal("17", details["Children count"]);
        Assert.Equal(group.KindergartenName, details["Kindergarten name"]);
        Assert.Equal(group.TeacherName, details["Teacher name"]);
        Assert.False(string.IsNullOrWhiteSpace(details["Created at"]));
        Assert.False(string.IsNullOrWhiteSpace(details["Updated at"]));
    }
    /// <summary>
    /// Muudab loodud rühma kõiki põhiandmeid ja kontrollib sama ID ning loomise aja säilimist.
    /// </summary>
    [Fact]
    public void Should_UpdateCreatedGroup_WithoutChangingItsIdOrCreationTime()
    {
        var page = new KindergartenPage(Driver, BaseAddress);
        var original = NewGroup();
        var id = page.Create(original);
        page.OpenDetails(id);
        var createdAt = page.Details()["Created at"];
        page.BackToList();
        var changed = original with
        {
            GroupName = original.GroupName + " uus",
            ChildrenCount = 22,
            KindergartenName = "Metsatuka lasteaed",
            TeacherName = "Jüri Tamm"
        };

        page.OpenUpdate(id);
        Assert.Equal(id.ToString(), page.FormValue("Id"));
        Assert.Equal(original.GroupName, page.FormValue("GroupName"));
        page.FillForm(changed);
        page.Save();
        page.ReloadIndex();

        Assert.Empty(page.IdsWithName(original.GroupName));
        Assert.Equal(id, Assert.Single(page.IdsWithName(changed.GroupName)));
        Assert.Equal(new[] { changed.GroupName, "22", changed.KindergartenName, changed.TeacherName }, page.IndexValues(id));
        page.OpenDetails(id);
        var details = page.Details();
        Assert.Equal(id.ToString(), details["Id"]);
        Assert.Equal(changed.GroupName, details["Group name"]);
        Assert.Equal("22", details["Children count"]);
        Assert.Equal(changed.KindergartenName, details["Kindergarten name"]);
        Assert.Equal(changed.TeacherName, details["Teacher name"]);
        Assert.Equal(createdAt, details["Created at"]);
    }
}
