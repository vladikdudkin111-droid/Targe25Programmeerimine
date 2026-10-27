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
}
