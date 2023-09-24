using Microsoft.Extensions.Logging;
using SpinCluster.sdk.Application;
using Toolbox.Rest;
using Toolbox.Security.Principal;
using Toolbox.Security.Sign;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Signature;

public class SignatureClient : ISign, ISignValidate
{
    private readonly HttpClient _client;
    private readonly ILogger<SignatureClient> _logger;

    public SignatureClient(HttpClient client, ILogger<SignatureClient> logger)
    {
        _client = client.NotNull();
        _logger = logger.NotNull();
    }


    public async Task<Option<SignResponse>> SignDigest(string principalId, string messageDigest, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);

        var request = new SignRequest
        {
            PrincipalId = principalId,
            MessageDigest = messageDigest,
        };

        return await SignDigest(request, context);
    }

    public async Task<Option<SignResponse>> SignDigest(SignRequest model, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.Signature}/sign")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .SetContent(model)
        .PostAsync(context.With(_logger))
        .GetContent<SignResponse>();


    public async Task<Option> ValidateDigest(SignValidateRequest model, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.Signature}/validate")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .SetContent(model)
        .PostAsync(context.With(_logger))
        .ToOption();

    public Task<Option> ValidateDigest(string jwtSignature, string messageDigest, string traceId)
    {
        var context = new ScopeContext(_logger);

        var request = new SignValidateRequest
        {
            JwtSignature = jwtSignature,
            MessageDigest = messageDigest,
        };

        return ValidateDigest(request, context);
    }
}
