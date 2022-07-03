using System;
using System.Collections.Generic;
using Toolbox.Tools;

namespace Toolbox.Work;

public class WorkHostBuilder
{
    private readonly Dictionary<string, Type> _orchestration = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, Type> _activities = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
    private readonly IServiceProvider _service;

    public WorkHostBuilder(IServiceProvider service)
    {
        _service = service.NotNull(); ;
    }

    public WorkHostBuilder AddOrchestration<T>() where T : Orchestration => AddOrchestration(typeof(T).Name, typeof(T));
    public WorkHostBuilder AddOrchestration<T>(string name) where T : Orchestration => AddOrchestration(name, typeof(T));

    public WorkHostBuilder AddOrchestration(string name, Type type)
    {
        name
            .NotEmpty()
            .Assert(x => !_orchestration.ContainsKey(x), $"Duplicate key {name}");
        type
            .NotNull()
            .Assert(x => typeof(Orchestration).IsAssignableFrom(x), $"Not assignable to {typeof(Orchestration).FullName}");

        _orchestration[name] = type;
        return this;
    }

    public WorkHostBuilder AddActivity<T>() where T : Activity => AddActivity(typeof(T).Name, typeof(T));
    public WorkHostBuilder AddActivity<T>(string name) where T : Activity => AddActivity(name, typeof(T));

    public WorkHostBuilder AddActivity(string name, Type type)
    {
        name
            .NotEmpty()
            .Assert(x => !_orchestration.ContainsKey(x), $"Duplicate key {name}");
        type
            .NotNull()
            .Assert(x => typeof(Activity).IsAssignableFrom(x), $"Not assignable to {typeof(Activity).FullName}");

        _activities[name] = type;
        return this;
    }

    public WorkHost Build() => new WorkHost(_service, _orchestration, _activities);
}
