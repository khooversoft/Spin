using System.Collections.Concurrent;

namespace SpinPortal.Application;

public class ServerOprationalState
{
    private ConcurrentQueue<string> _log = new ConcurrentQueue<string>();

    public IReadOnlyList<string> Log => _log.ToArray();

    public void Add(string line) => _log.Enqueue(line);
}
