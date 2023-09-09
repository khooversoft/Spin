using Toolbox.Types;

namespace SpinCluster.sdk.Application;

public static class IdTool
{
    // Systems
    public static ResourceId CreateSubscriptionId(string subscriptionId) => $"{SpinConstants.Schema.Subscription}:{subscriptionId}";

    // tenant:company3.com
    public static ResourceId CreateTenantId(string tenant) => $"{SpinConstants.Schema.Tenant}:{tenant}";

    // user:user1@company3.com
    public static ResourceId CreateUserId(string principalId) => $"{SpinConstants.Schema.User}:{principalId}";

    // kid:user1@company3.com[/path]
    public static ResourceId CreateKid(string principalId, string? path = null) => path switch
    {
        null => $"{SpinConstants.Schema.Kid}:{principalId}",
        var v => $"{SpinConstants.Schema.Kid}:{principalId}/{v}",
    };

    // principal-key:user1@company3.com[/path]"
    public static ResourceId CreatePublicKeyId(string principalId, string? path = null) => path switch
    {
        null => $"{SpinConstants.Schema.PrincipalKey}:{principalId}",
        var v => $"{SpinConstants.Schema.PrincipalKey}:{principalId}/{v}",
    };

    // principal-private-key:user1@company3.com"
    public static ResourceId CreatePrivateKeyId(string principalId, string? path = null) => path switch
    {
        null => $"{SpinConstants.Schema.PrincipalPrivateKey}:{principalId}",
        var v => $"{SpinConstants.Schema.PrincipalPrivateKey}:{principalId}/{v}",
    };

    public static ResourceId CreateLeaseId(string domain, string path) => $"{SpinConstants.Schema.Lease}:{domain}/{path}";
}

