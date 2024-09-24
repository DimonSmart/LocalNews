using Microsoft.Playwright;

namespace DimonSmart.WebScraper
{
    public class PageDownloader : IPageDownloader
    {
        public async Task<string?> DownloadPageContentAsync(string url)
        {
            try
            {
                // Initialize Playwright
                using var playwright = await Playwright.CreateAsync();

                // Launch a headless browser instance
                await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
                {
                    Headless = true // Run in headless mode
                });

                // Create a new browser page
                var page = await browser.NewPageAsync();

                // Navigate to the URL
                var response = await page.GotoAsync(url);

                // Check if navigation was successful
                if (!response.Ok)
                {
                    Console.WriteLine($"Failed to load page: {response.StatusText}");
                    return null;
                }

                // Wait for the page to load completely
                await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

                // Get the page content
                var content = await page.ContentAsync();

                return content;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while downloading page: {ex.Message}");
                return null;
            }
        }
    }
}