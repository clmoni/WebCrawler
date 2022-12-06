using System;
using Models;

namespace Crawler.Service.Tests
{
	public class LinkTests
	{
		[Theory]
		[InlineData("http://test.com/about", "http://test.com", "http://test.com")]
        [InlineData("https://test.com/about", "https://test.com", "https://test.com")]
        [InlineData("test/about", "http://test.com", "http://test.com")]
        [InlineData("https://test.com/contact", "https://test.com?test=test", "https://test.com")]
        [InlineData("https://test.com/contact", "https://test.com/test.html", "https://test.com")]
        public void Constructor_GivenFullyQualifiedOriginalLinkAndParentLink_ShouldIniitaliseWithFullyQualifiedUriAndBeVisitable(string expectedOriginalLink, string parentLink, string expectedParentLink)
		{
			// act
			var actual = new Link(expectedOriginalLink, parentLink);

			// assert
			actual.Should().Match<Link>(l =>
			l.OriginalLink.Equals(expectedOriginalLink) &&
			l.ParentLink.Equals(expectedParentLink) &&
			l.IsOriginalLinkFullQualified == true &&
			l.IsLinkToBeVisited == true &&
			l.FullyQualifiedUri == new Uri(expectedOriginalLink));
		}

        [Theory]
        [InlineData("mailto:name@email.com")]
        [InlineData("sms:+18664504185")]
        [InlineData("tel:+18664504185")]
        [InlineData("#element")]
        public void Constructor_GivenNonFullyQualifiedOriginalLinkAndParentLink_ShouldIniitaliseWithNullUriAndNotBeVisitable(string expectedOriginalLink)
        {
            // arrange
            var expectedParentLink = "http://test.com";

            // act
            var actual = new Link(expectedOriginalLink, expectedParentLink);

            // assert
            actual.Should().Match<Link>(l =>
            l.OriginalLink.Equals(expectedOriginalLink) &&
            l.ParentLink.Equals(expectedParentLink) &&
            l.IsOriginalLinkFullQualified == false &&
            l.IsLinkToBeVisited == false &&
            l.FullyQualifiedUri == default);
        }
    }
}

