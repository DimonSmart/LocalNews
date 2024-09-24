using Microsoft.Extensions.Logging;
using Microsoft.Playwright;

namespace DimonSmart.WebScraper
{
    public class PageDownloader(ILogger<PageDownloader> logger) : IPageDownloader, IAsyncDisposable
    {
        private IBrowser? _browser;
        private IPlaywright? _playwright;
        private bool _isInitialized;

        private async Task<IBrowser> InitializeAsync()
        {
            if (_isInitialized) return _browser ?? throw new InvalidOperationException("Browser is not initialized");
            _isInitialized = true;

            _playwright = await Playwright.CreateAsync();
            _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true
            });

            return _browser;
        }

        public async Task<string?> DownloadPageContentAsync(string url)
        {
            try
            {
                var browser = await InitializeAsync();

                var page = await browser.NewPageAsync();
                var navigationResult = await page.GotoAsync(url);

                if (navigationResult == null || !navigationResult.Ok)
                {
                    logger.LogError($"Failed to load page at {url}: {navigationResult?.StatusText}");
                    return null;
                }

                await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

                var content = await page.ContentAsync();

                return content;
            }
            catch (Exception ex)
            {
                logger.LogError($"Error while downloading page at {url}: {ex.Message}");
                return null;
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_browser != null) await _browser.CloseAsync();
            _playwright?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}