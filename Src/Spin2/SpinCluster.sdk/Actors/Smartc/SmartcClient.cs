using Microsoft.Extensions.Logging;
using SpinCluster.sdk.Application;
using Toolbox.Data;
using Toolbox.Rest;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Smartc;

public class SmartcClient
{
    protected readonly HttpClient _client;
    private readonly ILogger<SmartcClient> _logger;

    public SmartcClient(HttpClient client, ILogger<SmartcClient> logger)
    {
        _client = client.NotNull();
        _logger = logger.NotNull();
    }

    public async Task<Option> Delete(string smartcId, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.Smartc}/{Uri.EscapeDataString(smartcId)}")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .DeleteAsync(context.With(_logger))
        .ToOption();

    public async Task<Option> Exist(string smartcId, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.Smartc}/{Uri.EscapeDataString(smartcId)}/exist")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .GetAsync(context.With(_logger))
        .ToOption();

    public async Task<Option<SmartcModel>> Get(string smartcId, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.Smartc}/{Uri.EscapeDataString(smartcId)}")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .GetAsync(context.With(_logger))
        .GetContent<SmartcModel>();

    public async Task<Option> Set(SmartcModel content, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.Smartc}")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .SetContent(content)
        .PostAsync(context.With(_logger))
        .ToOption();

    public async Task<Option> SetPayload(string smartcId, DataObject content, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.Smartc}/{Uri.EscapeDataString(smartcId)}/setPayload")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .SetContent(content)
        .PostAsync(context.With(_logger))
        .ToOption();
}