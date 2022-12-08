using System.Text.Json;
using Microsoft.Extensions.Logging;
using Service.Abstractions;

namespace Service;

public class LinkRepository: ILinkRepository
{
    private readonly Dictionary<string, List<string?>> _linkRepository;
    private readonly object _visitedLinksLock;
    private const char ForwardSlash = '/';
    private readonly ILogger<LinkRepository> _logger;

    public LinkRepository(Dictionary<string, List<string?>> linkRepository, ILoggerFactory loggerFactory)
    {
        _linkRepository = linkRepository;
        _visitedLinksLock = new object();
        _logger = loggerFactory.CreateLogger<LinkRepository>();
    }

    private readonly Func<Uri, string> _convertUriToReliableKey = 
        uri => uri.ToString().Trim().TrimEnd(ForwardSlash).ToUpper();
    
    private readonly Func<Uri, string> _convertUriToValueEntry = 
        uri => uri.ToString().Trim().TrimEnd(ForwardSlash);
    
    public void AddVisited(Uri uri)
    {
        lock (_visitedLinksLock)
        {
            var key = _convertUriToReliableKey(uri);
            _linkRepository.TryAdd(key, new List<string?>());
        }
    }
    
    public void AddChildOfVisited(Uri parentUri, Uri childUri)
    {
        lock (_visitedLinksLock)
        {
            var key = _convertUriToReliableKey(parentUri);
            var childUriStr = _convertUriToValueEntry(childUri);
            if (_linkRepository.ContainsKey(key))
            {
                _linkRepository[key].Add(childUriStr);
            }
        }
    }

    public bool IsAlreadyVisited(Uri uri)
    {
        lock (_visitedLinksLock)
        {
            var key = _convertUriToReliableKey(uri);
            return _linkRepository.ContainsKey(key);
        }
    }

    public void PrintAll()
    {
        lock (_visitedLinksLock)
        {
            var results = JsonSerializer.Serialize(_linkRepository, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            
            _logger.LogInformation("Results: {Results} \n\n Visited: {count} page(s)", results, _linkRepository.Count);
        }
    }
}