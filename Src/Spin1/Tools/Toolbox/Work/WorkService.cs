using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Monads;
using Toolbox.Tools;

namespace Toolbox.Work;

public class WorkService
{
    private readonly ILogger<WorkService> _logger;
    private readonly ConcurrentDictionary<string, Func<Request, Task<Response>>> _activities = new(StringComparer.OrdinalIgnoreCase);
    public WorkService(ILogger<WorkService> logger) => _logger = logger.NotNull();

    public WorkService Add(string name, Func<Request, Task<Response>> activity) => this.Action(x => x._activities[name] = activity);

    public async Task<Option<Response>> Run(string name, Context context)
    {
        name.NotEmpty();
        context.NotNull();

        _logger.LogTrace("Running={name}", name);

        var request = new Request { Name = name, Context = context };

        if (!_activities.TryGetValue(name, out var activities)) return Option<Response>.None;
        return await activities(request);
    }
}

public record Request
{
    public string Name { get; init; } = null!;
    public Context Context { get; init; } = null!;
}

public record Response
{
    public string NextName { get; init; } = null!;
    public Context Context { get; init; } = null!;
}
