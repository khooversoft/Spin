using Toolbox.Extensions;
using Toolbox.Security.Principal;
using Toolbox.Security.Sign;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Block;

public static class BlockChainExtensions
{
    public static Option<IEnumerable<DataBlock>> Filter(this BlockChain blockChain, string principalId, string? blockType = null)
    {
        blockChain.NotNull();

        bool authorized = blockType switch
        {
            string v => blockChain.IsAuthorized(BlockGrant.Read, v, principalId).IsOk(),
            _ => blockChain.IsOwner(principalId).IsOk(),
        };

        if (!authorized) return StatusCode.Forbidden;

        IEnumerable<DataBlock> filter = blockChain.Blocks
            .Select(x => x.DataBlock)
            .Where(x => blockType == null || x.BlockType == blockType);

        return filter.ToOption();
    }

    public static Option<IEnumerable<DataBlock>> Filter<T>(this BlockChain blockChain, string principalId)
    {
        return blockChain.Filter(principalId, typeof(T).Name);
    }

    public static async Task<Option> ValidateBlockChain(this BlockChain blockChain, ISignValidate signValidate, ScopeContext context)
    {
        blockChain.NotNull();
        signValidate.NotNull();

        if (!blockChain.IsValid()) return new Option(StatusCode.BadRequest, "Block chain is not valid");

        IReadOnlyList<PrincipalDigest> principleDigests = blockChain.GetPrincipleDigests();

        var list = new List<string>();
        foreach (var digest in principleDigests)
        {
            Option result = await signValidate.ValidateDigest(digest.JwtSignature, digest.MessageDigest, context.TraceId);
            if (result.StatusCode.IsError())
            {
                list.Add(result.Error ?? "< no error message >");
            }
        }

        return list.Count switch
        {
            0 => new Option(StatusCode.OK),
            _ => new Option(StatusCode.Conflict, list.Join(", ")),
        };
    }
}