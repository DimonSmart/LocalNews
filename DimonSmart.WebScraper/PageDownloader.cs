namespace DimonSmart.WebScraper;

public class PageDownloader : IPageDownloader
{
    private readonly HttpClient _httpClient = new HttpClient();

    public async Task<string?> DownloadPageContentAsync(string url)
    {
        try
        {
            var response = await _httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Error while downloading page: {ex.Message}");
        }

        return null;
    }
}