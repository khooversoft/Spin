using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Logging;
using Toolbox.Tools;

namespace Toolbox.Work;

public class WorkflowBuilder
{
    private readonly ConcurrentDictionary<string, Type> _list = new(StringComparer.OrdinalIgnoreCase);

    public WorkflowBuilder() { }
    public WorkflowBuilder(IServiceProvider serviceProvider, ILoggerFactory loggerFactory)
    {
        ServiceProvider = serviceProvider;
        LoggerFactory = loggerFactory;
    }

    public IServiceProvider? ServiceProvider { get; set; }
    private ILoggerFactory? LoggerFactory { get; set; }

    public WorkflowBuilder SetServiceProvider(IServiceProvider serviceProvider) => this.Action(x => x.ServiceProvider = serviceProvider);
    public WorkflowBuilder SetLoggerFactory(ILoggerFactory loggerFactory) => this.Action(x => x.LoggerFactory = loggerFactory);
    public WorkflowBuilder Add<T>() where T : WorkflowActivityBase => this.Action(_ => _list[typeof(T).Name] = typeof(T));
    public WorkflowBuilder Add<T>(string name) where T : WorkflowActivityBase => this.Action(_ => _list[name.NotEmpty()] = typeof(T));

    public Workflow Build()
    {
        ServiceProvider.NotNull();
        LoggerFactory.NotNull();
        _list.Count.Assert(x => x > 0, "no activities registered");

        return new Workflow(_list, ServiceProvider, LoggerFactory.CreateLogger<Workflow>());
    }
}

public class Workflow
{
    private readonly IReadOnlyDictionary<string, Type> _activities;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<Workflow> _logger;

    public Workflow(IEnumerable<KeyValuePair<string, Type>> activities, IServiceProvider serviceProvider, ILogger<Workflow> logger)
    {
        _activities = activities
            .NotNull()
            .ToDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase)
            .Assert(x => x.Count > 0, "No activities");

        _serviceProvider = serviceProvider.NotNull();
        _logger = logger.NotNull();
    }

    public async Task<string> Send(string name, string message, CancellationToken token = default)
    {
        name.NotEmpty();
        var ls = _logger.LogEntryExit();

        _activities.TryGetValue(name, out Type? activityType)
            .Assert(x => x == true, x => $"Activity name={x} not registered");

        WorkflowActivityBase activity = (WorkflowActivityBase)_serviceProvider.GetRequiredService(activityType!);

        _logger.LogTrace("Running name={name}, message={message}", name, message);
        var response = await activity.Send(message, this, token);
        _logger.LogTrace("Ran name={name}, message={message}, response={response}", name, message, response);

        return response;
    }
}

public static class WorkflowExtensions
{
    public static Task<T> Send<T>(this Workflow workflow, string name, T request, CancellationToken token = default) where T : class =>
        workflow.Send<T, T>(name, request, token);

    public static async Task<TResponse> Send<TRequest, TResponse>(this Workflow workflow, string name, TRequest request, CancellationToken token = default)
        where TRequest : class
        where TResponse : class
    {
        string message = request switch
        {
            string v => v,
            _ => request.ToJson()
        };

        string reponseMessage = await workflow.Send(name, message, token);

        TResponse response = typeof(TResponse) switch
        {
            Type v when v == typeof(string) => (TResponse)(object)message,
            _ => reponseMessage.ToObject<TResponse>().NotNull(),
        };

        return response;
    }

    public static Task<string> Send<T>(this Workflow workflow, string message, CancellationToken token = default) => workflow
        .NotNull()
        .Send(typeof(T).Name, message, token);

    public static Task<TResponse> Send<T, TRequest, TResponse>(this Workflow workflow, TRequest request, CancellationToken token = default)
        where T : WorkflowActivityBase
        where TRequest : class
        where TResponse : class
        => workflow
        .NotNull()
        .Send<TRequest, TResponse>(typeof(T).Name, request, token);
}

public abstract class WorkflowActivityBase
{
    private string? _name;

    public string Name { get { return _name ?? GetName(); } init { _name = value; } }

    public abstract Task<string> Send(string message, Workflow workflow, CancellationToken token);
    private string GetName() => this.GetType().Name;
}

public abstract class WorkflowActivity<TRequest, TResponse> : WorkflowActivityBase
    where TRequest : class
    where TResponse : class
{
    public override async Task<string> Send(string message, Workflow workflow, CancellationToken token)
    {
        message.NotEmpty();
        workflow.NotNull();

        TRequest request = typeof(TRequest) switch
        {
            Type v when v == typeof(string) => (TRequest)(object)message,
            _ => message.ToObject<TRequest>().NotNull(),
        };

        var response = await Send(request, workflow);

        return response switch
        {
            string v => v,
            _ => response.ToJson()
        };
    }

    protected abstract Task<TResponse> Send(TRequest request, Workflow workflow);
}
