using Moq.Protected;
using System.Net;
using System.Net.Http.Headers;

namespace Crawler.Service.Tests
{
    public class TestBase
    {
        private Mock<HttpMessageHandler>? _handlerMock;

        protected static string TestWebPageWithLinks =>
            @"<!DOCTYPE html>
            <html>
            <body><a href='https://www.link.com/one'>This is a link</a>
            <a href='https://www.link.com/two'>This is a link</a>
            <a href='https://www.link.com/three'>This is a link   \\n</a>
            <a href='https://www.link.com/four'>This is a link   \\n</a>
            <a href='/four'>This is a link</body>
            </html>";       
        
        protected static string TestRobotsTxt =>
            @"# robotstxt.org/

            User-agent: *
            Disallow: /docs/
            Disallow: /referral/
            Disallow: /-staging-referral/
            Disallow: /install/
            Disallow: /blog/authors/
            Disallow: /pay/
            Disallow: /-deeplinks/";

        protected static string TestWebPageWithNoLinks =>
            @"<!DOCTYPE html>
            <html>
            <body></body>
            </html>";

        public HttpClient CreateFakeHttpClient(string expectedContent, HttpStatusCode statusCode = HttpStatusCode.OK, string responseContentType = "text/html")
        {
            var response = new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(expectedContent),
            };

            response.Content.Headers.ContentType = new MediaTypeHeaderValue(responseContentType);

            _handlerMock = new Mock<HttpMessageHandler>();
            _handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(response)
                .Verifiable();

            return new(_handlerMock.Object);
        }

    }
}

