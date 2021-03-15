using System.Linq;
using Toolbox.BlockDocument.Block;
using Toolbox.Security;

namespace Toolbox.BlockDocument
{
    public static class MerkleTreeExtensions
    {
        public static MerkleTree ToMerkleTree(this BlockChain blockChain)
        {
            return new MerkleTree()
                .Append(blockChain.Blocks.Select(x => x.Digest).ToArray());
        }
    }
}