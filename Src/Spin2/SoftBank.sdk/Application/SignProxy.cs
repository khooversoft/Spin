using Microsoft.Extensions.Logging;
using SpinCluster.sdk.Actors.Signature;
using Toolbox.Orleans.Types;
using Toolbox.Security.Principal;
using Toolbox.Tools;
using Toolbox.Types;

namespace SoftBank.sdk.Application;

public class SignProxy : ISign
{
    private readonly IClusterClient _client;
    private readonly ILogger<SignProxy> _logger;

    public SignProxy(IClusterClient client, ILogger<SignProxy> logger)
    {
        _client = client.NotNull();
        _logger = logger.NotNull();
    }

    public async Task<Option<string>> SignDigest(string kid, string messageDigest, ScopeContext context)
    {
        ISignatureActor signatureActor = _client.GetGrain<ISignatureActor>(kid);
        SpinResponse<string> result = await signatureActor.Sign(messageDigest, context.TraceId);
        return result.ToOption();
    }
}
