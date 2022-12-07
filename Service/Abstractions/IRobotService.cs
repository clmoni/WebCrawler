using Models;

namespace Service.Abstractions;

public interface IRobotService
{
    Task<IReadOnlyList<string>> GetDisallowedPaths(RobotsLink robotsLink);
}