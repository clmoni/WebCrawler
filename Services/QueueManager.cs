using System.Collections.Concurrent;
using Models;
using Services.Abstractions;

namespace Services;

/*
 * using out of the box thread safe BlockingCollection due to its
 * use of a ConcurrentQueue.
 * class not unit tested because its just wrapping the out of the box queue
 * with no extra logic on top, not much value in testing out of the box code.
*/
public class QueueManager: IQueueManager
{
    private readonly BlockingCollection<Link> _linksQueue;
    private bool _disposed;
    private readonly int _waitTimeInMilliseconds;
    
    public QueueManager(BlockingCollection<Link> linksQueue, int waitTimeInMilliseconds)
    {
        _linksQueue = linksQueue;
        _waitTimeInMilliseconds = waitTimeInMilliseconds;
    }

    public void Enqueue(Link link)
    {
        _linksQueue.Add(link);
    }

    public bool Dequeue(out Link? link)
    {
        var timeout = TimeSpan.FromMilliseconds(_waitTimeInMilliseconds);
        return _linksQueue.TryTake(out link, timeout);
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _linksQueue.Dispose();
            }
        }
            
        _disposed = true;
    }
}