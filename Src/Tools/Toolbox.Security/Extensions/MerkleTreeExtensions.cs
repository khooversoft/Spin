using System;
using System.Collections.Generic;
using System.Linq;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Security
{
    public static class MerkleTreeExtensions
    {
        public static string ToMerkleHash(this IEnumerable<string> hashes)
        {
            hashes.VerifyNotNull(nameof(hashes));

            return new MerkleTree()
                .Append(hashes.ToArray())
                .BuildTree().Value.ToArray()
                .Func(Convert.ToBase64String);
        }
    }
}