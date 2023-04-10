//using Identity.sdk.Models;
//using Identity.sdk.Types;
//using Microsoft.Extensions.Logging;
//using System.Net.Http;
//using System.Threading;
//using System.Threading.Tasks;

//namespace Identity.sdk.Client
//{
//    public class TenantClient : ClientBase<Tenant>
//    {
//        public TenantClient(HttpClient httpClient, ILogger logger)
//            : base(httpClient, "tenant", logger)
//        {
//        }

//        public async Task<bool> Delete(IdentityId tenantId, CancellationToken token = default) =>
//            await Delete((string)tenantId, token);

//        public async Task<Tenant?> Get(IdentityId tenantId, CancellationToken token = default) =>
//            await Get((string)tenantId, token);
//    }
//}