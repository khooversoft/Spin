using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Security;
using Toolbox.Security.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Block;

public record DataBlock
{
    public long TimeStamp { get; init; } = UnixDate.UtcNow;

    public string BlockType { get; init; } = null!;

    public string BlockId { get; init; } = null!;

    public string Data { get; init; } = null!;

    public string PrincipleId { get; init; } = null!;

    public string? JwtSignature { get; init; }

    public string Digest { get; init; } = null!;
}


public static class DataBlockExtensions
{
    public static string GetDigest(this DataBlock dataBlock)
    {
        dataBlock.VerifyNotNull(nameof(dataBlock));

        var hashes = new string[]
        {
                $"{dataBlock.TimeStamp}-{dataBlock.BlockType}-{dataBlock.BlockId}-{dataBlock.PrincipleId}".ToBytes().ToSHA256Hash(),
                dataBlock.Data.ToBytes().ToSHA256Hash(),
        };

        return hashes.ToMerkleHash();
    }

    public static bool IsValid(this DataBlock dataBlock) => dataBlock.Digest == dataBlock.GetDigest();

    public static void Verify(this DataBlock dataBlock)
    {
        dataBlock.VerifyNotNull(nameof(dataBlock));

        dataBlock.BlockType.VerifyNotEmpty(nameof(dataBlock.BlockType));
        dataBlock.BlockId.VerifyNotEmpty(nameof(dataBlock.BlockId));
        dataBlock.Data.VerifyNotEmpty(nameof(dataBlock.Data));
        dataBlock.PrincipleId.VerifyNotEmpty(nameof(dataBlock.PrincipleId));
        dataBlock.Digest.VerifyNotEmpty(nameof(dataBlock.Digest));
    }
}