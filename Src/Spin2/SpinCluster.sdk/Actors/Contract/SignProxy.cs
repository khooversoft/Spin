using Microsoft.Extensions.Logging;
using SpinCluster.sdk.Application;
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

    public Task<Option<string>> SignDigest(string kid, string messageDigest, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);

        //Option<PrincipalId> ownerIdResult = PrincipalId.Create(kid).LogResult(context.Location());
        //if (ownerIdResult.IsError()) return ownerIdResult.ToOptionStatus<string>();

        //PrincipalId ownerId = ownerIdResult.Return();
        //string objectId = $"{SpinConstants.Schema.PrincipalKey}/{ownerId.Domain}/{ownerId}";

        //Option<string> result = await _client.GetSignatureActor().SignDigest(kid, messageDigest, context.TraceId);
        //return result;
        return Task.FromResult(new Option<string>(kid));
    }
}
