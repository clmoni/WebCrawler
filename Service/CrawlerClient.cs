using Microsoft.Extensions.Logging;
using Models;
using Service.Abstractions;

namespace Service;
public class CrawlerClient : ICrawlerClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CrawlerClient> _logger;
    private const string HtmlContentMediaType = "text/html";

    public CrawlerClient(HttpClient httpClient, ILoggerFactory loggerFactory)
    {
        _httpClient = httpClient;
        _logger = loggerFactory.CreateLogger<CrawlerClient>();

    }


    // add polly for retrying here
    public async Task<string> GetPageContentAsync(Uri uri)
    {
        try
        {
            var response = await _httpClient.GetAsync(uri);
            response.EnsureSuccessStatusCode();
            using var content = response.Content;
            var contenType = content.Headers.ContentType;

            if (!HtmlContentMediaType.Equals(contenType?.MediaType))
            {
                return string.Empty;
            }

            return await content.ReadAsStringAsync();
        }
        catch (Exception ex)
        {
            _logger.LogInformation("{Uri} failed with {message}", uri.AbsolutePath, ex.Message);
            return string.Empty;
        }
    }
}

