using Models;

namespace Service.Abstractions;

public interface ILinkRepository
{
    void AddVisited(Uri uri);
    void AddChildOfVisited(Uri parentUri, Uri childUri);
    bool IsAlreadyVisited(Uri uri);
    void IncrementTotalFound(int linkCount);
    CrawlResult GetResult();
}