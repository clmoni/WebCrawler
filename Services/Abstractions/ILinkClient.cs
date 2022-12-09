namespace Services.Abstractions
{
    public interface ILinkClient
    {
        Task<string> GetLinkContentAsync(Uri uri);
    }
}

