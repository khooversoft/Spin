using SpinCluster.sdk.Actors.ActorBase;
using SpinCluster.sdk.Actors.Signature;
using SpinCluster.sdk.Application;
using SpinCluster.sdk.Types;
using Toolbox.Rest;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.PrincipalKey;

public class SignatureClient
{
    private readonly HttpClient _client;

    public SignatureClient(HttpClient client)
    {
        _client = client;
    }

    public Task<Option> Delete(ObjectId id, ScopeContext context) => _client.Delete(SpinConstants.Schema.Signature, id, context);

    public Task<Option> Exist(ObjectId id, ScopeContext context) => _client.Exist(SpinConstants.Schema.Signature, id, context);

    public async Task<Option> Create(ObjectId id, PrincipalKeyRequest model, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.Signature}/{id}")
        .AddHeader(SpinConstants.Protocol.TraceId, context.TraceId)
        .SetContent(model)
        .PostAsync(context)
        .ToOption();

    public async Task<Option<string>> Sign(ObjectId id, ValidateRequest model, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.Signature}/sign/{id}")
        .AddHeader(SpinConstants.Protocol.TraceId, context.TraceId)
        .SetContent(model)
        .PostAsync(context)
        .GetContent<string>();

    public async Task<Option> ValidateJwtSignature(ObjectId id, ValidateRequest model, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.Signature}/validate/{id}")
        .AddHeader(SpinConstants.Protocol.TraceId, context.TraceId)
        .SetContent(model)
        .PostAsync(context)
        .ToOption();
}
