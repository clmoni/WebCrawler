namespace Models;

public class Link
{
    private const string _insecureHttpProtocol = "http:";
    private const string _secureHttpProtocol = "https:";
    private const char _forwardSlash = '/';


    public string OriginalLink { get; }
    public string ParentLink { get; }
    public bool IsOriginalLinkFullQualified { get; }
    public Uri? FullyQualifiedUri { get; }
    public bool IsLinkToBeVisited { get; }

    // Domain is in parent link
    public Link(string originalLink, string parentLink)
    {
        OriginalLink = originalLink;
        ParentLink = NormaliseLink(parentLink);
        IsOriginalLinkFullQualified = IsLinkFullQualified(originalLink);
        FullyQualifiedUri = IsOriginalLinkFullQualified ? new Uri(OriginalLink) : default;
        IsLinkToBeVisited = IsOriginalLinkFullQualified;
    }

    private bool IsLinkFullQualified(string originalLink)
    {
        return originalLink.ToLower().StartsWith(_insecureHttpProtocol) || originalLink.ToLower().StartsWith(_secureHttpProtocol);
    }

    public string NormaliseLink(string link)
    {
        var uri = new Uri(link);

        var builder = new UriBuilder();
        builder.Scheme = uri.Scheme;
        builder.Port = uri.Port;
        builder.Host = uri.Host;
        builder.Path = uri.AbsolutePath;

        return builder.Uri.AbsoluteUri
            .Remove(builder.Uri.AbsoluteUri.Length - builder.Uri.Segments.Last().Length)
            .TrimEnd(_forwardSlash);
    }
}

