using Moq.Protected;
using System.Net;
using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;
using Models;
using Service.Abstractions;

namespace Crawler.Service.Tests
{
    public abstract class TestBase
    {
        private Mock<HttpMessageHandler>? _handlerMock;
        protected Mock<IQueueManager> MockQueueManager;
        protected Mock<ILinkService> MockLinkService;
        protected Mock<ILinkRepository> MockLinkRepository;
        protected Mock<ILoggerFactory> MockLoggerFactory;
        protected Mock<ILinkClient> MockLinkClient;

        protected TestBase()
        {
            MockQueueManager = new Mock<IQueueManager>();
            MockLinkService = new Mock<ILinkService>();
            MockLinkRepository = new Mock<ILinkRepository>();
            MockLoggerFactory = new Mock<ILoggerFactory>();
            MockLinkClient = new Mock<ILinkClient>();
        }
        
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

        protected HttpClient CreateFakeHttpClient(string expectedContent, HttpStatusCode statusCode = HttpStatusCode.OK, string responseContentType = "text/html")
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

            return new HttpClient(_handlerMock.Object);
        }

        protected void MockSuccessCalls(string expectedParent, string expectedChild)
        {
            var expectedChildLink = new Link(expectedChild, expectedParent);
            MockLinkService.SetupSequence(ls => ls.FindChildLinksAsync(It.IsAny<Uri>()))
                .ReturnsAsync(new PageResult (1,new[]{ expectedChildLink }))
                .ReturnsAsync(new PageResult(0, new List<Link>()));

            MockLinkRepository.Setup(lr => lr.IsAlreadyVisited(
                    It.Is<Uri>(u => u.Equals(new Uri(expectedParent)))))
                .Returns(false);

            MockQueueManager.SetupSequence(qm => qm.Dequeue(out expectedChildLink))
                .Returns(true)
                .Returns(false);
        }
        
        protected void VerifyCrawl(string expectedParent, string expectedChild)
        {
            var expectedChildLink = new Link(expectedChild, expectedParent);

            MockLinkService.Verify(ms => ms.FindChildLinksAsync(It.IsAny<Uri>()), Times.Exactly(2));

            MockLinkRepository.Verify(lr => lr.AddVisited(
                It.Is<Uri>(u => u.Equals(new Uri(expectedParent)))), Times.Once);

            MockLinkRepository.Verify(lr => lr.AddChildOfVisited(
                It.Is<Uri>(u => u.Equals(new Uri(expectedParent))),
                It.Is<Uri>(u => u.Equals(new Uri(expectedChild)))), Times.Once);

            MockLinkRepository.Verify(lr => lr.IsAlreadyVisited(
                It.Is<Uri>(u => u.Equals(new Uri(expectedChild)))), Times.Once);

            MockQueueManager.Verify(qm => qm.Enqueue(
                It.Is<Link>(l => l.Uri != null && l.Uri.Equals(expectedChildLink.Uri))), Times.Once);

            MockQueueManager.Verify(qm => qm.Dequeue(out expectedChildLink), Times.Exactly(2));

            MockLinkRepository.Verify(lr => lr.AddVisited(
                It.Is<Uri>(u => u.Equals(new Uri(expectedChild)))), Times.Once);
        }
        
        protected void MockAlreadyVisitedCalls(string expectedParent, string expectedChild)
        {
            var expectedChildLink = new Link(expectedChild, expectedParent);
            MockLinkService.Setup(ls => ls.FindChildLinksAsync(It.IsAny<Uri>()))
                .ReturnsAsync( new PageResult(1, new[] {expectedChildLink}) );
            
            MockLinkRepository.SetupSequence(lr => lr.IsAlreadyVisited(
                    It.IsAny<Uri>()))
                .Returns(false)
                .Returns(true);

            MockQueueManager.SetupSequence(qm => qm.Dequeue(out expectedChildLink))
                .Returns(true)
                .Returns(true)
                .Returns(false);
        }
        
        protected void VerifyAlreadyVisitedCrawl(string expectedParent, string expectedChild)
        {
            var expectedChildLink = new Link(expectedChild, expectedParent);

            MockLinkService.Verify(ms => ms.FindChildLinksAsync(It.IsAny<Uri>()), Times.Exactly(3));

            MockLinkRepository.Verify(lr => lr.AddVisited(
                It.Is<Uri>(u => u.Equals(new Uri(expectedParent)))), Times.Once);
            
            MockLinkRepository.Verify(lr => lr.AddChildOfVisited(
                It.Is<Uri>(u => u.Equals(new Uri(expectedParent))),
                It.Is<Uri>(u => u.Equals(new Uri(expectedChild)))), Times.Exactly(3));
            
            MockLinkRepository.Verify(lr => lr.IsAlreadyVisited(
                It.Is<Uri>(u => u.Equals(new Uri(expectedChild)))), Times.Exactly(3));
            
            MockQueueManager.Verify(qm => qm.Enqueue(
                It.Is<Link>(l => l.Uri != null && l.Uri.Equals(expectedChildLink.Uri))), Times.Exactly(2));
            
            MockQueueManager.Verify(qm => qm.Dequeue(out expectedChildLink), Times.Exactly(3));
            
            MockLinkRepository.Verify(lr => lr.AddVisited(
                It.Is<Uri>(u => u.Equals(new Uri(expectedChild)))), Times.Exactly(2));
        }
    }
}

