using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Types;

public static class MerkleTreeExtensions
{
    public static string ToMerkleHash(this IEnumerable<string> hashes)
    {
        hashes.NotNull();

        var bytes = new MerkleTree()
            .Append(hashes.ToArray())
            .BuildTree()
            .Value;

        return bytes.ToHex();
    }
}