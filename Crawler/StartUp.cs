using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Models;
using Service;
using Service.Abstractions;

namespace Crawler;

public static class StartUp
{
    private const string DefaultStartingUri = "https://monzo.com/";
    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        var startingUri =  GetStartingUri(args);

        return Host.CreateDefaultBuilder(args)
            .ConfigureServices(services =>
            {
                services.AddSingleton<IQueueManager, QueueManager>(_ =>
                        new QueueManager(new BlockingCollection<Link>(new ConcurrentQueue<Link>())))
                    .AddSingleton<ILinkRepository, LinkRepository>(_ =>
                        new LinkRepository(new Dictionary<string, List<string?>>()))
                    .AddScoped<ILinkService, LinkService>()
                    .AddScoped<ICrawlerEngine, CrawlerEngine>(p =>
                        new CrawlerEngine(
                            p.GetRequiredService<IQueueManager>(),
                            p.GetRequiredService<ILinkService>(),
                            p.GetRequiredService<ILinkRepository>(),
                            p.GetRequiredService<ILogger<CrawlerEngine>>(),
                            startingUri
                        ))
                    .AddHttpClient<ILinkClient, LinkClient>();
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