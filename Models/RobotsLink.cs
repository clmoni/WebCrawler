namespace Models;

public class RobotsLink: Link
{
    private const string RobotsRelativePath = "robots.txt";
    public Uri? RobotsUri { get; }

    public RobotsLink(string parentLink) : base(RobotsRelativePath, parentLink)
    {
        RobotsUri = Uri;
    }
}