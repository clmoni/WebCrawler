using Service;
using Service.Abstractions;

namespace Crawler.Service.Tests
{
	public class LinkServiceTests: TestBase
	{
		private readonly Mock<ILinkClient> _mockLinkClient;

		public LinkServiceTests()
		{
			_mockLinkClient = new Mock<ILinkClient>();
        }

		[Fact]
		public async Task FindChildLinksAsync_WhenLinksFoundInContent_ReturnsListOfLinks()
		{
			// arrange
			var expectedUri = new Uri("https://www.link.com");
			_mockLinkClient.Setup(c => c.GetLinkContentAsync(It.Is<Uri>(u => u == expectedUri))).ReturnsAsync(TestWebPageWithLinks);
			var linkService = new LinkService(_mockLinkClient.Object);

			// act
			var actual = await linkService.FindChildLinksAsync(expectedUri);

			// assert
			actual.Count.Should().Be(4);
			actual[0].Uri.Should().Be("https://www.link.com/one");
            actual[1].Uri.Should().Be("https://www.link.com/two");
            actual[2].Uri.Should().Be("https://www.link.com/three");
            actual[3].Uri.Should().Be("https://www.link.com/four");
        }
		
		[Fact]
		public async Task FindChildLinksAsync_EmptyPageContent_ReturnsEmptyListOfLinks()
		{
			// arrange
			var expectedUri = new Uri("https://www.link.com");
			_mockLinkClient.Setup(c => c.GetLinkContentAsync(It.Is<Uri>(u => u == expectedUri))).ReturnsAsync(string.Empty);
			var linkService = new LinkService(_mockLinkClient.Object);

			// act
			var actual = await linkService.FindChildLinksAsync(expectedUri);

			// assert
			actual.Should().BeEmpty();
		}

        [Theory]
        [InlineData("")]
        [InlineData("    ")]
        [InlineData(" ")]
        public async Task FFindChildLinksAsync_EmptyContentIsFound_ReturnsEmptyListOfLinks(string expectedContent)
        {
            // arrange
            var expectedUri = new Uri("http://test");
            _mockLinkClient.Setup(c => c.GetLinkContentAsync(It.Is<Uri>(u => u == expectedUri))).ReturnsAsync(expectedContent);
            var linkService = new LinkService(_mockLinkClient.Object);

            // act
            var actual = await linkService.FindChildLinksAsync(expectedUri);

            // assert
            actual.Count.Should().Be(0);
        }

        [Fact]
        public async Task FindChildLinksAsync_NoLinksFoundInPageContent_ReturnsEmptyListOfLinks()
        {
            // arrange
            var expectedUri = new Uri("http://test");
            _mockLinkClient.Setup(c => c.GetLinkContentAsync(It.Is<Uri>(u => u == expectedUri))).ReturnsAsync(TestWebPageWithNoLinks);
            var linkService = new LinkService(_mockLinkClient.Object);

            // act
            var actual = await linkService.FindChildLinksAsync(expectedUri);

            // assert
            actual.Count.Should().Be(0);
        }
    }
}

