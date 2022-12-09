namespace Models;

public class PageResult
{
    public int TotalLinksFound { get; }
    public IReadOnlyList<Link> VisitableLinks { get; }

    public PageResult(int totalLinksFound, IReadOnlyList<Link> visitableLinks)
    {
        TotalLinksFound = totalLinksFound;
        VisitableLinks = visitableLinks;
    }
}