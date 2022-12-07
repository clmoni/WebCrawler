using Microsoft.Extensions.Logging;
using Service.Abstractions;

namespace Service;

public class CrawlerEngine: EngineBase, ICrawlerEngine
{
    private readonly ILogger<CrawlerEngine> _logger;

    public CrawlerEngine(
        IQueueManager queueManager,
        ILinkService linkService,
        ILinkRepository linkRepository,
        ILogger<CrawlerEngine> logger,
        Uri topLevelUri) :
        base(queueManager, linkService, linkRepository, topLevelUri)
    {
        _logger = logger;
    }

    public async Task Crawl()
    {
        _logger.LogInformation("Crawling has started");

        // dispose queue upon completion of crawling
        using (QueueManager)
        {
            var topLevelLinks = await LinkService.FindChildLinksAsync(TopLevelUri);
            EnqueueTopLevelLinks(topLevelLinks);

            foreach (var _ in topLevelLinks)
            {
                await Task.Factory.StartNew(async () => { await CrawlChildren(TopLevelUri); });

                Thread.Sleep(1000);
            }
        }

        _logger.LogInformation("Crawling is complete");
    }
}