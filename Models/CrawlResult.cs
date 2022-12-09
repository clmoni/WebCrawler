namespace Models;

public class CrawlResult
{
    public string Pages { get; }
    public int PageCount { get; }
    public int LinksDiscarded { get; }

    public CrawlResult(string pages, int pageCount, int linksDiscarded)
    {
        Pages = pages;
        PageCount = pageCount;
        LinksDiscarded = linksDiscarded;
    }
}