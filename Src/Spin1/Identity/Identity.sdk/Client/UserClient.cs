//using System.Net.Http;
//using System.Threading;
//using System.Threading.Tasks;
//using Identity.sdk.Models;
//using Identity.sdk.Types;
//using Microsoft.Extensions.Logging;

//namespace Identity.sdk.Client
//{
//    public class UserClient : ClientBase<User>
//    {
//        public UserClient(HttpClient httpClient, ILogger logger)
//            : base(httpClient, "user", logger)
//        {
//        }

//        public async Task<bool> Delete(IdentityId tenantId, IdentityId subscriptionId, UserId userId, CancellationToken token = default) =>
//            await Delete($"{(string)tenantId}/{(string)subscriptionId}/{(string)userId}", token);

//        public async Task<User?> Get(IdentityId tenantId, IdentityId subscriptionId, UserId userId, CancellationToken token = default) =>
//                    await Get($"{(string)tenantId}/{(string)subscriptionId}/{(string)userId}", token);
//    }
//}