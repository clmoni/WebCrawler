using Services;
using Services.Abstractions;

namespace Crawler.Service.Tests.UnitTests
{
	public class LinkServiceTests: TestBase
	{
		public LinkServiceTests()
		{
			MockLinkClient = new Mock<ILinkClient>();
        }

		[Fact]
		public async Task FindChildLinksAsync_WhenLinksFoundInContent_ReturnsListOfLinks()
		{
			// arrange
			var expectedUri = new Uri("https://www.link.com");
			MockLinkClient.Setup(c => c.GetLinkContentAsync(It.Is<Uri>(u => u == expectedUri))).ReturnsAsync(TestWebPageWithLinks);
			var linkService = new LinkService(MockLinkClient.Object);

			// act
			var actual = await linkService.FindChildLinksAsync(expectedUri);

			// assert
			actual.VisitableLinks.Count.Should().Be(4);
			actual.VisitableLinks[0].Uri.Should().Be("https://www.link.com/one");
            actual.VisitableLinks[1].Uri.Should().Be("https://www.link.com/two");
            actual.VisitableLinks[2].Uri.Should().Be("https://www.link.com/three");
            actual.VisitableLinks[3].Uri.Should().Be("https://www.link.com/four");
        }
		
		[Fact]
		public async Task FindChildLinksAsync_EmptyPageContent_ReturnsEmptyListOfLinks()
		{
			// arrange
			var expectedUri = new Uri("https://www.link.com");
			MockLinkClient.Setup(c => c.GetLinkContentAsync(It.Is<Uri>(u => u == expectedUri))).ReturnsAsync(string.Empty);
			var linkService = new LinkService(MockLinkClient.Object);

			// act
			var actual = await linkService.FindChildLinksAsync(expectedUri);

			// assert
			actual.VisitableLinks.Should().BeEmpty();
		}

        [Theory]
        [InlineData("")]
        [InlineData("    ")]
        [InlineData(" ")]
        public async Task FFindChildLinksAsync_EmptyContentIsFound_ReturnsEmptyListOfLinks(string expectedContent)
        {
            // arrange
            var expectedUri = new Uri("http://test");
            MockLinkClient.Setup(c => c.GetLinkContentAsync(It.Is<Uri>(u => u == expectedUri))).ReturnsAsync(expectedContent);
            var linkService = new LinkService(MockLinkClient.Object);

            // act
            var actual = await linkService.FindChildLinksAsync(expectedUri);

            // assert
            actual.VisitableLinks.Count.Should().Be(0);
        }

        [Fact]
        public async Task FindChildLinksAsync_NoLinksFoundInPageContent_ReturnsEmptyListOfLinks()
        {
            // arrange
            var expectedUri = new Uri("http://test");
            MockLinkClient.Setup(c => c.GetLinkContentAsync(It.Is<Uri>(u => u == expectedUri))).ReturnsAsync(TestWebPageWithNoLinks);
            var linkService = new LinkService(MockLinkClient.Object);

            // act
            var actual = await linkService.FindChildLinksAsync(expectedUri);

            // assert
            actual.VisitableLinks.Count.Should().Be(0);
        }
    }
}

