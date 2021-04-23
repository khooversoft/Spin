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
    public class UserClient : ClientBase<User>
    {
        public UserClient(HttpClient httpClient, ILogger logger)
            : base(httpClient, "user", logger)
        {
        }

        public async Task<bool> Delete(IdentityId tenantId, IdentityId subscriptionId, UserId userId, CancellationToken token = default) =>
            await Delete($"{(string)tenantId}/{(string)subscriptionId}/{(string)userId}", token);

        public async Task<User?> Get(IdentityId tenantId, IdentityId subscriptionId, UserId userId, CancellationToken token = default) =>
                    await Get($"{(string)tenantId}/{(string)subscriptionId}/{(string)userId}", token);

        public async Task Set(User user, CancellationToken token) =>
            await Set(user, $"{(string)user.TenantId}/{(string)user.SubscriptionId}/{(string)user.UserId}", token);
    }
}
