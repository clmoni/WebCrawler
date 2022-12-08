using Models;
using Service.Abstractions;

namespace Service;

public abstract class EngineBase
{
    protected readonly IQueueManager QueueManager;
    protected readonly ILinkRepository LinkRepository;
    protected readonly ILinkService LinkService;
    protected readonly Uri TopLevelUri;

    protected EngineBase(IQueueManager queueManager, ILinkService linkService, ILinkRepository linkRepository, Uri topLevelUri)
    {
        QueueManager = queueManager;
        LinkService = linkService;
        LinkRepository = linkRepository;
        TopLevelUri = topLevelUri;
    }
    
    protected void EnqueueTopLevelLinks(IEnumerable<Link> topLevelLinks)
    {
        foreach (var link in topLevelLinks)
        {
            if (!link.IsCrawlableLink(TopLevelUri)) continue;
            LinkRepository.AddVisited(TopLevelUri);
            Enqueue(link);
        }
    }
    
    protected async Task CrawlChildren()
    {
        while (QueueManager.Dequeue(out var link))
        {
            await HandleLink(TopLevelUri, link);
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
    
    private async Task HandleLink(Uri topLevelUri, Link link)
    {
        if (link.Uri is null) return;
        LinkRepository.AddVisited(link.Uri);
        var childLinks = await LinkService.FindChildLinksAsync(link.Uri);

        foreach (var childLink in childLinks)
        {
            if (!childLink.IsCrawlableLink(topLevelUri)) continue;
            Enqueue(childLink);
        }
    }
}