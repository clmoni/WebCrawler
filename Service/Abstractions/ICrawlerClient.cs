using System;
using Models;

namespace Service.Abstractions
{
    public interface ICrawlerClient
    {
        Task<string> GetPageContentAsync(Uri uri);
    }
}

