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

            // Стартовая страница содержит две ссылки
            var rootPageContent = "<html><a href='https://example.com/page1'></a><a href='https://example.com/page2'></a></html>";
            // Первая дочерняя страница содержит две ссылки (они не должны быть скачаны)
            var page1Content = "<html><a href='https://example.com/subpage1'></a><a href='https://example.com/subpage2'></a></html>";
            // Вторая дочерняя страница также содержит две ссылки (они тоже не должны быть скачаны)
            var page2Content = "<html><a href='https://example.com/subpage3'></a><a href='https://example.com/subpage4'></a></html>";

            // Настройка моков для загрузчика страниц
            mockPageDownloader.Setup(pd => pd.DownloadPageContentAsync("https://example.com"))
                .ReturnsAsync(rootPageContent);
            mockPageDownloader.Setup(pd => pd.DownloadPageContentAsync("https://example.com/page1"))
                .ReturnsAsync(page1Content);
            mockPageDownloader.Setup(pd => pd.DownloadPageContentAsync("https://example.com/page2"))
                .ReturnsAsync(page2Content);

            // Настройка моков для обработчика страниц
            mockPageHandler.Setup(ph => ph.ExtractLinksFromPage(rootPageContent, "https://example.com"))
                .Returns(new List<string> { "https://example.com/page1", "https://example.com/page2" });
            mockPageHandler.Setup(ph => ph.ExtractLinksFromPage(page1Content, "https://example.com/page1"))
                .Returns(new List<string> { "https://example.com/subpage1", "https://example.com/subpage2" });
            mockPageHandler.Setup(ph => ph.ExtractLinksFromPage(page2Content, "https://example.com/page2"))
                .Returns(new List<string> { "https://example.com/subpage3", "https://example.com/subpage4" });

            // Настройка моков для UrlQueueManager - первые три ссылки можно добавлять, остальные нельзя
            mockUrlQueueManager.Setup(um => um.CanAddUrl("https://example.com")).Returns(true);
            mockUrlQueueManager.Setup(um => um.CanAddUrl("https://example.com/page1")).Returns(true);
            mockUrlQueueManager.Setup(um => um.CanAddUrl("https://example.com/page2")).Returns(true);
            mockUrlQueueManager.Setup(um => um.CanAddUrl(It.Is<string>(url =>
                url == "https://example.com/subpage1" ||
                url == "https://example.com/subpage2" ||
                url == "https://example.com/subpage3" ||
                url == "https://example.com/subpage4"))).Returns(false);

            // Создаем объект WebScraper с глубиной скачивания 1
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
                new DownloadRequest("https://example.com", 1) // Ограничение по глубине - 1
            });

            // Assert

            // Должны быть сделаны только три загрузки (главная страница и две ссылки первого уровня)
            mockPageDownloader.Verify(pd => pd.DownloadPageContentAsync("https://example.com"), Times.Once);
            mockPageDownloader.Verify(pd => pd.DownloadPageContentAsync("https://example.com/page1"), Times.Once);
            mockPageDownloader.Verify(pd => pd.DownloadPageContentAsync("https://example.com/page2"), Times.Once);

            // Убедимся, что страницы второго уровня не скачивались
            mockPageDownloader.Verify(pd => pd.DownloadPageContentAsync("https://example.com/subpage1"), Times.Never);
            mockPageDownloader.Verify(pd => pd.DownloadPageContentAsync("https://example.com/subpage2"), Times.Never);
            mockPageDownloader.Verify(pd => pd.DownloadPageContentAsync("https://example.com/subpage3"), Times.Never);
            mockPageDownloader.Verify(pd => pd.DownloadPageContentAsync("https://example.com/subpage4"), Times.Never);

            // Проверка, что были вызваны сохранения только для 3-х страниц
            mockPageStorage.Verify(ps => ps.SavePageAsync(It.IsAny<ScrapedWebPage>()), Times.Exactly(3));

            // Проверка, что UrlQueueManager был вызван с правильными URL
            mockUrlQueueManager.Verify(um => um.CanAddUrl("https://example.com/page1"), Times.Once);
            mockUrlQueueManager.Verify(um => um.CanAddUrl("https://example.com/page2"), Times.Once);
            mockUrlQueueManager.Verify(um => um.CanAddUrl("https://example.com/subpage1"), Times.Once);
            mockUrlQueueManager.Verify(um => um.CanAddUrl("https://example.com/subpage2"), Times.Once);
            mockUrlQueueManager.Verify(um => um.CanAddUrl("https://example.com/subpage3"), Times.Once);
            mockUrlQueueManager.Verify(um => um.CanAddUrl("https://example.com/subpage4"), Times.Once);
        }
    }
}
