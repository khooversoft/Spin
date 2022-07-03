using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Tools;

namespace Toolbox.Work;

public class WorkHost
{
    private readonly IReadOnlyDictionary<string, Type> _orchestration;
    private readonly IReadOnlyDictionary<string, Type> _activities;
    private readonly IServiceProvider _service;

    public WorkHost(IServiceProvider service, IReadOnlyDictionary<string, Type> orchestration, IReadOnlyDictionary<string, Type> activities)
    {
        _service = service.NotNull();

        _orchestration = orchestration
            .Assert(x => x.Count > 0, "Orchestration list is empty")
            .ToDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase);

        _activities = activities
            .ToDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase);
    }

    public Task<string> Run<T>(string input) where T : Orchestration => Run(typeof(T).Name, input);

    public async Task<string> Run(string name, string input)
    {
        name.NotEmpty();
        input.NotNull();

        _orchestration.TryGetValue(name, out Type? orchestrationType)
            .Assert(x => x == true, $"{name} is not found");

        var context = new OrchestrationContext(this);
        Orchestration orchestration = (Orchestration)_service.GetRequiredService(orchestrationType!);

        return await orchestration!.Run(context, input);
    }

    public Task<TResult> Run<T, TResult, TInput>(TInput input) where T : Orchestration => Run<TResult, TInput>(typeof(T).Name, input);

    public async Task<TResult> Run<TResult, TInput>(string name, TInput input)
    {
        name.NotEmpty();
        input.NotNull();

        _orchestration.TryGetValue(name, out Type? orchestrationType)
            .Assert(x => x == true, $"{name} is not found");

        var context = new OrchestrationContext(this);
        Orchestration orchestrationBase = (Orchestration)_service.GetRequiredService(orchestrationType!);

        Orchestration<TResult, TInput> orchestration = (Orchestration<TResult, TInput>)orchestrationBase;

        return await orchestration.Execute(context, input);
    }

    public async Task<string> RunActivity(OrchestrationContext context, string name, string input)
    {
        context.NotNull();
        name.NotEmpty();
        input.NotNull();

        _activities.TryGetValue(name, out Type? activityType)
            .Assert(x => x == true, $"Activity {name} not found");

        Activity activity = (Activity)_service.GetRequiredService(activityType!);

        return await activity!.Run(context, input);
    }

    public Task<TResult> RunActivity<T, TResult, TInput>(OrchestrationContext context, TInput input) =>
        RunActivity<TResult, TInput>(context, typeof(T).Name, input);

    public async Task<TResult> RunActivity<TResult, TInput>(OrchestrationContext context, string name, TInput input)
    {
        context.NotNull();
        name.NotEmpty();

        _activities.TryGetValue(name, out Type? activityType)
                    .Assert(x => x == true, $"Activity {name} not found");

        Activity activityBase = (Activity)_service.GetRequiredService(activityType!);

        Activity<TResult, TInput> activity = (Activity<TResult, TInput>)activityBase;

        return await activity.Execute(context, input);
    }

}
