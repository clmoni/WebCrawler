using Service.Abstractions;

namespace Service;

public class LinkRepository: ILinkRepository
{
    private readonly Dictionary<string, List<string?>> _linkRepository;
    private readonly object _visitedLinksLock;
    private const char ForwardSlash = '/';

    public LinkRepository(Dictionary<string, List<string?>> linkRepository)
    {
        _linkRepository = linkRepository;
        _visitedLinksLock = new object();
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
}