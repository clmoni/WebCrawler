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
        ILoggerFactory loggerFactory,
        Uri topLevelUri) :
        base(queueManager, linkService, linkRepository, topLevelUri)
    {
        _logger = loggerFactory.CreateLogger<CrawlerEngine>();;
    }

    public async Task Crawl()
    {
        // dispose queue upon completion of crawling
        using (QueueManager)
        {
            _logger.LogInformation("Crawling has started");
            var topLevelLinks = await LinkService.FindChildLinksAsync(TopLevelUri);
            EnqueueTopLevelLinks(topLevelLinks);
            
            var tasks = topLevelLinks.Select(_ => Task.Run(CrawlChildren)).ToList();

            await Task.WhenAll(tasks);
            _logger.LogInformation("Crawling is complete");
        }
        
        LinkRepository.PrintAll();
    }
}