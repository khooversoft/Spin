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
            string v => v.Split(';').All(x => blockChain.HasAccess(principalId, BlockGrant.Read, x).IsOk()),
            _ => blockChain.HasAccess(principalId, BlockRoleGrant.Owner).IsOk(),
        };

        if (!authorized) return StatusCode.Forbidden;

        IEnumerable<DataBlock> filter = blockType switch
        {
            string v => v.Split(';')
                .Join(blockChain.Blocks, x => x, x => x.DataBlock.BlockType, (o, i) => i.DataBlock)
                .ToArray(),

            null => blockChain.Blocks
                .Select(x => x.DataBlock)
                .ToArray()
        };

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

    public static bool HasAccess(this BlockChain subject, string principalId, BlockGrant grant, string blockType, out Option result)
    {
        result = subject.HasAccess(principalId, grant, blockType);
        return result.IsOk();
    }

    public static bool HasAccess(this BlockChain subject, string principalId, BlockRoleGrant grant, out Option result)
    {
        result = subject.HasAccess(principalId, grant);
        return result.IsOk();
    }
}