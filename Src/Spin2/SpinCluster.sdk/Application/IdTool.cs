using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Types;

namespace SpinCluster.sdk.Application;

public static class IdTool
{
    public static ObjectId CreateSubscriptionId(NameId nameId) => $"{SpinConstants.Schema.Subscription}/$system/{nameId}";
    public static ObjectId CreateTenantId(TenantId tenantId) => $"{SpinConstants.Schema.Tenant}/$system/{tenantId}";
    public static ObjectId CreateUserId(PrincipalId principalId) => $"{SpinConstants.Schema.User}/{principalId.Domain}/{principalId}";
    public static ObjectId CreatePublicKeyId(PrincipalId principalId) => $"{SpinConstants.Schema.PrincipalKey}/{principalId.Domain}/{principalId}";
    public static ObjectId CreatePrivateKeyId(PrincipalId principalId) => $"{SpinConstants.Schema.PrincipalPrivateKey}/{principalId.Domain}/{principalId}";

    public static KeyId CreateKeyId(PrincipalId principalId, string? name = null) => KeyId.Create(principalId, name).Return();
    public static ObjectId CreatePublicKeyObjectId(KeyId keyId) => InternalToKeyId(keyId, SpinConstants.Schema.PrincipalKey);
    public static ObjectId CreatePrivateKeyObjectId(KeyId keyId) => InternalToKeyId(keyId, SpinConstants.Schema.PrincipalPrivateKey);

    private static ObjectId InternalToKeyId(KeyId keyId, string schema)
    {
        schema.IsNotEmpty();

        (PrincipalId principalId, string? path) = keyId.GetDetails();

        string id = path.ToNullIfEmpty() switch
        {
            null => $"{principalId.Domain}/{principalId}",
            not null => $"{principalId.Domain}/{principalId}/{path}",
        };

        return $"{schema}/{id}";
    }

    // subscription:subscriptionId
    public static ResourceId CreateSubscription(string subscriptionId) => $"{SpinConstants.Schema.Subscription}:{subscriptionId}";

    // tenant:company3.com
    public static ResourceId CreateTenant(string tenant) => $"{SpinConstants.Schema.Tenant}:{tenant}";

    // user:user1@company3.com
    public static ResourceId CreateUser(string principalId) => $"{SpinConstants.Schema.User}:{principalId}";

    // kid:user1@company3.com[/path]
    public static ResourceId CreateKid(string principalId, string? path = null) => path switch
    {
        null => $"{SpinConstants.Schema.Kid}:{principalId}",
        var v => $"{SpinConstants.Schema.Kid}:{principalId}/{v}",
    };

    // principal-key:user1@company3.com[/path]"
    public static ResourceId CreatePublicKey(string principalId, string? path = null) => path switch
    {
        null => $"{SpinConstants.Schema.PrincipalKey}:{principalId}",
        var v => $"{SpinConstants.Schema.PrincipalKey}:{principalId}/{v}",
    };

    // principal-private-key:user1@company3.com"
    public static ResourceId CreatePrivateKey(string principalId, string? path = null) => path switch
    {
        null => $"{SpinConstants.Schema.PrincipalPrivateKey}:{principalId}",
        var v => $"{SpinConstants.Schema.PrincipalPrivateKey}:{principalId}/{v}",
    };
}

