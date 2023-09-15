using Microsoft.Extensions.Logging;
using SpinCluster.sdk.Actors.PrincipalPrivateKey;
using SpinCluster.sdk.Application;
using Toolbox.Rest;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.PrincipalKey;

public class PrincipalPrivateKeyClient
{
    protected readonly HttpClient _client;
    private readonly ILogger<PrincipalPrivateKeyClient> _logger;

    public PrincipalPrivateKeyClient(HttpClient client, ILogger<PrincipalPrivateKeyClient> logger)
    {
        _client = client.NotNull();
        _logger = logger.NotNull();
    }

    public async Task<Option> Delete(string principalId, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.PrincipalPrivateKey}/{Uri.EscapeDataString(principalId)}")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .DeleteAsync(context.With(_logger))
        .ToOption();

    public async Task<Option> Delete(string principalId, string path, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.PrincipalPrivateKey}/{Uri.EscapeDataString(principalId)}/{Uri.EscapeDataString(path)}")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .DeleteAsync(context.With(_logger))
        .ToOption();

    public async Task<Option<PrincipalPrivateKeyModel>> Get(string principalId, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.PrincipalPrivateKey}/{Uri.EscapeDataString(principalId)}")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .GetAsync(context.With(_logger))
        .GetContent<PrincipalPrivateKeyModel>();

    public async Task<Option<PrincipalPrivateKeyModel>> Get(string principalId, string path, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.PrincipalPrivateKey}/{Uri.EscapeDataString(principalId)}/{Uri.EscapeDataString(path)}")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .GetAsync(context.With(_logger))
        .GetContent<PrincipalPrivateKeyModel>();

    public async Task<Option> Set(PrincipalPrivateKeyModel content, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.PrincipalPrivateKey}")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .SetContent(content)
        .PostAsync(context.With(_logger))
        .ToOption();
}
