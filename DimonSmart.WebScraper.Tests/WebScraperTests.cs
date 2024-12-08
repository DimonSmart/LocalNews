using Microsoft.Extensions.Options;
using Moq;
using Serilog;

namespace DimonSmart.WebScraper.Tests;

public class WebScraperTests
{
    [Fact]
    public async Task Test_WebScraper_ProcessesRequests_With_Depth_Restriction()
    {
        // Arrange
        var contentExtractor = new MainContentExtractor();

        var mockPageDownloader = new Mock<IPageDownloader>();
        var mockPageHandler = new Mock<ILinkExtractor>();
        var mockPageStorage = new Mock<IPageStorage>();
        var mockLogger = new Mock<ILogger>();
        var mockUrlQueueManager = new Mock<IUrlQueueManager>();

        // The start page contains two links
        const string rootPageContent =
            "<html><a href='https://example.com/page1'></a><a href='https://example.com/page2'></a></html>";
        // The first child page contains two links (they should not be downloaded)
        const string page1Content =
            "<html><a href='https://example.com/subpage1'></a><a href='https://example.com/subpage2'></a></html>";
        // The second child page also contains two links (they should not be downloaded either)
        const string page2Content =
            "<html><a href='https://example.com/subpage3'></a><a href='https://example.com/subpage4'></a></html>";

        // Setting up mocks for the page downloader
        mockPageDownloader.Setup(pd => pd.DownloadPageContentAsync("https://example.com"))
            .ReturnsAsync(rootPageContent);
        mockPageDownloader.Setup(pd => pd.DownloadPageContentAsync("https://example.com/page1"))
            .ReturnsAsync(page1Content);
        mockPageDownloader.Setup(pd => pd.DownloadPageContentAsync("https://example.com/page2"))
            .ReturnsAsync(page2Content);

        // Setting up mocks for the page handler
        mockPageHandler.Setup(ph => ph.ExtractLinksFromPage(rootPageContent, "https://example.com"))
            .Returns(new List<string> { "https://example.com/page1", "https://example.com/page2" });
        mockPageHandler.Setup(ph => ph.ExtractLinksFromPage(page1Content, "https://example.com/page1"))
            .Returns(new List<string> { "https://example.com/subpage1", "https://example.com/subpage2" });
        mockPageHandler.Setup(ph => ph.ExtractLinksFromPage(page2Content, "https://example.com/page2"))
            .Returns(new List<string> { "https://example.com/subpage3", "https://example.com/subpage4" });

        // Setting up mocks for UrlQueueManager - the first three links can be added, the rest cannot
        mockUrlQueueManager.Setup(um => um.CanAddUrl("https://example.com")).Returns(true);
        mockUrlQueueManager.Setup(um => um.CanAddUrl("https://example.com/page1")).Returns(true);
        mockUrlQueueManager.Setup(um => um.CanAddUrl("https://example.com/page2")).Returns(true);
        mockUrlQueueManager.Setup(um => um.CanAddUrl(It.Is<string>(url =>
            url == "https://example.com/subpage1" ||
            url == "https://example.com/subpage2" ||
            url == "https://example.com/subpage3" ||
            url == "https://example.com/subpage4"))).Returns(false);

        // Creating a WebScraper object with a download depth of 1
        var settings = Options.Create(new WebScraperSettings { MaxThreads = 1 });
        var scraper = new WebScraper(
            settings,
            mockPageDownloader.Object,
            mockPageHandler.Object,
            mockPageStorage.Object,
            mockLogger.Object,
            mockUrlQueueManager.Object,
            contentExtractor
        );

        // Act
        var result = await scraper.ScrapAsync(new List<DownloadRequest>
        {
            new DownloadRequest("https://example.com", 1)
        });

        // Assert

        // Only three downloads should have been made (main page and two first-level links)
        mockPageDownloader.Verify(pd => pd.DownloadPageContentAsync("https://example.com"), Times.Once);
        mockPageDownloader.Verify(pd => pd.DownloadPageContentAsync("https://example.com/page1"), Times.Once);
        mockPageDownloader.Verify(pd => pd.DownloadPageContentAsync("https://example.com/page2"), Times.Once);

        // Ensure that second-level pages were not downloaded
        mockPageDownloader.Verify(pd => pd.DownloadPageContentAsync("https://example.com/subpage1"), Times.Never);
        mockPageDownloader.Verify(pd => pd.DownloadPageContentAsync("https://example.com/subpage2"), Times.Never);
        mockPageDownloader.Verify(pd => pd.DownloadPageContentAsync("https://example.com/subpage3"), Times.Never);
        mockPageDownloader.Verify(pd => pd.DownloadPageContentAsync("https://example.com/subpage4"), Times.Never);

        // Verify that saves were called only for 0 pages (as no content)
        mockPageStorage.Verify(ps => ps.SavePageAsync(It.IsAny<ScrapedWebPage>()), Times.Exactly(0));

        // Verify that UrlQueueManager was called with the correct URLs
        mockUrlQueueManager.Verify(um => um.CanAddUrl("https://example.com/page1"), Times.Once);
        mockUrlQueueManager.Verify(um => um.CanAddUrl("https://example.com/page2"), Times.Once);
        mockUrlQueueManager.Verify(um => um.CanAddUrl("https://example.com/subpage1"), Times.Never);
        mockUrlQueueManager.Verify(um => um.CanAddUrl("https://example.com/subpage2"), Times.Never);
        mockUrlQueueManager.Verify(um => um.CanAddUrl("https://example.com/subpage3"), Times.Never);
        mockUrlQueueManager.Verify(um => um.CanAddUrl("https://example.com/subpage4"), Times.Never);
    }
}
