using Microsoft.Extensions.Logging;
using Services.Abstractions;

namespace Services;

public class CrawlerEngine: EngineBase
{
    private readonly ILogger<CrawlerEngine> _logger;

    public CrawlerEngine(
        IQueueManager queueManager,
        ILinkService linkService,
        ILinkRepository linkRepository,
        ILoggerFactory loggerFactory,
        Uri topLevelUri) :
        base(queueManager, linkService, linkRepository, loggerFactory, topLevelUri)
    {
        _logger = loggerFactory.CreateLogger<CrawlerEngine>();
    }

    public override async Task Crawl(CancellationToken cancellationToken)
    {
        // dispose queue upon completion of crawling
        using (QueueManager)
        {
            _logger.LogInformation("Crawling has started");
            
            var pageResult = await LinkService.FindChildLinksAsync(TopLevelUri);
            var topLevelLinks = pageResult.VisitableLinks;
            LinkRepository.IncrementTotalFound(pageResult.TotalLinksFound);
            _logger.LogInformation("Found {topLevelLinks} top level pages in {uri}", topLevelLinks.Count, TopLevelUri);
            await EnqueueTopLevelLinks(topLevelLinks);
            
            // fire off an asynchronous task per child link found on first page.
            // we largely dont need to worry about a max because of the way these Tasks
            // are assigned to available threads on the thread pool.
            var tasks = topLevelLinks.Select(_ => Task.Run(CrawlChildren, cancellationToken));
            
            // wait for all the above task to finish crawling
            await Task.WhenAll(tasks);
            
            _logger.LogInformation("Crawling is complete");
        }
    }
}