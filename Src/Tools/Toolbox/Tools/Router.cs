using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Toolbox.Extensions;
using Toolbox.Monads;

namespace Toolbox.Tools;


public interface IRouter<T, TResult>
{
    int Count { get; }
    IRouter<T, TResult> Add(string path, Func<T, CancellationToken, TResult> receiver);
    IRouter<T, TResult> Remove(string path);
    TResult Send(string path, T message, CancellationToken token);
}


public class Router<T, TResult> : IRouter<T, TResult>, IEnumerable<Router<T, TResult>.Route>
{
    private readonly ConcurrentDictionary<string, Route> _routes = new(StringComparer.OrdinalIgnoreCase);
    private readonly ILogger<Router<T, TResult>> _logger;

    public Router(ILogger<Router<T, TResult>> logger) => _logger = logger.NotNull();

    public int Count => _routes.Count;

    public IRouter<T, TResult> Add(string path, Func<T, CancellationToken, TResult> receiver)
    {
        _routes.TryAdd(path, new Route(path, receiver))
            .Assert(x => x == true, $"Path={path} already exist", _logger);

        _logger.LogTrace("Adding route, path={path}", path);
        return this;
    }

    public IRouter<T, TResult> Remove(string path) => this.Action(_ => _routes.Remove(path.NotEmpty(), out var _));

    public TResult Send(string path, T message, CancellationToken token)
    {
        path.NotEmpty();

        _logger.LogTrace("Sending to path={path}", path);

        var route = path.Seq<string, Route>()
            .Bind(x => _routes.TryGetValue(x))
            .Bind(x => _routes.TryGetValue("*"))
            .Return()
            .First(x => x.HasValue)
            .Return();

        return route!.Receiver(message, token);
    }

    public IEnumerator<Route> GetEnumerator() => _routes.Values.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => _routes.Values.GetEnumerator();
    public record Route(string Path, Func<T, CancellationToken, TResult> Receiver);
}

