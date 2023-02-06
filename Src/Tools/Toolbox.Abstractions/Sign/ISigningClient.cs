namespace Toolbox.Sign
{
    public interface ISigningClient
    {
        Task<SignRequestResponse> Sign(SignRequest signRequest, CancellationToken token);
        Task<bool> Validate(ValidateRequest validateRequest, CancellationToken token);
    }
}