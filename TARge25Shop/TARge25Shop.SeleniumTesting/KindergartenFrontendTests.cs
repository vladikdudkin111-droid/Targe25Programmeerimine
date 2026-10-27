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
}
