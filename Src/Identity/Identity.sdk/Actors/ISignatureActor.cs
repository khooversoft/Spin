using Identity.sdk.Models;
using System.Threading;
using System.Threading.Tasks;
using Toolbox.Actor;

namespace Identity.sdk.Actors
{
    public interface ISignatureActor : IActor
    {
        Task<bool> Delete(CancellationToken token);
        Task<Signature?> Get(CancellationToken token);
        Task Set(Signature signature, CancellationToken token);
    }
}