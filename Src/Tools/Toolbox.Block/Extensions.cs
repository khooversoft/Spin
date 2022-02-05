using System;
using System.Linq;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Security;
using Toolbox.Security.Extensions;
using Toolbox.Security.Sign;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Block
{
    public static class Extensions
    {
        public static BlockChain Add<T>(this BlockChain blockChain, T value, string principleId)
        {
            blockChain.VerifyNotNull(nameof(blockChain));
            value.VerifyNotNull(nameof(value));
            principleId.VerifyNotEmpty(nameof(principleId));

            blockChain += new DataBlockBuilder()
                .SetTimeStamp(DateTime.UtcNow)
                .SetBlockType(value.GetType().Name)
                .SetBlockId(blockChain.Blocks.Count.ToString())
                .SetData(value.ToJson())
                .SetPrincipleId(principleId)
                .Build();

            return blockChain;
        }

        public static BlockChain ToBlockChain(this BlockChainModel blockChainModel)
        {
            blockChainModel.VerifyNotNull(nameof(blockChainModel));

            return new BlockChain(blockChainModel.Blocks);
        }

        public static BlockChainModel ToBlockChainModel(this BlockChain blockChain)
        {
            blockChain.VerifyNotNull(nameof(blockChain));

            return new BlockChainModel { Blocks = blockChain.Blocks.ToList() };
        }

        public static MerkleTree ToMerkleTree(this BlockChain blockChain)
        {
            return new MerkleTree()
                .Append(blockChain.Blocks.Select(x => x.Digest).ToArray());
        }
    }
}