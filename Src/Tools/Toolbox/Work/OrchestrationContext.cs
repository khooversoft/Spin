using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Work;

public class OrchestrationContext
{
    private readonly WorkHost _workHost;

    public OrchestrationContext(WorkHost workHost)
    {
        _workHost = workHost.NotNull();
    }

    public ContextProperty Property { get; } = new ContextProperty();


    public Task<string> RunActivity(string name, string input) => _workHost.RunActivity(this, name, input);

    public Task<TResult> RunActivity<T, TResult, TInput>(TInput input) => _workHost.RunActivity<T, TResult, TInput>(this, input);

    public Task<TResult> RunActivity<TResult, TInput>(string name, TInput input) => _workHost.RunActivity<TResult, TInput>(this, name, input);

    public Task<string> RunOrchestration<T>(string input) where T : Orchestration => _workHost.Run<T>(input);

    public Task<string> RunOrchestration(string name, string input) => _workHost.Run(name, input);

    public Task<TResult> RunOrchestration<T, TResult, TInput>(TInput input) where T : Orchestration => _workHost.Run<T, TResult, TInput>(input);

    public Task<TResult> RunOrchestration<TResult, TInput>(string name, TInput input) => _workHost.Run<TResult, TInput>(name, input);
}
