using System.Text.Json;
using Microsoft.Extensions.Logging;
using Models;
using Services.Abstractions;

namespace Services;

/*
 * not all methods here are tests as most of them are just wrapping
 * the methods on the out of the box Dictionary. However, a lock (_linkRepositoryLock) has been*
 * introduced to synchronise access to the Dictionary as it will be shared by multiple threads.
 * This is to avoid race conditions. 
 */
public class LinkRepository: ILinkRepository
{
    private readonly Dictionary<string, List<string?>> _visitableLinks;
    private readonly object _linkRepositoryLock;
    private const char ForwardSlash = '/';
    private readonly ILogger<LinkRepository> _logger;
    private int _totalLinksFound;

    public LinkRepository(Dictionary<string, List<string?>> visitableLinks, ILoggerFactory loggerFactory)
    {
        _visitableLinks = visitableLinks;
        _linkRepositoryLock = new object();
        _logger = loggerFactory.CreateLogger<LinkRepository>();
        _totalLinksFound = 0;
    }

    private readonly Func<Uri, string> _convertUriToReliableKey = 
        uri => uri.ToString().Trim().TrimEnd(ForwardSlash).ToUpper();
    
    private readonly Func<Uri, string> _convertUriToValueEntry = 
        uri => uri.ToString().Trim().TrimEnd(ForwardSlash);
    
    public void AddVisited(Uri uri)
    {
        lock (_linkRepositoryLock)
        {
            var key = _convertUriToReliableKey(uri);
            if (!_visitableLinks.TryAdd(key, new List<string?>()))
            {
                _logger.LogDebug("Uri {Uri} is already added as key", uri.ToString());
            }
        }
    }
    
    public void AddChildOfVisited(Uri parentUri, Uri childUri)
    {
        lock (_linkRepositoryLock)
        {
            var key = _convertUriToReliableKey(parentUri);
            var childUriStr = _convertUriToValueEntry(childUri);
            
            if (_visitableLinks.ContainsKey(key))
            {
                _visitableLinks[key].Add(childUriStr);
            }
            else
            {
                _logger.LogDebug("Parent {ParentUri} not found, skipping", parentUri.ToString());
            }
        }
    }

    public bool IsAlreadyVisited(Uri uri)
    {
        lock (_linkRepositoryLock)
        {
            var key = _convertUriToReliableKey(uri);
            return _visitableLinks.ContainsKey(key);
        }
    }

    public void IncrementTotalFound(int linkCount)
    {
        lock (_linkRepositoryLock)
        {
            _totalLinksFound += linkCount;
        }
    }

    public CrawlResult GetResult()
    {
        lock (_linkRepositoryLock)
        {
            var results = JsonSerializer.Serialize(_visitableLinks, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            var discardedLinks = _totalLinksFound - _visitableLinks.Count;
            return new CrawlResult(results, _visitableLinks.Count, discardedLinks);
        }
    }
}