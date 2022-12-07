using Microsoft.Extensions.DependencyInjection;
using Service.Abstractions;

namespace Crawler;

public static class Program
{
    private static async Task Main(string[] args)
    {
        var host = StartUp.CreateHostBuilder(args).Build();
        await host.Services.GetRequiredService<ICrawlerEngine>().Crawl();
    }
}

