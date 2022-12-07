using Microsoft.Extensions.Logging;
using Service.Abstractions;

namespace Service;
public class LinkClient : ILinkClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<LinkClient> _logger;
    private const string HtmlContentMediaType = "text/html";

    public LinkClient(HttpClient httpClient, ILoggerFactory loggerFactory)
    {
        _httpClient = httpClient;
        _logger = loggerFactory.CreateLogger<LinkClient>();

    }
    
    // add polly for retrying here.
    // for now I've made this method return 
    // an empty string when failure occurs.
    // to be handled by caller.
    public async Task<string> GetLinkContentAsync(Uri uri)
    {
        try
        {
            var response = await _httpClient.GetAsync(uri);
            response.EnsureSuccessStatusCode();
            using var content = response.Content;
            var contentType = content.Headers.ContentType;

            if (!HtmlContentMediaType.Equals(contentType?.MediaType))
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

