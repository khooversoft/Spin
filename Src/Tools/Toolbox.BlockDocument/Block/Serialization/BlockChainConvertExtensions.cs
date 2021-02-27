using System;
using System.Collections.Generic;
using Toolbox.Tools;

namespace Toolbox.BlockDocument
{
    public static class BlockChainConvertExtensions
    {
        public static string ToJson(this BlockChain blockChain)
        {
            blockChain.VerifyNotNull(nameof(blockChain));
            blockChain.Blocks.Count.VerifyAssert<int, InvalidOperationException>(x => x > 1, _ => "Empty block chain");

            var list = new List<BlockChainNodeModel>();

            foreach (BlockNode node in blockChain)
            {
                BlockChainNodeModel dataBlockNodeModel = node.ConvertTo();
                list.Add(dataBlockNodeModel);
            }

            var blockChainModel = new BlockChainModel()
            {
                Blocks = list,
            };

            return Json.Default.Serialize(blockChainModel);
        }

        public static BlockChain ToBlockChain(this string json)
        {
            json.VerifyNotEmpty(nameof(json));

            BlockChainModel blockChainModel = Json.Default.Deserialize<BlockChainModel>(json)
                .VerifyNotNull(nameof(json));

            blockChainModel.Blocks.VerifyNotNull(nameof(blockChainModel.Blocks));

            var list = new List<BlockNode>();

            foreach (var node in blockChainModel.Blocks!)
            {
                BlockNode blockNode = node.ConvertTo();
                list.Add(blockNode);
            }

            return new BlockChain(list);
        }
    }
}