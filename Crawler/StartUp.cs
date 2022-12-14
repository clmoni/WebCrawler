using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Models;
using Services;
using Services.Abstractions;

namespace Crawler;

public static class StartUp
{
    private const string DefaultStartingUri = "https://fast.com/";
    private const int DefaultDictionaryCapacity = 5000;
    private const int DefaultHttpTimeout = 2000;
    private const int QueueWaitTimeMilliseconds = 1000;

    /*
     * Dictionary this[key], Add(key, value), Remove(key) & Contains(key) are all constant time [O(1)].
     * This is because the underlying implementation is a HashMap.
     * However, the worst case scenario for Add(key, value) is O(n) where the dictionary needs to be
     * resized due to the capacity being too small. For large websites we can specify this capacity from
     * the console args. The ContainsValue(value) method is O(n) - linear time but we are not searching for values here.
     */
    
    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        var startingUri =  GetStartingUri(args);
        var httpTimeout = TimeSpan.FromMilliseconds(DefaultHttpTimeout);
        var queue = new BlockingCollection<Link>(new ConcurrentQueue<Link>());
        var queueManager = new QueueManager(queue, QueueWaitTimeMilliseconds);
        var repositoryList = new Dictionary<string, IList<string?>>(DefaultDictionaryCapacity);
        
        return Host.CreateDefaultBuilder(args)
            .ConfigureServices(services =>
            {
                services.AddSingleton<IQueueManager, QueueManager>(_ => queueManager)
                    .AddSingleton<ILinkRepository, LinkRepository>(p =>
                        new LinkRepository(repositoryList, p.GetRequiredService<ILoggerFactory>()))
                    .AddScoped<ILinkService, LinkService>()
                    .AddScoped<IEngine, CrawlerEngine>(p =>
                        new CrawlerEngine(
                            p.GetRequiredService<IQueueManager>(),
                            p.GetRequiredService<ILinkService>(),
                            p.GetRequiredService<ILinkRepository>(),
                            p.GetRequiredService<ILoggerFactory>(),
                            startingUri
                        ))
                    .AddHttpClient<ILinkClient, LinkClient>(c => c.Timeout = httpTimeout);
            });
    }

    private static Uri GetStartingUri(string[] args)
    {
        var startingUri = args.FirstOrDefault() is null ? DefaultStartingUri : args.First();
        try
        {
            return new Uri(startingUri);
        }
        catch (Exception ex)
        {
            throw new Exception("Invalid starting uri", ex);
        }
    }
}