using System;
using Moq;
using Moq.Protected;
using System.Net;
using System.Reflection.Metadata;
using System.Net.Http.Headers;

namespace Crawler.Service.Tests
{
    public class TestBase
    {
        private Mock<HttpMessageHandler>? _handlerMock;

        protected static string TestWebPageWithLinks =>
            @"<!DOCTYPE html>
            <html>
            <body><a href='https://www.link1.com'>This is a link</a>
            <a href='https://www.link2.com'>This is a link</a>
            <a href='https://www.link3.com'>This is a link   \\n</a>
            <a href='https://www.link2.com'>This is a link</body>
            </html>";

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

