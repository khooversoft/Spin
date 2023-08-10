using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpinCluster.sdk.Actors.ActorBase;
using SpinCluster.sdk.Application;
using Toolbox.Rest;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Subscription;

public class SubscriptionClient 
{
    protected readonly HttpClient _client;
    public SubscriptionClient(HttpClient client) => _client = client.NotNull();

    public async Task<Option> Delete(NameId nameId, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.Subscription}/{nameId.ToUrlEncoding()}")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .DeleteAsync(context)
        .ToOption();

    public async Task<Option<SubscriptionModel>> Get(NameId nameId, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.Subscription}/{nameId.ToUrlEncoding()}")
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
