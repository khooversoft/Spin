using Microsoft.Extensions.Logging;
using SpinCluster.abstraction;
using Toolbox.Rest;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinClient.sdk;

public class SubscriptionClient
{
    protected readonly HttpClient _client;
    private readonly ILogger<SubscriptionClient> _logger;

    public SubscriptionClient(HttpClient client, ILogger<SubscriptionClient> logger)
    {
        _client = client.NotNull();
        _logger = logger.NotNull();
    }

    public async Task<Option> Delete(string name, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.Subscription}/{Uri.EscapeDataString(name)}")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .DeleteAsync(context.With(_logger))
        .ToOption();

    public async Task<Option> Exist(string nameId, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.Subscription}/{Uri.EscapeDataString(nameId)}/exist")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .GetAsync(context.With(_logger))
        .ToOption();

    public async Task<Option<SubscriptionModel>> Get(string nameId, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.Subscription}/{Uri.EscapeDataString(nameId)}")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .GetAsync(context.With(_logger))
        .GetContent<SubscriptionModel>();

    public async Task<Option> Set(SubscriptionModel content, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.Subscription}")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .SetContent(content)
        .PostAsync(context.With(_logger))
        .ToOption();
}
