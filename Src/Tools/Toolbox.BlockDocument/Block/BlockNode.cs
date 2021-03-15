using System;
using Toolbox.Extensions;
using Toolbox.Security;
using Toolbox.Tools;

namespace Toolbox.BlockDocument.Block
{
    public class BlockNode
    {
        public BlockNode() { }

        public BlockNode(DataBlock blockData)
        {
            blockData.VerifyNotNull(nameof(blockData));

            BlockData = blockData;
            Digest = GetDigest();
        }

        public BlockNode(DataBlock blockData, int index, string? previousHash)
        {
            blockData.VerifyNotNull(nameof(blockData));

            BlockData = blockData;
            Index = index;
            PreviousHash = previousHash;
            Digest = GetDigest();
        }

        public BlockNode(BlockNode blockNode)
            : this(blockNode.BlockData, blockNode.Index, blockNode.PreviousHash!)
        {
        }

        public DataBlock BlockData { get; init; } = null!;

        public int Index { get; init; }

        public string? PreviousHash { get; init; }

        public string Digest { get; init; } = null!;

        public bool IsValid() => Digest == GetDigest();

        public string GetDigest()
        {
            var hashes = new string[]
            {
                $"{Index}-{PreviousHash ?? ""}".ToBytes().ToSHA256Hash(),
                BlockData.ToBytes().ToSHA256Hash(),
            };

            return hashes.ToMerkleHash();
        }

        public override bool Equals(object? obj) => obj is BlockNode blockNode &&
                Index == blockNode.Index &&
                PreviousHash == blockNode.PreviousHash &&
                Digest == blockNode.Digest;

        public override int GetHashCode() => HashCode.Combine(Index, PreviousHash, Digest);

        public static bool operator ==(BlockNode v1, BlockNode v2) => v1.Equals(v2);

        public static bool operator !=(BlockNode v1, BlockNode v2) => !v1.Equals(v2);
    }
}