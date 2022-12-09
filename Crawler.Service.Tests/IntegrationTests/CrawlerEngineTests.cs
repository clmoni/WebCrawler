namespace Crawler.Service.Tests.IntegrationTests;
/*
 * Integration testing would require not mocking any of the classes. However,
 * for the test to be deterministic & quick it shouldn't hit a real website.
 * It should hit something running locally. If this was code I was taking to production I would
 * create a tiny react app within this repository & use TestContainer to spin it up for the tests to hit.
 * https://dotnet.testcontainers.org
 */
public class CrawlerEngineTests
{
    
}