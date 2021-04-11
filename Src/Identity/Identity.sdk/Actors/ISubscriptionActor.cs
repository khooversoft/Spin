using Identity.sdk.Models;
using System.Threading;
using System.Threading.Tasks;
using Toolbox.Actor;

namespace Identity.sdk.Actors
{
    public interface ISubscriptionActor : IActor
    {
        Task<bool> Delete(CancellationToken token);
        Task<Subscription?> Get(CancellationToken token);
        Task Set(Subscription subscription, CancellationToken token);
    }
}