using System.Threading;
using System.Threading.Tasks;
using Spin.Common.Sign;

namespace Spin.Common.Client
{
    public interface ISigningClient
    {
        Task<SignRequestResponse> Sign(SignRequest signRequest, CancellationToken token = default);
        Task<bool> Validate(ValidateRequest validateRequest, CancellationToken token = default);
    }
}