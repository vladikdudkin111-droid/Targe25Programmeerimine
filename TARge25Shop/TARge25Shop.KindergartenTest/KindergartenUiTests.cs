using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Xunit;

namespace TARge25Shop.KindergartenTest;

public class KindergartenUiTests : IDisposable
{
    private const string ListUrl = "http://localhost:5277/Kindergarten";
    private readonly ChromeDriver _browser = new();

    /// <summary>
    /// Loob rühma ja kontrollib, et nimekirjas kuvatakse kõik sisestatud andmed.
    /// </summary>
    [Fact]
    public void CreateGroupShowsSavedDataInList()
    {
        string name = NewName();

        CreateGroup(name);

        AssertRow(name, "12", "Metsatark", "Mari");
        DeleteGroup(name);
    }

    /// <summary>
    /// Loob rühma, vaatab ja muudab seda ning kontrollib, et sama ID-ga rühm
    /// kaob pärast kustutamist nimekirjast.
    /// </summary>
    [Fact]
    public void DeleteRemovesGroupAfterUpdate()
    {
        string name = NewName();
        CreateGroup(name);
        OpenGroupPage(name, "Details");

        string id = DetailsValue("Id");
        Assert.Equal(name, DetailsValue("Group name"));

        ClickLink("Update");
        string updatedName = name + "-muudetud";
        FillForm(updatedName, "16", "Männik", "Kati");
        SubmitForm();

        AssertRow(updatedName, "16", "Männik", "Kati");
        OpenGroupPage(updatedName, "Delete");
        Assert.Equal(id, DetailsValue("Id"));
        Assert.Equal(updatedName, DetailsValue("Group name"));
        SubmitForm();

        _browser.Navigate().Refresh();
        Assert.Null(FindRow(updatedName));
    }

    /// <summary>
    /// Jätab rühma nime tühjaks ja kontrollib, et vorm kuvab veateate.
    /// </summary>
    [Fact]
    public void EmptyGroupNameShowsValidationError()
    {
        OpenCreateForm();
        FillForm("", "12", "Metsatark", "Mari");

        SubmitForm();

        var error = _browser.FindElement(
            By.CssSelector("[data-valmsg-for='GroupName']"));
        Assert.True(error.Displayed);
        Assert.Equal("Rühma nimi on kohustuslik.", error.Text);
    }

    /// <summary>
    /// Sisestab negatiivse laste arvu ja kontrollib, et rühma ei salvestata.
    /// </summary>
    [Fact]
    public void NegativeChildrenCountPreventsSaving()
    {
        string name = NewName();
        OpenCreateForm();
        FillForm(name, "-1", "Metsatark", "Mari");

        SubmitForm();

        Assert.Equal("-1", Field("ChildrenCount").GetDomProperty("value"));
        Assert.NotEmpty(Field("ChildrenCount").GetDomProperty("validationMessage") ?? "");

        ClickLink("Back to list");
        Assert.Null(FindRow(name));
    }

    /// <summary>
    /// Lahkub muutmisvormilt salvestamata ja kontrollib, et algsed andmed jäävad alles.
    /// </summary>
    [Fact]
    public void CancelUpdateKeepsOriginalData()
    {
        string name = NewName();
        CreateGroup(name);
        OpenGroupPage(name, "Update");

        string unsavedName = name + "-salvestamata";
        FillForm(unsavedName, "20", "Männik", "Kati");
        ClickLink("Back to list");

        Assert.Null(FindRow(unsavedName));
        AssertRow(name, "12", "Metsatark", "Mari");
        DeleteGroup(name);
    }

    /// <summary>
    /// Loob kaks sama nimega rühma ja kontrollib, et kustutatakse ainult valitud rühm.
    /// </summary>
    [Fact]
    public void DeleteKeepsOtherGroupWithSameName()
    {
        string name = NewName();
        CreateGroup(name, "Mari");
        CreateGroup(name, "Kati");

        AssertRow(name, "12", "Metsatark", "Mari");
        AssertRow(name, "12", "Metsatark", "Kati");

        DeleteGroup(name, "Mari");

        Assert.Null(FindRow(name, "Mari"));
        AssertRow(name, "12", "Metsatark", "Kati");
        DeleteGroup(name, "Kati");
    }

    private static string NewName() => $"UI-test-{Guid.NewGuid():N}";

    /// <summary>
    /// Avab loodud rühma detailvaate ja kontrollib rühma ID-d ning kõiki sisestatud andmeid.
    /// </summary>
    [Fact]
    public void DetailsShowsCreatedGroup()
    {
        string name = NewName();
        CreateGroup(name);

        OpenGroupPage(name, "Details");

        Assert.True(Guid.TryParse(DetailsValue("Id"), out Guid id));
        Assert.NotEqual(Guid.Empty, id);
        Assert.Equal(name, DetailsValue("Group name"));
        Assert.Equal("12", DetailsValue("Children count"));
        Assert.Equal("Metsatark", DetailsValue("Kindergarten name"));
        Assert.Equal("Mari", DetailsValue("Teacher name"));

        ClickLink("Back to list");
        DeleteGroup(name);
    }

    /// <summary>
    /// Muudab rühma andmeid ja kontrollib, et pärast salvestamist jääb rühma ID samaks.
    /// </summary>
    [Fact]
    public void UpdateChangesDataOfSameGroup()
    {
        string name = NewName();
        CreateGroup(name);
        OpenGroupPage(name, "Details");

        string id = DetailsValue("Id");
        ClickLink("Update");
        Assert.Equal(name, Field("GroupName").GetDomProperty("value"));

        string updatedName = name + "-muudetud";
        FillForm(updatedName, "16", "Männik", "Kati");
        SubmitForm();

        Assert.Null(FindRow(name));
        AssertRow(updatedName, "16", "Männik", "Kati");
        OpenGroupPage(updatedName, "Details");
        Assert.Equal(id, DetailsValue("Id"));

        ClickLink("Back to list");
        DeleteGroup(updatedName);
    }

    private string DetailsValue(string label) =>
        _browser.FindElement(By.XPath(
            $"//main//dt[normalize-space(.)='{label}']/following-sibling::dd[1]")).Text;

    private void OpenCreateForm()
    {
        _browser.Navigate().GoToUrl(ListUrl);
        ClickLink("Create");
    }

    private void CreateGroup(string name, string teacher = "Mari")
    {
        OpenCreateForm();
        FillForm(name, "12", "Metsatark", teacher);
        SubmitForm();
        var tables = _browser.FindElements(By.CssSelector("main table"));
        var errors = _browser.FindElements(
                By.CssSelector("main .field-validation-error, main .validation-summary-errors"))
            .Select(element => element.Text)
            .Where(message => !string.IsNullOrWhiteSpace(message));

        Assert.True(
            tables.Count > 0,
            $"После сохранения список не открылся. URL: {_browser.Url}. " +
            $"Ошибки формы: {string.Join(" | ", errors)}");
    }

    private void FillForm(string name, string count, string kindergarten, string teacher)
    {
        Fill("GroupName", name);
        Fill("ChildrenCount", count);
        Fill("KindergartenName", kindergarten);
        Fill("TeacherName", teacher);
    }

    private void Fill(string fieldName, string value)
    {
        var input = Field(fieldName);
        input.Clear();

        if (value.Length > 0)
        {
            input.SendKeys(value);
        }
    }

    private IWebElement Field(string name) =>
        _browser.FindElement(By.Name(name));

    private void ClickLink(string text) =>
        _browser.FindElement(By.LinkText(text)).Click();

    private void SubmitForm() =>
        _browser.FindElement(By.CssSelector("main form button[type='submit']")).Click();

    private IWebElement? FindRow(string name, string? teacher = null)
    {
        return _browser.FindElements(By.CssSelector("main table tbody tr"))
            .SingleOrDefault(row =>
            {
                var cells = row.FindElements(By.TagName("td"));
                return cells[1].Text == name &&
                    (teacher == null || cells[4].Text == teacher);
            });
    }

    private void AssertRow(string name, string count, string kindergarten, string teacher)
    {
        var row = FindRow(name, teacher);
        Assert.NotNull(row);

        var cells = row.FindElements(By.TagName("td"));
        Assert.Equal(name, cells[1].Text);
        Assert.Equal(count, cells[2].Text);
        Assert.Equal(kindergarten, cells[3].Text);
        Assert.Equal(teacher, cells[4].Text);
    }

    private void OpenGroupPage(string name, string link, string? teacher = null)
    {
        var row = FindRow(name, teacher);
        Assert.NotNull(row);
        row.FindElement(By.LinkText(link)).Click();
    }

    private void DeleteGroup(string name, string? teacher = null)
    {
        OpenGroupPage(name, "Delete", teacher);
        SubmitForm();
    }

    public void Dispose() => _browser.Dispose();
}