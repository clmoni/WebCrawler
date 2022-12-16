using Microsoft.Extensions.Logging;
using Services.Abstractions;

namespace Services;

/*
 * HttpClient is intended to be instantiated once & reused throughout the life of the application.
 * Creating a new HttpClient instance per request ourselves will exhaust the available sockets result
 * in a SocketException being thrown. This is important for an application such as this that can potentially
 * fire off thousands of requests.
 *
 * hence this class does not dispose or instantiate the HttpClient & is registered at start up by using AddHttpClient().
 * This basically ensures this class is short lived but every time a new one is needed,
 * it gets a new HttpClient with a recycled HttpMessageHandler & connection from the HttpClientFactory managed by the framework.
 * essentially, after every request the HttpMessageHandler is released & recycled (not disposed) by other HttpClients.
 */
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
    
    /*
     * add polly for retrying here.
     * for now I've made this method return
     * an empty string when failure occurs.
     * to be handled by caller.
     */
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
            _logger.LogDebug("{Uri} failed with {Message}", uri.AbsolutePath, ex.Message);
            return string.Empty;
        }
    }
}

