using Models;

namespace Service.Abstractions
{
    public interface ILinkService
    {
        Task<PageResult> FindChildLinksAsync(Uri uri);
    }
}

