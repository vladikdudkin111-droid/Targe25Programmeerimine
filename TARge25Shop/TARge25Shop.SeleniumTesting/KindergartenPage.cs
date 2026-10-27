using System.Globalization;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace TARge25Shop.SeleniumTesting;

public sealed class KindergartenPage(IWebDriver driver, Uri baseAddress)
{
    private readonly WebDriverWait _wait = new(driver, TimeSpan.FromSeconds(10));

    public void OpenIndex()
    {
        driver.Navigate().GoToUrl(new Uri(baseAddress, "Kindergarten"));
        WaitForIndex();
    }

    public void OpenCreate()
    {
        driver.FindElement(By.LinkText("Create")).Click();
        WaitForForm();
    }

    public Guid Create(KindergartenData data)
    {
        OpenIndex();
        var previousIds = IdsWithName(data.GroupName);
        OpenCreate();
        FillForm(data);
        Save();
        return IdsWithName(data.GroupName).Except(previousIds).Single();
    }

    public void FillForm(KindergartenData data)
    {
        Fill("GroupName", data.GroupName);
        Fill("ChildrenCount", data.ChildrenCount.ToString(CultureInfo.InvariantCulture));
        Fill("KindergartenName", data.KindergartenName);
        Fill("TeacherName", data.TeacherName);
    }

    public string FormValue(string field) => driver.FindElement(By.Id(field)).GetAttribute("value") ?? "";

    public void Submit() => driver.FindElement(By.CssSelector("main form button[type='submit']")).Click();

    public void Save()
    {
        Submit();
        WaitForIndex();
    }

    public void BackToList()
    {
        driver.FindElement(By.LinkText("Back to list")).Click();
        WaitForIndex();
    }

    public Guid[] IdsWithName(string name) => Rows()
        .Where(row => row.FindElements(By.TagName("td"))[1].Text == name)
        .Select(RowId).ToArray();

    public bool Contains(Guid id) => Rows().Any(row => RowId(row) == id);

    public string[] IndexValues(Guid id) => Row(id).FindElements(By.TagName("td"))
        .Skip(1).Take(4).Select(cell => cell.Text).ToArray();

    public void OpenDetails(Guid id)
    {
        Row(id).FindElement(By.LinkText("Details")).Click();
        WaitForHeading("Details");
    }

    public void OpenUpdate(Guid id)
    {
        Row(id).FindElement(By.LinkText("Update")).Click();
        WaitForForm();
    }

    public void OpenDelete(Guid id)
    {
        Row(id).FindElement(By.LinkText("Delete")).Click();
        WaitForHeading("Delete");
    }

    public Dictionary<string, string> Details()
    {
        var names = driver.FindElements(By.CssSelector("main dl dt"));
        var values = driver.FindElements(By.CssSelector("main dl dd"));
        return names.Zip(values).ToDictionary(pair => pair.First.Text, pair => pair.Second.Text);
    }

    public string ValidationMessage(string field) => _wait.Until(browser =>
    {
        var message = browser.FindElement(By.CssSelector($"[data-valmsg-for='{field}']")).Text;
        return string.IsNullOrWhiteSpace(message) ? null : message;
    });

    public void ReloadIndex() => OpenIndex();

    private IReadOnlyCollection<IWebElement> Rows() => driver.FindElements(By.CssSelector("main table tbody tr"));

    private IWebElement Row(Guid id) => Rows().Single(row => RowId(row) == id);

    private static Guid RowId(IWebElement row)
    {
        var address = new Uri(row.FindElement(By.LinkText("Details")).GetAttribute("href")!);
        return Guid.Parse(address.Segments.Last());
    }

    private void Fill(string field, string value)
    {
        var input = driver.FindElement(By.Id(field));
        input.Clear();
        input.SendKeys(value);
    }

    private void WaitForHeading(string text) =>
        _wait.Until(browser => browser.FindElement(By.CssSelector("main h1")).Text == text);

    private void WaitForIndex() => _wait.Until(browser =>
        browser.FindElements(By.CssSelector("main table tbody")).Count == 1);

    private void WaitForForm() => _wait.Until(browser => browser.FindElement(By.Id("GroupName")).Displayed);
}
