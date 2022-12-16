using Microsoft.Extensions.Logging;
using Services.Abstractions;

namespace Services;

public class CrawlerEngine: EngineBase
{
    public CrawlerEngine(
        IQueueManager queueManager,
        ILinkService linkService,
        ILinkRepository linkRepository,
        ILoggerFactory loggerFactory,
        Uri topLevelUri) :
        base(queueManager, linkService, linkRepository, loggerFactory, topLevelUri)
    { }

    public override async Task Crawl(CancellationToken cancellationToken)
    {
        Logger.LogInformation("Crawl: thread ID {id}", Environment.CurrentManagedThreadId);
        // dispose queue upon completion of crawling
        using (QueueManager)
        {
            Logger.LogInformation("Crawling has started");
            
            var pageResult = await LinkService.FindChildLinksAsync(TopLevelUri);
            var topLevelLinks = pageResult.VisitableLinks;
            LinkRepository.AddVisited(TopLevelUri);
            LinkRepository.IncrementTotalFound(pageResult.TotalLinksFound);
            Logger.LogInformation("Found {topLevelLinks} top level pages in {uri}", topLevelLinks.Count, TopLevelUri);
            EnqueueTopLevelLinks(topLevelLinks);
            
            // fire off an asynchronous task per child link found on first page.
            // we largely dont need to worry about a max because of the way these Tasks
            // are assigned to available threads on the thread pool.
            var tasks = topLevelLinks.Select(_ => Task.Run(CrawlChildren, cancellationToken));
            
            // wait for all the above task to finish crawling
            await Task.WhenAll(tasks);
            
            Logger.LogInformation("Crawling is complete");
        }
    }
}