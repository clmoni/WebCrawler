using Microsoft.Extensions.Logging;
using Models;
using Service.Abstractions;

namespace Service;

public abstract class EngineBase: IEngine
{
    protected readonly ILinkRepository LinkRepository;
    protected readonly IQueueManager QueueManager;
    protected readonly ILinkService LinkService;
    protected readonly ILogger<IEngine> Logger;

    protected readonly Uri TopLevelUri;

    protected EngineBase(IQueueManager queueManager, ILinkService linkService, ILinkRepository linkRepository, ILoggerFactory logger, Uri topLevelUri)
    {
        QueueManager = queueManager;
        LinkService = linkService;
        LinkRepository = linkRepository;
        TopLevelUri = topLevelUri;
        Logger = logger.CreateLogger<IEngine>();
    }
    
    protected async Task EnqueueTopLevelLinks(IEnumerable<Link> topLevelLinks)
    {
        foreach (var link in topLevelLinks)
        {
            if (!link.IsCrawlableLink(TopLevelUri)) continue;
            await Task.Run(() => LinkRepository.AddVisited(TopLevelUri));
            await Task.Run(() => Enqueue(link));
        }
    }
    
    protected async Task CrawlChildren()
    {
        while (QueueManager.Dequeue(out var link))
        {
            await HandleDequeuedLink(TopLevelUri, link);
        }
    }

    private async Task HandleDequeuedLink(Uri topLevelUri, Link? link)
    {
        if (link?.Uri is null) return;
        await Task.Run(() => LinkRepository.AddVisited(link.Uri));
        
        var pageResult = await LinkService.FindChildLinksAsync(link.Uri);
        var childLinks = pageResult.VisitableLinks;
        LinkRepository.IncrementTotalFound(pageResult.TotalLinksFound);
        Logger.LogInformation("Found {childLinks} child pages in {uri}", childLinks.Count, link.Uri);

        foreach (var childLink in childLinks)
        {
            if (!childLink.IsCrawlableLink(topLevelUri)) continue;
            await Enqueue(childLink);
        }
    }
    
    private async Task Enqueue(Link link)
    {
        if (link.Uri == null) return;
        
        await Task.Run(() => LinkRepository.AddChildOfVisited(link.ParentLink, link.Uri));
        if (!await Task.Run(() => LinkRepository.IsAlreadyVisited(link.Uri)))
        {
            await Task.Run(() => QueueManager.Enqueue(link));
        }
    }

    public abstract Task Crawl(CancellationToken cancellationToken);
}