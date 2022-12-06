using System;
using Models;

namespace Service.Abstractions
{
    public interface ICrawlerService
    {
        Task<IReadOnlyList<string>> FindLinks(Uri uri);
    }
}

