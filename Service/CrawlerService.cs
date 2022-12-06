using System;
using HtmlAgilityPack;
using System.Net.Http;
using Models;
using Service.Abstractions;

namespace Service
{
	public class CrawlerService: ICrawlerService
	{
        private ICrawlerClient _crawlerClient;
        private const string HrefTagName = "href";
        private const char NewLine = '\n';

        private static string HrefXpath => $"//a[@{HrefTagName}]";

        public CrawlerService(ICrawlerClient crawlerClient)
		{
            _crawlerClient = crawlerClient;
        }

        // Uri repre
        public async Task<IReadOnlyList<string>> FindLinks(Uri uri)
        {
            var pageContent = await _crawlerClient.GetPageContentAsync(uri);

            if (string.IsNullOrWhiteSpace(pageContent))
            {
                return new List<string>();
            }

            var document = new HtmlDocument();
            document.LoadHtml(pageContent);
            var linkNodes = document.DocumentNode.SelectNodes(HrefXpath);

            if (linkNodes is null)
            {
                return new List<string>();
            }

            var links = linkNodes.Where(n => n.Attributes.Contains(HrefTagName)).Select(n => n.Attributes[HrefTagName]).ToList();

            //var linnks = links.Select(l => new Link()).Distinct().ToList();

            return links.Select(l => l.Value.Trim().Trim(NewLine)).Distinct().ToList();
        }
    }
}

