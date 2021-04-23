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
    public class TenantClient : ClientBase<Tenant>
    {
        public TenantClient(HttpClient httpClient, ILogger logger)
            : base(httpClient, "tenant", logger)
        {
        }

        public async Task<bool> Delete(IdentityId tenantId, CancellationToken token = default) =>
            await Delete((string)tenantId, token);

        public async Task<Tenant?> Get(IdentityId tenantId, CancellationToken token = default) =>
            await Get((string)tenantId, token);

        public async Task Set(Tenant tenant, CancellationToken token = default) =>
            await Set(tenant, (string)tenant.TenantId, token);
    }
}

