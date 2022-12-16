using Microsoft.Extensions.Logging;
using Models;
using Services.Abstractions;

namespace Services;

public abstract class EngineBase: IEngine
{
    protected readonly ILinkRepository LinkRepository;
    protected readonly IQueueManager QueueManager;
    protected readonly ILinkService LinkService;
    protected readonly ILogger<EngineBase> Logger;

    protected readonly Uri TopLevelUri;

    protected EngineBase(IQueueManager queueManager, ILinkService linkService, ILinkRepository linkRepository, ILoggerFactory logger, Uri topLevelUri)
    {
        QueueManager = queueManager;
        LinkService = linkService;
        LinkRepository = linkRepository;
        TopLevelUri = topLevelUri;
        Logger = logger.CreateLogger<EngineBase>();
    }
    
    protected void EnqueueTopLevelLinks(IReadOnlyList<Link> topLevelLinks)
    {
        Logger.LogInformation("EnqueueTopLevelLinks: thread ID {id}", Environment.CurrentManagedThreadId);

        foreach (var link in topLevelLinks)
        {
            if (!link.IsCrawlableLink(TopLevelUri)) continue;
            Enqueue(link);
        }
    }
    
    protected async Task CrawlChildren()
    {
        //To show the task executes on a different thread than the main application thread.
        Logger.LogInformation("CrawlChildren: thread ID {id}", Environment.CurrentManagedThreadId);
        while (QueueManager.Dequeue(out var link))
        {
            await HandleDequeuedLink(TopLevelUri, link);
        }
    }

    private async Task HandleDequeuedLink(Uri topLevelUri, Link? link)
    {
        if (link?.Uri is null) return;
        LinkRepository.AddVisited(link.Uri);
        
        var pageResult = await LinkService.FindChildLinksAsync(link.Uri);
        var childLinks = pageResult.VisitableLinks;
        LinkRepository.IncrementTotalFound(pageResult.TotalLinksFound);
        Logger.LogInformation("Found {childLinks} child pages in {uri}", childLinks.Count, link.Uri);

        foreach (var childLink in childLinks)
        {
            if (!childLink.IsCrawlableLink(topLevelUri)) continue;
            Enqueue(childLink);
        }
    }
    
    private void Enqueue(Link link)
    {
        if (link.Uri == null) return;
        
        LinkRepository.AddChildOfVisited(link.ParentLink, link.Uri);
        if (!LinkRepository.IsAlreadyVisited(link.Uri))
        {
            QueueManager.Enqueue(link);
        }
    }

    public abstract Task Crawl(CancellationToken cancellationToken);
}