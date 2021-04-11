using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Identity.sdk.Client
{
    public class IdentityClient : IIdentityClient
    {
        public IdentityClient(HttpClient httpClient, ILogger<IdentityClient> logger)
        {
            Signature = new SignatureClient(httpClient, logger);
            Subscription = new SubscriptionClient(httpClient, logger);
            Tenant = new TenantClient(httpClient, logger);
            User = new UserClient(httpClient, logger);
        }

        public SignatureClient Signature { get; }

        public SubscriptionClient Subscription { get; }

        public TenantClient Tenant { get; }

        public UserClient User { get; }
    }
}
