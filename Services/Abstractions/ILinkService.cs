using Models;

namespace Services.Abstractions
{
    public interface ILinkService
    {
        Task<PageResult> FindChildLinksAsync(Uri uri);
    }
}

