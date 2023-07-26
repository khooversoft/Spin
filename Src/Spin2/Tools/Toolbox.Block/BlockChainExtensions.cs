using Toolbox.Block;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Security.Jwt;
using Toolbox.Security.Principal;
using Toolbox.Security.Sign;
using Toolbox.Tools;
using Toolbox.Types;
using Toolbox.Types.MerkleTree;

namespace Toolbox.Block;

public static class BlockChainExtensions
{
    public static IReadOnlyList<PrincipalDigest> GetPrincipleDigests(this BlockChain blockChain)
    {
        blockChain.NotNull();

        return blockChain.Blocks
            .Select(x => new PrincipalDigest
            {
                Id = x.DataBlock.BlockId,
                PrincipleId = x.DataBlock.PrincipleId,
                MessageDigest = x.DataBlock.Digest,
                JwtSignature = x.DataBlock.JwtSignature,
            }).ToArray();
    }

    public static IReadOnlyList<T> GetTypedBlocks<T>(this BlockChain blockChain) => blockChain.NotNull()
        .GetTypedBlocks(typeof(T).GetTypeName())
        .Select(x => x.ToObject<T>())
        .ToArray();

    public static IReadOnlyList<T> GetTypedBlocks<T>(this BlockChain blockChain, string blockType) => blockChain.NotNull()
        .GetTypedBlocks(blockType)
        .Select(x => x.ToObject<T>())
        .ToArray();

    public static IReadOnlyList<DataBlock> GetTypedBlocks(this BlockChain blockChain, string blockType) => blockChain.NotNull()
        .Blocks
        .Where(x => x.DataBlock.BlockType == blockType)
        .Select(x => x.DataBlock)
        .ToList();

    public static Option<GenesisBlock> GetGenesisBlock(this BlockChain blockChain) => blockChain
        .GetTypedBlocks<GenesisBlock>(GenesisBlock.BlockType)
        .FirstOrDefaultOption();

    public static Option<BlockAcl> GetAclBlock(this BlockChain blockChain) => blockChain
        .GetTypedBlocks<BlockAcl>(BlockAcl.BlockType)
        .LastOrDefaultOption();

    public static Option CheckWriteAccess(this BlockChain blockChain, IEnumerable<DataBlock> blocks)
    {
        blocks.NotNull();

        Option<BlockAcl> aclOption = blockChain.GetAclBlock();
        if (aclOption == Option<BlockAcl>.None) return new Option(StatusCode.OK);

        BlockAcl acl = aclOption.Return();

        var errors = blocks
            .Select(x => acl.HasWriteAccess(new NameId(x.BlockType), new PrincipalId(x.PrincipleId)) switch
                {
                    true => (string)null!,
                    false => $"Principal={x.PrincipleId} is not authorized for write",
                })
            .Where(x => x != null)
            .ToArray();

        return errors.Length switch
        {
            0 => new Option(StatusCode.OK),
            _ => new Option(StatusCode.Unauthorized, errors.Join(",")),
        };
    }

    public static BlobPackage ToBlobPackage(this BlockChain blockChain)
    {
        GenesisBlock genesisBlock = blockChain.GetGenesisBlock().Return();

        var blob = new BlobPackageBuilder()
            .SetObjectId(genesisBlock.ObjectId.ToObjectId())
            .SetContent(blockChain.ToBlockChainModel())
            .Build();

        return blob;
    }

    public static Option<BlockChain> ToBlockChain(this BlobPackage package, ScopeContext context)
    {
        var validationOption = package.Validate(context.Location()).ToOption<BlockChain>();
        if (validationOption.IsError()) return validationOption;

        BlockChain blockChain = package
            .ToObject<BlockChainModel>()
            .ToBlockChain();

        return blockChain;
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
            Option result = await signValidate.ValidateDigest(digest.JwtSignature, digest.MessageDigest, context);
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