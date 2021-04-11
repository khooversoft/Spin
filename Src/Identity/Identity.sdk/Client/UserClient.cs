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
            : base(httpClient, "subscription", logger)
        {
        }

        public async Task<User?> Get(IdentityId tenantId, IdentityId subscriptionId, UserId userId, CancellationToken token = default) =>
            await Get(User.ToArtifactId(tenantId, subscriptionId, userId), token);

        public async Task<bool> Delete(IdentityId tenantId, IdentityId subscriptionId, UserId userId, CancellationToken token = default) =>
            await Delete(User.ToArtifactId(tenantId, subscriptionId, userId), token);
    }
}
