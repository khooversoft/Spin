using Microsoft.Extensions.Logging;
using SpinCluster.sdk.Actors.Tenant;
using SpinCluster.sdk.Application;
using Toolbox.Rest;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Configuration;

public class ConfigClient
{
    protected readonly HttpClient _client;
    private readonly ILogger<ConfigClient> _logger;

    public ConfigClient(HttpClient client, ILogger<ConfigClient> logger)
    {
        _client = client.NotNull();
        _logger = logger;
    }

    public async Task<Option> Delete(string configId, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.Config}/{Uri.EscapeDataString(configId)}")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .DeleteAsync(context.With(_logger))
        .ToOption();

    public async Task<Option> Exist(string configId, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.Config}/{Uri.EscapeDataString(configId)}/exist")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .GetAsync(context.With(_logger))
        .ToOption();

    public async Task<Option<ConfigModel>> Get(string configId, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.Config}/{Uri.EscapeDataString(configId)}")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .GetAsync(context.With(_logger))
        .GetContent<ConfigModel>();

    public async Task<Option> Set(ConfigModel configId, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.Config}")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .SetContent(configId)
        .PostAsync(context.With(_logger))
        .ToOption();
}
