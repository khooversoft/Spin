using SpinCluster.sdk.Application;
using Toolbox.Rest;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Subscription;

public class SubscriptionClient
{
    protected readonly HttpClient _client;
    public SubscriptionClient(HttpClient client) => _client = client.NotNull();

    public async Task<Option> Delete(string name, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.Subscription}/{Uri.EscapeDataString(name)}")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .DeleteAsync(context)
        .ToOption();

    public async Task<Option<SubscriptionModel>> Get(string nameId, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.Subscription}/{Uri.EscapeDataString(nameId)}")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .GetAsync(context)
        .GetContent<SubscriptionModel>();

    public async Task<Option> Set(SubscriptionModel content, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.Subscription}")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .SetContent(content)
        .PostAsync(context)
        .ToOption();
}
