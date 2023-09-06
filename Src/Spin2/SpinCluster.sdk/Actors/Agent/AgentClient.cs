using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpinCluster.sdk.Actors.Subscription;
using SpinCluster.sdk.Application;
using Toolbox.Rest;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Agent;

public class AgentClient
{
    protected readonly HttpClient _client;
    public AgentClient(HttpClient client) => _client = client.NotNull();

    public async Task<Option> Delete(string name, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.Agent}/{Uri.EscapeDataString(name)}")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .DeleteAsync(context)
        .ToOption();

    public async Task<Option<AgentModel>> Get(string nameId, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.Agent}/{Uri.EscapeDataString(nameId)}")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .GetAsync(context)
        .GetContent<AgentModel>();

    public async Task<Option> Set(AgentModel content, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.Agent}")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .SetContent(content)
        .PostAsync(context)
        .ToOption();
}
