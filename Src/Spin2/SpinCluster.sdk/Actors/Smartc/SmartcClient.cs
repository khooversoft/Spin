using SpinCluster.sdk.Application;
using SpinCluster.sdk.Models;
using Toolbox.Rest;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Smartc;

public class SmartcClient
{
    protected readonly HttpClient _client;
    public SmartcClient(HttpClient client) => _client = client.NotNull();

    public async Task<Option> CompletedWork(SmartcRunResultModel content, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.Smartc}/completed")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .SetContent(content)
        .PostAsync(context)
        .ToOption();

    public async Task<Option> Delete(string smartcId, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.Smartc}/{Uri.EscapeDataString(smartcId)}")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .DeleteAsync(context)
        .ToOption();

    public async Task<Option<SmartcModel>> Get(string smartcId, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.Smartc}/{Uri.EscapeDataString(smartcId)}")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .GetAsync(context)
        .GetContent<SmartcModel>();

    public async Task<Option<AgentAssignmentModel>> GetAssignment(string smartcId, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.Smartc}/{Uri.EscapeDataString(smartcId)}/assignment")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .GetAsync(context)
        .GetContent<AgentAssignmentModel>();

    public async Task<Option> Exist(string smartcId, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.Smartc}/{Uri.EscapeDataString(smartcId)}/exist")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .GetAsync(context)
        .ToOption();

    public async Task<Option> Set(SmartcModel content, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.Smartc}")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .SetContent(content)
        .PostAsync(context)
        .ToOption();
}