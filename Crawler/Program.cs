using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Services.Abstractions;
using static System.Console;

namespace Crawler;

public static class Program
{
    private static readonly Stopwatch Timer = new();
    
    private static async Task Main(string[] args)
    { 
        WriteLine($"MAIN: thread ID {Environment.CurrentManagedThreadId}");

        var source = new CancellationTokenSource();
        var token = source.Token;
        var host = StartUp.CreateHostBuilder(args).Build();
        
        Timer.Start();
        await host.Services.GetRequiredService<IEngine>().Crawl(token);
        Timer.Stop();
        
        var crawlResult = host.Services.GetRequiredService<ILinkRepository>().GetResult();
        
        WriteLine("Results: {0}", crawlResult.Pages);
        WriteLine("{0} page(s) found:", crawlResult.PageCount);
        WriteLine("{0} link(s) discarded:", crawlResult.LinksDiscarded );
        WriteLine("Time Elapsed: {0}", Timer.Elapsed.ToString());
    }
}

