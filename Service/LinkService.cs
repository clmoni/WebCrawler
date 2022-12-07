using HtmlAgilityPack;
using Models;
using Service.Abstractions;

namespace Service
{
	public class LinkService: ILinkService
	{
        private readonly ILinkClient _linkClient;
        private const string HrefTagName = "href";
        private static readonly string[] AllowedExtension = {".html", ".com", ".co.uk" };
        private const char NewLine = '\n';
        private const char ForwardSlash = '/';
        private static readonly string[] NonVisitableLinkTypes = { "#", "mailto:", "tel:", "sms:" };

        private static string HrefXpath => $"//a[@{HrefTagName}]";

        public LinkService(ILinkClient linkClient)
		{
            _linkClient = linkClient;
        }

        public async Task<IReadOnlyList<Link>> FindChildLinksAsync(Uri uri)
        {
            var pageContent = await _linkClient.GetLinkContentAsync(uri);

            var linkNodes = GetAllLinkInPage(pageContent);

            var linkAttributes = linkNodes.Where(n => n.Attributes.Contains(HrefTagName)).Select(n => n.Attributes[HrefTagName]).ToList();

            var linkAttributesWithinHost = Sanitise(linkAttributes, uri);

            return CreateLinks(linkAttributesWithinHost, uri);
        }

        private static HtmlNodeCollection GetAllLinkInPage(string pageContent)
        {
            var document = new HtmlDocument();
            document.LoadHtml(pageContent);
            return document.DocumentNode.SelectNodes(HrefXpath) ?? 
                   new HtmlNodeCollection(HtmlNode.CreateNode(string.Empty));
        }

        private static IEnumerable<HtmlAttribute> Sanitise(List<HtmlAttribute> linkAttributes, Uri parentUri)
        {
           linkAttributes.RemoveAll(l => IsDisallowedExtension(l.Value));
           linkAttributes.RemoveAll(l => NonVisitableLinkTypes.Any(l.Value.StartsWith));
           linkAttributes.RemoveAll(l => l.Value.Equals(ForwardSlash.ToString()));

           return linkAttributes
               .Where(l =>
                   l.Value.Contains(parentUri.Host) ||
                   l.Value.StartsWith(ForwardSlash)
               );
        }

        private static IReadOnlyList<Link> CreateLinks(IEnumerable<HtmlAttribute> linkAttributes, Uri parentUri)
        {
            return linkAttributes
                .Select(l =>
                    new Link(l.Value.Trim().Trim(NewLine), parentUri.ToString()))
                .DistinctBy(l => l.RawChildLink)
                .DistinctBy(l => l.Uri?.ToString())
                .ToList();
        }

        private static bool IsDisallowedExtension(string link)
        {
            return Path.HasExtension(link) && !AllowedExtension.Any(link.EndsWith);
        }
    }
}

