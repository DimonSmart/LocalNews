using Microsoft.Extensions.Logging;
using Microsoft.Playwright;

namespace DimonSmart.WebScraper
{
    public class PageDownloader(ILogger<PageDownloader> logger) : IPageDownloader, IAsyncDisposable
    {
        private IBrowser? _browser;
        private IPlaywright? _playwright;
        private bool _isInitialized;

        private async Task<IPage> GetNewPageAsync()
        {
            if (!_isInitialized)
            {
                _playwright = await Playwright.CreateAsync();
                _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
                {
                    Headless = true
                });
                _isInitialized = true;
            }

            return await _browser!.NewPageAsync();
        }

        public async Task<string?> DownloadPageContentAsync(string url)
        {
            IPage? page = null;
            try
            {
                page = await GetNewPageAsync();
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
            finally
            {
                if (page != null) await page.CloseAsync();
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