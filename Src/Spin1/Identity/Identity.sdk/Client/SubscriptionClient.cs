//using System.Net.Http;
//using System.Threading;
//using System.Threading.Tasks;
//using Identity.sdk.Models;
//using Identity.sdk.Types;
//using Microsoft.Extensions.Logging;

//namespace Identity.sdk.Client
//{
//    public class SubscriptionClient : ClientBase<Subscription>
//    {
//        public SubscriptionClient(HttpClient httpClient, ILogger logger)
//            : base(httpClient, "subscription", logger)
//        {
//        }

//        public async Task<bool> Delete(IdentityId tenantId, IdentityId subscriptionId, CancellationToken token = default) =>
//            await Delete($"{(string)tenantId}/{(string)subscriptionId}", token);

//        public async Task<Subscription?> Get(IdentityId tenantId, IdentityId subscriptionId, CancellationToken token = default) =>
//                    await Get($"{(string)tenantId}/{(string)subscriptionId}", token);
//    }
//}