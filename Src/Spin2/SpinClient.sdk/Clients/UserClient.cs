using Microsoft.Extensions.Logging;
using SpinCluster.abstraction;
using Toolbox.Rest;
using Toolbox.Security.Sign;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinClient.sdk;

public class UserClient
{
    protected readonly HttpClient _client;
    private readonly ILogger<UserClient> _logger;

    public UserClient(HttpClient client, ILogger<UserClient> logger)
    {
        _client = client.NotNull();
        _logger = logger.NotNull();
    }

    public async Task<Option> Delete(string userId, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.User}/{Uri.EscapeDataString(userId)}")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .DeleteAsync(context.With(_logger))
        .ToOption();

    public async Task<Option> Exist(string userId, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.User}/{Uri.EscapeDataString(userId)}/exist")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .DeleteAsync(context.With(_logger))
        .ToOption();

    public async Task<Option<UserModel>> Get(string userId, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.User}/{Uri.EscapeDataString(userId)}")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .GetAsync(context.With(_logger))
        .GetContent<UserModel>();

    public async Task<Option> Create(UserCreateModel content, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.User}/create")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .SetContent(content)
        .PostAsync(context.With(_logger))
        .ToOption();

    public async Task<Option> Update(UserModel content, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.User}")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .SetContent(content)
        .PostAsync(context.With(_logger))
        .ToOption();

    public async Task<Option<SignResponse>> Sign(SignRequest content, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.User}/sign")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .SetContent(content)
        .PostAsync(context.With(_logger))
        .GetContent<SignResponse>();
}
