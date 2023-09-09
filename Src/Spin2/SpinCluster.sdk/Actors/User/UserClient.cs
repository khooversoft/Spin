using SpinCluster.sdk.Application;
using Toolbox.Rest;
using Toolbox.Security.Sign;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.User;

public class UserClient
{
    protected readonly HttpClient _client;
    public UserClient(HttpClient client) => _client = client.NotNull();

    public async Task<Option> Delete(string userId, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.User}/{Uri.EscapeDataString(userId)}")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .DeleteAsync(context)
        .ToOption();

    public async Task<Option<UserModel>> Get(string userId, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.User}/{Uri.EscapeDataString(userId)}")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .GetAsync(context)
        .GetContent<UserModel>();

    public async Task<Option> Create(UserCreateModel content, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.User}/create")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .SetContent(content)
        .PostAsync(context)
        .ToOption();

    public async Task<Option> Update(UserModel content, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.User}")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .SetContent(content)
        .PostAsync(context)
        .ToOption();

    public async Task<Option<SignResponse>> Sign(SignRequest content, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.User}/sign")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .SetContent(content)
        .PostAsync(context)
        .GetContent<SignResponse>();
}
