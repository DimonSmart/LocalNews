using Microsoft.Extensions.Options;
using Moq;

namespace DimonSmart.WebScraper.Tests;

public class FileUrlRepositoryTests
{
    private FileUrlRepository CreateRepositoryWithMockedSettings(string prohibitedUrlsFileName)
    {
        var settings = new WebScraperSettings
        {
            ProhibitedUrlsFileName = prohibitedUrlsFileName
        };

        var mockOptions = new Mock<IOptions<WebScraperSettings>>();
        mockOptions.Setup(o => o.Value).Returns(settings);

        return new FileUrlRepository(mockOptions.Object);
    }

    [Theory]
    [InlineData("https://www.youtube.com/channel/UCGTxCFAaIMDjrwB8S4OAbyw")]
    [InlineData("https://www.pinterest.es/malagaciudad/_created/")]
    [InlineData("https://www.facebook.com/MalagaTurismoOficial/")]
    public void ContainsProhibitedUrl_ShouldReturnTrue_ForProhibitedUrls(string url)
    {
        // Arrange
        var repository = CreateRepositoryWithMockedSettings("prohibited_urls.txt");

        // Act
        var result = repository.ContainsProhibitedUrl(url);

        // Assert
        Assert.True(result);
    }
}
