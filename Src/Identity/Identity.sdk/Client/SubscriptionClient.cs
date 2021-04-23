using ArtifactStore.sdk.Model;
using Identity.sdk.Models;
using Identity.sdk.Types;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Toolbox.Model;
using Toolbox.Tools;

namespace Identity.sdk.Client
{
    public class SubscriptionClient : ClientBase<Subscription>
    {
        public SubscriptionClient(HttpClient httpClient, ILogger logger)
            : base(httpClient, "subscription", logger)
        {
        }

        public async Task<bool> Delete(IdentityId tenantId, IdentityId subscriptionId, CancellationToken token = default) =>
            await Delete($"{(string)tenantId}/{(string)subscriptionId}", token);

        public async Task<Subscription?> Get(IdentityId tenantId, IdentityId subscriptionId, CancellationToken token = default) =>
                    await Get($"{(string)tenantId}/{(string)subscriptionId}", token);

        public async Task Set(Subscription subscription, CancellationToken token = default) =>
            await Set(subscription, $"{(string)subscription.TenantId}/{(string)subscription.SubscriptionId}", token);
    }
}
