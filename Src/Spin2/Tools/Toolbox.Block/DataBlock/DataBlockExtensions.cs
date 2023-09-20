using Toolbox.Block;
using Toolbox.Extensions;
using Toolbox.Security.Principal;
using Toolbox.Security.Sign;
using Toolbox.Tools;
using Toolbox.Types;
using Toolbox.Types.MerkleTree;

namespace Toolbox.Block;

public static class DataBlockExtensions
{
    public static string CalculateDigest(this DataBlock dataBlock)
    {
        dataBlock.NotNull();

        var hashes = new string[]
        {
            $"{dataBlock.CreatedDate.ToString("o")}-{dataBlock.BlockType}-{dataBlock.BlockId}-{dataBlock.PrincipleId}".ToBytes().ToHexHash(),
            dataBlock.Data.ToBytes().ToHexHash(),
        };

        return hashes.ToMerkleHash();
    }

    public static DataBlock ToDataBlock<T>(this T subject, string principalId, string? blockType = null) where T : class
    {
        subject.NotNull();
        principalId.Assert(IdPatterns.IsPrincipalId, x => $"{x} not valid principalId");
        blockType ??= typeof(T).GetTypeName();

        DataBlock dataBlock = new DataBlockBuilder()
            .SetBlockType(blockType)
            .SetPrincipleId(principalId)
            .SetContent(subject)
            .Build();

        return dataBlock;
    }

    public static async Task<Option<DataBlock>> Sign(this DataBlock dataBlock, ISign sign, ScopeContext context)
    {
        dataBlock.NotNull();
        sign.NotNull();

        Option<SignResponse> jwt = await sign.SignDigest(dataBlock.PrincipleId, dataBlock.Digest, context.TraceId);
        if (jwt.IsError()) return jwt.ToOptionStatus<DataBlock>();

        return dataBlock with { JwtSignature = jwt.Return().JwtSignature };
    }

    public static T ToObject<T>(this DataBlock dataBlock)
    {
        dataBlock.NotNull();
        dataBlock.Data.NotEmpty();

        return dataBlock.Data
            .ToObject<T>()
            .NotNull(message: "Serialization error");
    }
}