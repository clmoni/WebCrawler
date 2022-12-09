using Models;

namespace Crawler.Service.Tests.UnitTests
{
	public class LinkTests
	{
		[Theory]
		[InlineData("http://test.com/about", "http://test.com/about", "http://test.com", "http://test.com")]
        [InlineData("https://test.com/about", "https://test.com/about", "https://test.com", "https://test.com")]
        [InlineData("/about", "http://test.com/about", "http://test.com", "http://test.com")]
        [InlineData("about", "http://test.com/about", "http://test.com", "http://test.com")]
        [InlineData("/about", "http://test.com/about", "http://test.com/contact", "http://test.com/contact")]
        [InlineData("about", "http://test.com/about", "http://test.com/contact", "http://test.com/contact")]
        [InlineData("https://test.com/contact", "https://test.com/contact", "https://test.com?test=test", "https://test.com?test=test")]
        [InlineData("https://test.com/contact", "https://test.com/contact", "https://test.com/test.html", "https://test.com/test.html")]
        [InlineData("robots.txt", "https://test.com/robots.txt", "https://test.com", "https://test.com")]
        public void Constructor_GivenFullyQualifiedOriginalLinkAndParentLink_ShouldIniitaliseWithFullyQualifiedUriAndBeVisitable(string expectedOriginalLink, string expectedChildLink,string parentLink, string expectedParentLink)
		{
			// act
			var actual = new Link(expectedOriginalLink, parentLink);

			// assert
			actual.Should().Match<Link>(l =>
				l.RawChildLink.Equals(expectedOriginalLink) &&
				l.Uri != null &&
				l.Uri.Equals(new Uri(expectedChildLink)) &&
				l.ParentLink.Equals(new Uri(expectedParentLink)) &&
				l.IsCrawlableLink(new Uri(parentLink)) == true);
		}
        
        [Theory]
        [InlineData("http://test.com/about", "http://test.com/about")]
        [InlineData("https://test.com", "https://test.com/")]
        [InlineData("https://test.com/", "https://test.com")]
        [InlineData("https://test.com/", "https://test.com/")]
        [InlineData("/", "https://test.com/")]
        public void Constructor_GivenChildLinkSameAsParent_ShouldNotBeVisitable(string expectedOriginalLink,string parentLink)
        {
	        // act
	        var actual = new Link(expectedOriginalLink, parentLink);

	        // assert
	        actual.Should().Match<Link>(l =>
		        l.RawChildLink.Equals(expectedOriginalLink) &&
		        l.Uri == null &&
		        l.IsCrawlableLink(new Uri(parentLink)) == false);
        }

        [Theory]
        [InlineData("mailto:name@email.com")]
        [InlineData("sms:+18664504185")]
        [InlineData("tel:+18664504185")]
        [InlineData("#element")]
        public void Constructor_GivenNonFullyQualifiedOriginalLinkAndParentLink_ShouldIniitaliseWithNullUriAndNotBeVisitable(string expectedOriginalLink)
        {
            // arrange
            const string expectedParentLink = "http://test.com";

            // act
            var actual = new Link(expectedOriginalLink, expectedParentLink);

            // assert
            actual.Should().Match<Link>(l =>
	            l.RawChildLink == expectedOriginalLink &&
	            l.Uri == null &&
	            l.ParentLink.Equals(new Uri(expectedParentLink)) &&
	            l.IsCrawlableLink(new Uri(expectedParentLink)) == false);
        }
    }
}

