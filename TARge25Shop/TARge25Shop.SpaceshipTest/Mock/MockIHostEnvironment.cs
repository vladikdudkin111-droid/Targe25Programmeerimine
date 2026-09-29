using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace TARge25Shop.SpaceshipTest.Mock
{
    // Testi keskkond annab FileServices teenusele päris ajutise kausta.
    public sealed class MockIHostEnvironment : IHostEnvironment, IDisposable
    {
        // Kustutamisel kasutame ainult siin loodud kausta, mitte hiljem muudetavat omadust.
        private readonly string _temporaryRoot;
        private bool _disposed;

        public MockIHostEnvironment()
        {
            _temporaryRoot = Path.Combine(Path.GetTempPath(),
                "TARge25Shop.Tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_temporaryRoot);
            ContentRootPath = _temporaryRoot;
        }

        // Testides on omadused kasutatavad ega viska NotImplementedException erindit.
        public string EnvironmentName { get; set; } = Environments.Development;
        public string ApplicationName { get; set; } = "TARge25Shop.SpaceshipTest";
        public string ContentRootPath { get; set; }

        // Failiteenus kasutab füüsilist teed; staatiliste failide päringuid siin ei tehta.
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();

        // Eemaldame pärast testi ainult selle testi ajutised failid.
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            if (Directory.Exists(_temporaryRoot))
            {
                Directory.Delete(_temporaryRoot, recursive: true);
            }
        }
    }
}
