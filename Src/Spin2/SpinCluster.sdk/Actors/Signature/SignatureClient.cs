using SpinCluster.sdk.Actors.Signature;
using SpinCluster.sdk.Actors.User;
using SpinCluster.sdk.Application;
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

    public async Task<Option<SignResponse>> Sign(SignRequest model, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.Signature}/sign")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .SetContent(model)
        .PostAsync(context)
        .GetContent<SignResponse>();

    public async Task<Option> ValidateDigest(ValidateRequest model, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.Signature}/validate")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .SetContent(model)
        .PostAsync(context)
        .ToOption();
}
