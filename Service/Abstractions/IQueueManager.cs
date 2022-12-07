using Models;

namespace Service.Abstractions;

public interface IQueueManager: IDisposable
{
    void Enqueue(Link link);
    bool Dequeue(out Link? link);
}