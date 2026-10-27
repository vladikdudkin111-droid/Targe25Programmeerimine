using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace TARge25Shop.SeleniumTesting;

public abstract class BrowserTestBase : IDisposable
{
    private readonly UiApplication _application = new();
    protected IWebDriver Driver { get; }
    protected Uri BaseAddress { get; }

    protected BrowserTestBase()
    {
        try
        {
            BaseAddress = _application.Start();
            var options = new ChromeOptions { BrowserVersion = "stable" };
            options.AddArgument("--headless=new");
            options.AddArgument("--window-size=1440,1000");
            var service = ChromeDriverService.CreateDefaultService();
            service.HideCommandPromptWindow = true;
            Driver = new ChromeDriver(service, options, TimeSpan.FromSeconds(90));
            Driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(30);
        }
        catch
        {
            _application.Dispose();
            throw;
        }
    }

    protected static KindergartenData NewGroup() => new(
        "Meritähed-" + Guid.NewGuid().ToString("N")[..8], 17, "Päikese õu", "Õie Kask");

    public void Dispose()
    {
        try
        {
            Driver.Dispose();
        }
        finally
        {
            _application.Dispose();
        }
    }
}

public sealed record KindergartenData(
    string GroupName, int ChildrenCount, string KindergartenName, string TeacherName);
