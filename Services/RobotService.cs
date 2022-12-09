using Models;
using Services.Abstractions;

namespace Services;

public class RobotService: IRobotService
{
    /// <summary>
    /// will be used to determine pages to crawl or not as per the rules of the site
    /// </summary>
    /// <param name="robotsLink"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public Task<IReadOnlyList<string>> GetDisallowedPaths(RobotsLink robotsLink)
    {
        throw new NotImplementedException();
    }
}