﻿using System.Net;
using Microsoft.Extensions.Logging;
using Service;

namespace Crawler.Service.Tests;
public class LinkClientTests : TestBase
{
    private readonly Mock<ILoggerFactory> _mockLogger;

    public LinkClientTests()
    {

        var logger = new Mock<ILogger<LinkClient>>();
        _mockLogger = new Mock<ILoggerFactory>();
        _mockLogger.Setup(lc => lc.CreateLogger(It.IsAny<string>())).Returns(() => logger.Object);
    }

    [Fact]
    public async Task GetLinkContentAsync_OnSuccessCallToHtmlPage_ShouldReturnPageContent()
    {
        // arrange
        const string expectedContent = "test content";
        var httpClient = CreateFakeHttpClient(expectedContent);
        var linkClient = new LinkClient(httpClient, _mockLogger.Object);
        var uri = new Uri("https://monzo.com");

        // act
        var actual = await linkClient.GetLinkContentAsync(uri);

        // assert
        actual.Should().Be(expectedContent);
    }

    [Theory]
    [InlineData(HttpStatusCode.NotFound)]
    [InlineData(HttpStatusCode.GatewayTimeout)]
    public async Task GetLinkContentAsync_OnErrorCall_ShouldReturnEmptyString(HttpStatusCode expectedStatusCode)
    {
        // arrange
        var expectedContent = string.Empty;
        var httpClient = CreateFakeHttpClient(expectedContent, expectedStatusCode);
        var linkClient = new LinkClient(httpClient, _mockLogger.Object);
        var uri = new Uri("https://monzo.com");

        // act
        var actual = await linkClient.GetLinkContentAsync(uri);

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
    public async Task GetLinkContentAsync_WhenResponseContentTypeIsNotHtml_ShouldReturnEmptyString(string expectedContentType)
    {
        // arrange
        const string expectedContent = "test irrelevant content";
        var httpClient = CreateFakeHttpClient(expectedContent, HttpStatusCode.OK, expectedContentType);
        var linkClient = new LinkClient(httpClient, _mockLogger.Object);
        var uri = new Uri("https://monzo.com");

        // act
        var actual = await linkClient.GetLinkContentAsync(uri);

        // assert
        actual.Should().Be(string.Empty);
    }
}

