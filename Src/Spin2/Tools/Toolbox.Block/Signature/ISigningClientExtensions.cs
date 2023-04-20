using Toolbox.Block.Container;
using Toolbox.Security.Sign;
using Toolbox.Tools;

namespace Toolbox.Block.Signature;

public static class ISigningClientExtensions
{
    public static async Task<BlockChain> Sign(this ISigningClient subject, BlockChain blockChain, CancellationToken token)
    {
        subject.NotNull();
        blockChain.NotNull();

        var principleDigests = blockChain.GetPrincipleDigests();
        var request = principleDigests.ToSignRequest();

        SignRequestResponse response = await subject.Sign(request, token);
        return blockChain.Sign(response.PrincipleDigests);
    }

    public static async Task<bool> Validate(this ISigningClient subject, BlockChain blockChain, CancellationToken token)
    {
        subject.NotNull();
        blockChain.NotNull();

        var principleDigests = blockChain.GetPrincipleDigests();
        var request = principleDigests.ToValidateRequest();

        return await subject.Validate(request, token);
    }
}
