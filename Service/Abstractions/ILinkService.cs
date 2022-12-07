using Models;

namespace Service.Abstractions
{
    public interface ILinkService
    {
        Task<IReadOnlyList<Link>> FindChildLinksAsync(Uri uri);
    }
}

