using Microsoft.Extensions.Logging;
using Service;
using Service.Abstractions;

namespace Crawler.Service.Tests.UnitTests;

public class CrawlerEngineTests: TestBase
{
    public CrawlerEngineTests()
    {
        MockLinkService = new Mock<ILinkService>();
        MockLinkRepository = new Mock<ILinkRepository>();
        MockLoggerFactory = new Mock<ILoggerFactory>();
        MockQueueManager = new Mock<IQueueManager>();
        
        var logger = new Mock<ILogger<CrawlerEngine>>();
        MockLoggerFactory = new Mock<ILoggerFactory>();
        MockLoggerFactory.Setup(lc => lc.CreateLogger(It.IsAny<string>())).Returns(() => logger.Object);
    }


    [Fact]
    public async Task Crawl_WhenGivenStartingPageWithLinks_ShouldCrawlPageAndEnqueueChildLinks()
    {
        // arrange
        const string expectedParent = "https://parent.com";
        const string expectedChild= "https://parent.com/child";
        
        MockSuccessCalls(expectedParent, expectedChild);

        var crawlerEngine =
            new CrawlerEngine(MockQueueManager.Object, MockLinkService.Object, MockLinkRepository.Object,
                MockLoggerFactory.Object, new Uri(expectedParent));
        
        // act
        await crawlerEngine.Crawl(CancellationToken.None);

        // assert
        VerifyCrawl(expectedParent, expectedChild);
    }
    
    [Fact]
    public async Task Crawl_WhenAlreadyVisitedLink_ShouldNotEnqueueLinkForCrawling()
    {
        // arrange
        const string expectedParent = "https://parent.com";
        const string expectedChild= "https://parent.com/child";
        
        MockAlreadyVisitedCalls(expectedParent, expectedChild);

        var crawlerEngine =
            new CrawlerEngine(MockQueueManager.Object, MockLinkService.Object, MockLinkRepository.Object,
                MockLoggerFactory.Object, new Uri(expectedParent));
        
        // act
        await crawlerEngine.Crawl(CancellationToken.None);

        // assert
        VerifyAlreadyVisitedCrawl(expectedParent, expectedChild);
    }
}