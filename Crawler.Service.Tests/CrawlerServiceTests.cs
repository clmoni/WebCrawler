using System;
using FluentAssertions;
using Moq;
using Service;
using Service.Abstractions;

namespace Crawler.Service.Tests
{
	public class CrawlerServiceTests: TestBase
	{
		private Mock<ICrawlerClient> _mockCrawlerClient;

		public CrawlerServiceTests()
		{
			_mockCrawlerClient = new Mock<ICrawlerClient>();
        }

		[Fact]
		public async Task FindLinks_WhenLinksFoundInContent_ReturnsListOfLinks()
		{
			// arrange
			var expectedUri = new Uri("http://test");
			_mockCrawlerClient.Setup(c => c.GetPageContentAsync(It.Is<Uri>(u => u == expectedUri))).ReturnsAsync(TestWebPageWithLinks);
			var crawlerService = new CrawlerService(_mockCrawlerClient.Object);

			// act
			var actual = await crawlerService.FindLinks(expectedUri);

			// assert
			actual.Count.Should().Be(3);
			actual.Should().Contain("https://www.link1.com");
            actual.Should().Contain("https://www.link2.com");
            actual.Should().Contain("https://www.link3.com");
        }

        [Theory]
        [InlineData("")]
        [InlineData("    ")]
        [InlineData(" ")]
        public async Task FindLinks_EmptyContentIsFound_ReturnsEmptyListOfLinks(string expectedContent)
        {
            // arrange
            var expectedUri = new Uri("http://test");
            _mockCrawlerClient.Setup(c => c.GetPageContentAsync(It.Is<Uri>(u => u == expectedUri))).ReturnsAsync(expectedContent);
            var crawlerService = new CrawlerService(_mockCrawlerClient.Object);

            // act
            var actual = await crawlerService.FindLinks(expectedUri);

            // assert
            actual.Count.Should().Be(0);
        }

        [Fact]
        public async Task FindLinks_NoLinksFoundInPageContent_ReturnsEmptyListOfLinks()
        {
            // arrange
            var expectedUri = new Uri("http://test");
            _mockCrawlerClient.Setup(c => c.GetPageContentAsync(It.Is<Uri>(u => u == expectedUri))).ReturnsAsync(TestWebPageWithNoLinks);
            var crawlerService = new CrawlerService(_mockCrawlerClient.Object);

            // act
            var actual = await crawlerService.FindLinks(expectedUri);

            // assert
            actual.Count.Should().Be(0);
        }
    }
}

