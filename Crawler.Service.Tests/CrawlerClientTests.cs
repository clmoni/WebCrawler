using System.Net;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Service;
using Xunit;

namespace Crawler.Service.Tests;
public class CrawlerClientTests : TestBase
{
    private Mock<ILoggerFactory> _mockLogger;

    public CrawlerClientTests()
    {

        var logger = new Mock<ILogger<CrawlerClient>>();
        _mockLogger = new Mock<ILoggerFactory>();
        _mockLogger.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(() => logger.Object);
    }

    [Fact]
    public async Task GetPageContent_OnSuccessCallToHtmlPage_ShouldReturnPageContent()
    {
        // arrange
        var expectedContent = "test content";
        var httpClient = CreateFakeHttpClient(expectedContent, HttpStatusCode.OK, "text/html");
        var cc = new CrawlerClient(httpClient, _mockLogger.Object);
        var uri = new Uri("https://monzo.com");

        // act
        var actual = await cc.GetPageContentAsync(uri);

        // assert
        actual.Should().Be(expectedContent);
    }

    [Theory]
    [InlineData(HttpStatusCode.NotFound)]
    [InlineData(HttpStatusCode.GatewayTimeout)]
    public async Task GetPageContent_OnErrorCall_ShouldReturnEmptyString(HttpStatusCode expectedStatusCode)
    {
        // arrange
        var expectedContent = string.Empty;
        var httpClient = CreateFakeHttpClient(expectedContent, expectedStatusCode);
        var cc = new CrawlerClient(httpClient, _mockLogger.Object);
        var uri = new Uri("https://monzo.com");

        // act
        var actual = await cc.GetPageContentAsync(uri);

        // assert
        actual.Should().Be(expectedContent);
    }

    [Theory]
    [InlineData("application/pdf")]
    [InlineData("application/json")]
    [InlineData("text/xml")]
    [InlineData("text/css")]
    [InlineData("image/png")]
    [InlineData("application/x-www-form-urlencoded")]
    public async Task GetPageContent_WhenResponseContentTypeIsNotHtml_ShouldReturnEmptyString(string expectedContentType)
    {
        // arrange
        var expectedContent = "test irrelevant content";
        var httpClient = CreateFakeHttpClient(expectedContent, HttpStatusCode.OK, expectedContentType);
        var cc = new CrawlerClient(httpClient, _mockLogger.Object);
        var uri = new Uri("https://monzo.com");

        // act
        var actual = await cc.GetPageContentAsync(uri);

        // assert
        actual.Should().Be(string.Empty);
    }
}

