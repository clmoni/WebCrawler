using Models;

namespace Services.Abstractions;

public interface IRobotService
{
    Task<IReadOnlyList<string>> GetDisallowedPaths(RobotsLink robotsLink);
}