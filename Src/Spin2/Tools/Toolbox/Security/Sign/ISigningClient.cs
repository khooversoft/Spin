using Toolbox.Types;

namespace Toolbox.Security.Sign;

public interface ISigningClient
{
    Task<SignResponse> Sign(SignRequest signRequest, CancellationToken token);
    Task<Option> Validate(ValidateRequest validateRequest, CancellationToken token);
}