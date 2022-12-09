using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Service.Abstractions;

namespace Crawler;

public static class Program
{
    private static readonly Stopwatch _timer = new();
    
    private static async Task Main(string[] args)
    {
        var source = new CancellationTokenSource();
        var token = source.Token;
        var host = StartUp.CreateHostBuilder(args).Build();
        
        _timer.Start();
        await host.Services.GetRequiredService<IEngine>().Crawl(token);
        _timer.Stop();
        
        var crawlResult = host.Services.GetRequiredService<ILinkRepository>().GetResult();
        
        Console.WriteLine("Results: {0}", crawlResult.Pages);
        Console.WriteLine("{0} page(s) found:", crawlResult.PageCount);
        Console.WriteLine("{0} link(s) discarded:", crawlResult.LinksDiscarded );
        Console.WriteLine("Time Elapsed: {0}", _timer.Elapsed.ToString());
    }
}

