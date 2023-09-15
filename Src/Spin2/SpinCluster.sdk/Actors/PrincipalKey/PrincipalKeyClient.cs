using Microsoft.Extensions.Logging;
using SpinCluster.sdk.Application;
using Toolbox.Rest;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.PrincipalKey;

public class PrincipalKeyClient
{
    protected readonly HttpClient _client;
    private readonly ILogger<PrincipalKeyClient> _logger;

    public PrincipalKeyClient(HttpClient client, ILogger<PrincipalKeyClient> logger)
    {
        _client = client.NotNull();
        _logger = logger.NotNull();
    }

    public async Task<Option> Delete(string principalId, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.PrincipalKey}/{Uri.EscapeDataString(principalId)}")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .DeleteAsync(context.With(_logger))
        .ToOption();

    public async Task<Option> Delete(string principalId, string path, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.PrincipalKey}/{Uri.EscapeDataString(principalId)}/{Uri.EscapeDataString(path)}")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .DeleteAsync(context.With(_logger))
        .ToOption();

    public async Task<Option<PrincipalKeyModel>> Get(string principalId, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.PrincipalKey}/{Uri.EscapeDataString(principalId)}")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .GetAsync(context.With(_logger))
        .GetContent<PrincipalKeyModel>();

    public async Task<Option<PrincipalKeyModel>> Get(string principalId, string path, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.PrincipalKey}/{Uri.EscapeDataString(principalId)}/{Uri.EscapeDataString(path)}")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .GetAsync(context.With(_logger))
        .GetContent<PrincipalKeyModel>();

    public async Task<Option> Create(PrincipalKeyCreateModel content, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.PrincipalKey}/create")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .SetContent(content)
        .PostAsync(context.With(_logger))
        .ToOption();

    public async Task<Option> Update(PrincipalKeyModel content, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.PrincipalKey}")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .SetContent(content)
        .PostAsync(context.With(_logger))
        .ToOption();
}
