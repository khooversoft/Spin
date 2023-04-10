using Toolbox.Block.Signature;
using Toolbox.Extensions;
using Toolbox.Security.Sign;
using Toolbox.Sign;

namespace InstallmentContract.Provider.Test.TestServices;

public class TestSigningClient : ISigningClient
{
    private readonly Dictionary<string, PrincipalSignature> _signatures = new Dictionary<string, PrincipalSignature>();

    public TestSigningClient Add(params (string kid, PrincipalSignature principalSignature)[] signers)
    {
        signers.ForEach(x => _signatures.Add(x.kid, x.principalSignature));
        return this;
    }

    public Task<SignRequestResponse> Sign(SignRequest signRequest, CancellationToken token = default)
    {
        IReadOnlyList<PrincipleDigest> result = signRequest.PrincipleDigests.Sign(x => _signatures[x]);

        var response = new SignRequestResponse
        {
            PrincipleDigests = result,
        };

        return Task.FromResult(response);
    }

    public Task<bool> Validate(ValidateRequest validateRequest, CancellationToken token = default)
    {
        validateRequest.PrincipleDigests.Validate(x => _signatures[x]);
        return Task.FromResult(true);
    }
}
