using Microsoft.Extensions.Logging;
using SpinCluster.sdk.Application;
using Toolbox.Rest;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Domain;

public class DomainClient
{
    protected readonly HttpClient _client;
    private readonly ILogger<DomainClient> _logger;

    public DomainClient(HttpClient client, ILogger<DomainClient> logger)
    {
        _client = client.NotNull();
        _logger = logger.NotNull();
    }

    public async Task<Option<DomainDetail>> GetDetails(string domain, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.Domain}/{Uri.EscapeDataString(domain)}")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .GetAsync(context.With(_logger))
        .GetContent<DomainDetail>();

    public async Task<Option<DomainList>> List(ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.Domain}/list")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .GetAsync(context.With(_logger))
        .GetContent<DomainList>();

    public async Task<Option> SetExternalDomain(string domain, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.Domain}/{Uri.EscapeDataString(domain)}/setExternalDomain")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .PostAsync(context.With(_logger))
        .ToOption();

    public async Task<Option> RemoveExternalDomain(string domain, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.Domain}/{Uri.EscapeDataString(domain)}/removeExternalDomain")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .PostAsync(context.With(_logger))
        .ToOption();
}
