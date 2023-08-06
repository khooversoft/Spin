using Microsoft.Extensions.Logging;
using SpinCluster.sdk.Actors.Signature;
using Toolbox.Orleans.Types;
using Toolbox.Security.Jwt;
using Toolbox.Security.Principal;
using Toolbox.Tools;
using Toolbox.Types;

namespace SoftBank.sdk.Application;

public class SignValidationProxy : ISignValidate
{
    private readonly IClusterClient _client;
    private readonly ILogger<SignProxy> _logger;

    public SignValidationProxy(IClusterClient client, ILogger<SignProxy> logger)
    {
        _client = client.NotNull();
        _logger = logger.NotNull();
    }

    public async Task<Option> ValidateDigest(string jwtSignature, string messageDigest, ScopeContext context)
    {
        string? kid = JwtTokenParser.GetKidFromJwtToken(jwtSignature);
        if (kid == null) return new Option(StatusCode.BadRequest, "no kid in jwtSignature");

        ISignatureActor signatureActor = _client.GetGrain<ISignatureActor>(kid);
        Option result = await signatureActor.ValidateJwtSignature(jwtSignature, messageDigest, context.TraceId);
        return result;
    }
}
