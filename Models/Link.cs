namespace Models;

/*
 * This is sort of like a value object because it is immutable.
 * once initialised you can be ask questions about its internal state.
 * Validation happens in the constructor. Ideally if not a valid link, it should be thrown out
 * but I didnt want to break the flow in the service class & opted for a null filter so the crawl is
 * continuous. Null Links should be logged & inspected later.
 */
public class Link
{
    private const char ForwardSlash = '/';
    private static readonly string[] NonVisitableLinkTypes = { "#", "mailto:", "tel:", "sms:" };
    private static readonly string[] VisitableLinkTypes = { "http:", "https:" };

    public string RawChildLink { get; }
    public Uri? Uri { get; }
    public Uri ParentLink { get; }

    // Domain is in parent link
    public Link(string childLink, string parentLink)
    {
        RawChildLink = childLink;
        Uri = NormaliseChildLink(childLink, parentLink);
        ParentLink = new Uri(parentLink);
    }

    private bool IsChildLinkSameAsParent()
    {
        return IsChildLinkSameAs(ParentLink);
    }
    
    public bool IsCrawlableLink(Uri topLevelUri)
    {
        return Uri is not null &&
               !IsChildLinkSameAsParent() &&
               !IsChildLinkSameAs(topLevelUri) &&
               IsChildLinkInParentDomain();
    }

    private bool IsChildLinkSameAs(Uri uri)
    {
        return Uri is not null && Uri.ToString().ToUpper().TrimEnd(ForwardSlash).Equals(uri.ToString().ToUpper().TrimEnd(ForwardSlash));
    }

    private static bool StartsWithHttpProtocol(string childLink)
    {
        return VisitableLinkTypes.Any(childLink.ToLower().StartsWith);
    }

    private static bool IsValidUri(string link)
    {
        try
        {
            if (!StartsWithHttpProtocol(link))
            {
                throw new Exception("Invalid link");
            }
            var _ = new Uri(link);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private bool IsChildLinkInParentDomain()
    {
        return ParentLink.Host == Uri?.Host;
    }
    
    // strings are immutable in c# so everytime you try to mutate one a new one is allocated on the heap.
    // this can be heavy on the GC & which will in turn pause the app to try to remove them once they're 
    // out of scope depending on their age in GC which will in turn impact performance. Here I am trying not 
    // to do unnecessary direct mutation of strings but could be improved with a Span type which allocates string
    // reference addresses on the stack as well as offsets instead.
    private static Uri? NormaliseChildLink(string childLink, string parentLink)
    {
        if (NonVisitableLinkTypes.Any(childLink.ToLower().StartsWith) ||
            childLink.Trim().TrimEnd(ForwardSlash)
                .Equals(parentLink.Trim().TrimEnd(ForwardSlash)) ||
            childLink.Trim().Equals(ForwardSlash.ToString()))
        {
            return default;
        }

        if (IsValidUri(childLink))
        {
            return new Uri(childLink);
        }
        
        var parentUri = new Uri(parentLink);
        var sections = new List<string>();
        
        /*
         each split stored on the heap
        var l = childLink.AsSpan();
        foreach (var s in l)
        {
            if (s == ForwardSlash)
            {
                sections.Add(l.Slice(l.IndexOf(s) + 1));
            }
        }
        */
        
        sections.AddRange(childLink.Split(ForwardSlash));

        var nonEmptySections = sections.Where(s => !string.IsNullOrWhiteSpace(s)).ToList();

        var relativeUrl = string.Join(ForwardSlash, nonEmptySections);

        var builder = new UriBuilder
        {
            Scheme = parentUri.Scheme,
            Port = parentUri.Port,
            Host = parentUri.Host
        };

        return new Uri(builder.Uri, relativeUrl);
    }
}

