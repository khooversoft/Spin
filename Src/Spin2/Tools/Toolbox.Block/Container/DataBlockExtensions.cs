using Toolbox.Block.Application;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types.MerkleTree;

namespace Toolbox.Block.Container;

public static class DataBlockExtensions
{
    public static string GetDigest(this DataBlock dataBlock)
    {
        dataBlock.NotNull();

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
        dataBlock.NotNull();

        dataBlock.BlockType.NotEmpty();
        dataBlock.ObjectClass.NotEmpty();
        dataBlock.BlockId.NotEmpty();
        dataBlock.Data.NotEmpty();
        dataBlock.PrincipleId.NotEmpty();
        dataBlock.Digest.NotEmpty();
    }

    public static DataBlock ToDataBlock<T>(this T subject, string principalId) where T : class
    {
        subject.NotNull();

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
        subjects.NotNull();
        subjects.Count().Assert(x => x > 0, "Empty set");

        return new DataBlockBuilder()
            .SetBlockType<T>()
            .SetPrincipleId(principalId)
            .SetData(new List<T>(subjects))
            .Build();
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