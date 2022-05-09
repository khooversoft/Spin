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
        dataBlock.NotNull(nameof(dataBlock));

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
        dataBlock.NotNull(nameof(dataBlock));

        dataBlock.BlockType.NotEmpty(nameof(dataBlock.BlockType));
        dataBlock.BlockId.NotEmpty(nameof(dataBlock.BlockId));
        dataBlock.Data.NotEmpty(nameof(dataBlock.Data));
        dataBlock.PrincipleId.NotEmpty(nameof(dataBlock.PrincipleId));
        dataBlock.Digest.NotEmpty(nameof(dataBlock.Digest));
    }

    public static DataBlock ToDataBlock<T>(this T subject, string principalId) where T : class
    {
        subject.NotNull(nameof(subject));

        if (subject.GetType().IsAssignableTo(typeof(IEnumerable<T>)))
        {
            return ((IEnumerable<T>)subject).ToDataBlock(principalId);
        }

        return new DataBlockBuilder()
            .SetBlockType<T>()
            .SetPrincipleId(principalId)
            .SetData(subject)
            .Build();
    }

    public static DataBlock ToDataBlock<T>(this IEnumerable<T> subjects, string principalId)
    {
        subjects.NotNull(nameof(subjects));
        subjects.Count().Assert(x => x > 0, "Empty set");

        return new DataBlockBuilder()
            .SetBlockType<T>()
            .SetPrincipleId(principalId)
            .SetData(new List<T>(subjects))
            .Build();
    }

    public static T ToObject<T>(this DataBlock dataBlock)
    {
        dataBlock.NotNull(nameof(dataBlock));
        dataBlock.Data.NotEmpty(nameof(dataBlock.Data));

        return dataBlock.Data
            .ToObject<T>()
            .NotNull("Serialization error");
    }
}