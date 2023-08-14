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
    public static Option<BlockReader<T>> GetReader<T>(this BlockChain blockChain, PrincipalId principalId) where T : class
    {
        return blockChain.NotNull().GetReader<T>(typeof(T).Name, principalId);
    }

    public static Option<BlockWriter<T>> GetWriter<T>(this BlockChain blockChain, PrincipalId principalId) where T : class
    {
        return blockChain.NotNull().GetWriter<T>(typeof(T).Name, principalId);
    }

    public static BlobPackage ToBlobPackage(this BlockChain blockChain)
    {
        GenesisBlock genesisBlock = blockChain.GetGenesisBlock();

        var blob = new BlobPackageBuilder()
            .SetObjectId(genesisBlock.ObjectId)
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