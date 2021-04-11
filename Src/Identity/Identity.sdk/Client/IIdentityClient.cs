namespace Identity.sdk.Client
{
    public interface IIdentityClient
    {
        SignatureClient Signature { get; }
        SubscriptionClient Subscription { get; }
        TenantClient Tenant { get; }
        UserClient User { get; }
    }
}