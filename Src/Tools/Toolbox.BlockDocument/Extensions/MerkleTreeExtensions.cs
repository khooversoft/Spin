using System.Linq;
using Toolbox.Security;

namespace Toolbox.BlockDocument
{
    public static class MerkleTreeExtensions
    {
        public static MerkleTree ToMerkleTree(this BlockChain blockChain)
        {
            return new MerkleTree()
                .Append(blockChain.Select(x => x.Digest).ToArray());
        }
    }
}