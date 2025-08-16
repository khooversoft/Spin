//using System.Collections.Concurrent;
//using System.Diagnostics;
//using Microsoft.Extensions.Logging;
//using Toolbox.Types;

//namespace Toolbox.Tools;

//public interface IMessageBroker
//{
//    IMessageBroker AddRoute<TSend, TReturn>(string path, Func<TSend, ScopeContext, Task<TReturn>> forwardTo, ScopeContext context) where TReturn : notnull;
//    StatusCode RemoveRoute(string path, ScopeContext context);
//    Task<TReturn> Call<TSend, TReturn>(string path, TSend message, ScopeContext context);
//}


//public class MessageBrokerEmulator : IMessageBroker
//{
//    private readonly ILogger<MessageBrokerEmulator> _logger;
//    private ConcurrentDictionary<string, Register> _routes = new(StringComparer.OrdinalIgnoreCase);

//    public MessageBrokerEmulator(ILogger<MessageBrokerEmulator> logger)
//    {
//        _logger = logger.NotNull();
//    }

//    [DebuggerStepThrough]
//    public IMessageBroker AddRoute<TSend, TReturn>(
//        string path,
//        Func<TSend, ScopeContext, Task<TReturn>> forwardTo,
//        ScopeContext context
//        ) where TReturn : notnull
//    {
//        path.NotEmpty();
//        _routes[path] = new Register
//        {
//            Path = path,
//            SendType = typeof(TSend),
//            ReturnType = typeof(TReturn),
//            ForwardTo = async (x, c) => await forwardTo((TSend)x, c),
//        };

//        context.Location().LogInformation("Register route, path={path}", path);

//        return this;
//    }

//    [DebuggerStepThrough]
//    public StatusCode RemoveRoute(string path, ScopeContext context)
//    {
//        path.NotEmpty();

//        context.Location().LogInformation("Removing route, path={path}", path);

//        return _routes.TryRemove(path, out _) switch
//        {
//            true => StatusCode.OK,
//            false => StatusCode.NotFound,
//        };
//    }



//    [DebuggerStepThrough]
//    public async Task<TReturn> Call<TSend, TReturn>(string path, TSend message, ScopeContext context)
//    {
//        path.NotEmpty();
//        message.NotNull();

//        if (!_routes.TryGetValue(path, out Register? route)) throw new ArgumentException($"Path={path} is not registered");

//        context.Location().LogInformation("Sending to path={path}, message={message}", path, message.ToJsonPascal());

//        return (TReturn)await route.ForwardTo(message, context with { CancellationToken = default });
//    }

//    private record Register
//    {
//        public required string Path { get; init; } = null!;
//        public required Type SendType { get; init; } = null!;
//        public required Type ReturnType { get; init; } = null!;
//        public required Func<object, ScopeContext, Task<object>> ForwardTo { get; init; } = null!;
//    }
//}
