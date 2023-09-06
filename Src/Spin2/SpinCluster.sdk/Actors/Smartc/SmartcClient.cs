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

namespace SpinCluster.sdk.Actors.Smartc;

public class SmartcClient
{
    protected readonly HttpClient _client;
    public SmartcClient(HttpClient client) => _client = client.NotNull();

    public async Task<Option> Delete(string name, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.Smartc}/{Uri.EscapeDataString(name)}")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .DeleteAsync(context)
        .ToOption();

    public async Task<Option<SmartcModel>> Get(string nameId, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.Smartc}/{Uri.EscapeDataString(nameId)}")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .GetAsync(context)
        .GetContent<SmartcModel>();

    public async Task<Option> Set(SmartcModel content, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.Smartc}")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .SetContent(content)
        .PostAsync(context)
        .ToOption();
}