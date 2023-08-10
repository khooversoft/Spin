using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpinCluster.sdk.Actors.ActorBase;
using SpinCluster.sdk.Actors.Tenant;
using SpinCluster.sdk.Application;
using Toolbox.Rest;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.User;

public class UserClient
{
    protected readonly HttpClient _client;
    public UserClient(HttpClient client) => _client = client.NotNull();

    public async Task<Option> Delete(NameId nameId, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.User}/{nameId.ToUrlEncoding()}")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .DeleteAsync(context)
        .ToOption();

    public async Task<Option<UserModel>> Get(NameId nameId, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.Tenant}/{nameId.ToUrlEncoding()}")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .GetAsync(context)
        .GetContent<UserModel>();

    public async Task<Option> Set(UserModel content, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.Tenant}")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .SetContent(content)
        .PostAsync(context)
        .ToOption();
}
