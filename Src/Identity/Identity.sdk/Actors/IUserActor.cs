using Identity.sdk.Models;
using System.Threading;
using System.Threading.Tasks;
using Toolbox.Actor;

namespace Identity.sdk.Actors
{
    public interface IUserActor : IActor
    {
        Task<bool> Delete(CancellationToken token);
        Task<User?> Get(CancellationToken token);
        Task Set(User user, CancellationToken token);
    }
}