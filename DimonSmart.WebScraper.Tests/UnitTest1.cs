using Moq;
using Xunit;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DimonSmart.WebScraper.Tests
{
    public class WebScraperTests
    {
        [Fact]
        public async Task Test_WebScraper_ProcessesRequests_With_Depth_Restriction()
        {
            // Arrange
            var mockPageDownloader = new Mock<IPageDownloader>();
            var mockPageHandler = new Mock<IPageHandler>();
            var mockPageStorage = new Mock<IPageStorage>();
            var mockLogger = new Mock<ILogger>();
            var mockUrlQueueManager = new Mock<IUrlQueueManager>();

            // ��������� �������� �������� ��� ������
            var rootPageContent = "<html><a href='https://example.com/page1'></a><a href='https://example.com/page2'></a></html>";
            // ������ �������� �������� �������� ��� ������ (��� �� ������ ���� �������)
            var page1Content = "<html><a href='https://example.com/subpage1'></a><a href='https://example.com/subpage2'></a></html>";
            // ������ �������� �������� ����� �������� ��� ������ (��� ���� �� ������ ���� �������)
            var page2Content = "<html><a href='https://example.com/subpage3'></a><a href='https://example.com/subpage4'></a></html>";

            // ��������� ����� ��� ���������� �������
            mockPageDownloader.Setup(pd => pd.DownloadPageContentAsync("https://example.com"))
                .ReturnsAsync(rootPageContent);
            mockPageDownloader.Setup(pd => pd.DownloadPageContentAsync("https://example.com/page1"))
                .ReturnsAsync(page1Content);
            mockPageDownloader.Setup(pd => pd.DownloadPageContentAsync("https://example.com/page2"))
                .ReturnsAsync(page2Content);

            // ��������� ����� ��� ����������� �������
            mockPageHandler.Setup(ph => ph.ExtractLinksFromPage(rootPageContent, "https://example.com"))
                .Returns(new List<string> { "https://example.com/page1", "https://example.com/page2" });
            mockPageHandler.Setup(ph => ph.ExtractLinksFromPage(page1Content, "https://example.com/page1"))
                .Returns(new List<string> { "https://example.com/subpage1", "https://example.com/subpage2" });
            mockPageHandler.Setup(ph => ph.ExtractLinksFromPage(page2Content, "https://example.com/page2"))
                .Returns(new List<string> { "https://example.com/subpage3", "https://example.com/subpage4" });

            // ��������� ����� ��� UrlQueueManager - ������ ��� ������ ����� ���������, ��������� ������
            mockUrlQueueManager.Setup(um => um.CanAddUrl("https://example.com")).Returns(true);
            mockUrlQueueManager.Setup(um => um.CanAddUrl("https://example.com/page1")).Returns(true);
            mockUrlQueueManager.Setup(um => um.CanAddUrl("https://example.com/page2")).Returns(true);
            mockUrlQueueManager.Setup(um => um.CanAddUrl(It.Is<string>(url =>
                url == "https://example.com/subpage1" ||
                url == "https://example.com/subpage2" ||
                url == "https://example.com/subpage3" ||
                url == "https://example.com/subpage4"))).Returns(false);

            // ������� ������ WebScraper � �������� ���������� 1
            var scraper = new WebScraper(
                maxThreads: 2,
                pageDownloader: mockPageDownloader.Object,
                pageHandler: mockPageHandler.Object,
                pageStorage: mockPageStorage.Object,
                logger: mockLogger.Object,
                urlQueueManager: mockUrlQueueManager.Object
            );

            // Act
            var result = await scraper.ScrapAsync(new List<DownloadRequest>
            {
                new DownloadRequest("https://example.com", 1) // ����������� �� ������� - 1
            });

            // Assert

            // ������ ���� ������� ������ ��� �������� (������� �������� � ��� ������ ������� ������)
            mockPageDownloader.Verify(pd => pd.DownloadPageContentAsync("https://example.com"), Times.Once);
            mockPageDownloader.Verify(pd => pd.DownloadPageContentAsync("https://example.com/page1"), Times.Once);
            mockPageDownloader.Verify(pd => pd.DownloadPageContentAsync("https://example.com/page2"), Times.Once);

            // ��������, ��� �������� ������� ������ �� �����������
            mockPageDownloader.Verify(pd => pd.DownloadPageContentAsync("https://example.com/subpage1"), Times.Never);
            mockPageDownloader.Verify(pd => pd.DownloadPageContentAsync("https://example.com/subpage2"), Times.Never);
            mockPageDownloader.Verify(pd => pd.DownloadPageContentAsync("https://example.com/subpage3"), Times.Never);
            mockPageDownloader.Verify(pd => pd.DownloadPageContentAsync("https://example.com/subpage4"), Times.Never);

            // ��������, ��� ���� ������� ���������� ������ ��� 3-� �������
            mockPageStorage.Verify(ps => ps.SavePageAsync(It.IsAny<ScrapedWebPage>()), Times.Exactly(3));

            // ��������, ��� UrlQueueManager ��� ������ � ����������� URL
            mockUrlQueueManager.Verify(um => um.CanAddUrl("https://example.com/page1"), Times.Once);
            mockUrlQueueManager.Verify(um => um.CanAddUrl("https://example.com/page2"), Times.Once);
            mockUrlQueueManager.Verify(um => um.CanAddUrl("https://example.com/subpage1"), Times.Once);
            mockUrlQueueManager.Verify(um => um.CanAddUrl("https://example.com/subpage2"), Times.Once);
            mockUrlQueueManager.Verify(um => um.CanAddUrl("https://example.com/subpage3"), Times.Once);
            mockUrlQueueManager.Verify(um => um.CanAddUrl("https://example.com/subpage4"), Times.Once);
        }
    }
}
