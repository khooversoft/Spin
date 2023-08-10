using Microsoft.Extensions.Logging;
using SpinCluster.sdk.Actors.Signature;
using SpinCluster.sdk.Application;
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
        Option<PrincipalId> ownerIdResult = PrincipalId.Create(kid).LogResult(context.Location());
        if (ownerIdResult.IsError()) return ownerIdResult.ToOptionStatus<string>();

        PrincipalId ownerId = ownerIdResult.Return();
        string objectId = $"{SpinConstants.Schema.PrincipalKey}/{ownerId.Domain}/{ownerId}";

        ISignatureActor signatureActor = _client.GetGrain<ISignatureActor>(objectId);
        Option<string> result = await signatureActor.Sign(messageDigest, context.TraceId);
        return result;
    }
}
