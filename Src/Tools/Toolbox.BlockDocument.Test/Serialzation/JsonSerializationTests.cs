using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit;

namespace Toolbox.BlockDocument.Test
{
    public class JsonSerializationTests
    {
        [Fact]
        public void GivenDataBlock_WhenSerialized_ShouldRoundTrip()
        {
            var block1 = new DataBlock<TextBlock>(DateTime.Now, "blockTypeV1", "blockIdV1", new TextBlock("name", "type", "author", "dataV1"));

            DataBlockModel<TextBlockModel> dataBlockModel = block1.ConvertTo<TextBlock, TextBlockModel>();
            string json = Json.Default.Serialize(dataBlockModel);
            json.Should().NotBeNullOrWhiteSpace();

            DataBlockModel<TextBlockModel>? resultModel = Json.Default.Deserialize<DataBlockModel<TextBlockModel>>(json);
            resultModel.Should().NotBeNull();

            DataBlock<TextBlock> result = resultModel!.ConvertTo<TextBlockModel, TextBlock>();
            result.Should().NotBeNull();

            block1.TimeStamp.Should().Be(result.TimeStamp);
            block1.BlockType.Should().Be(result.BlockType);
            block1.BlockId.Should().Be(result.BlockId);
            (block1.Data == result.Data).Should().BeTrue();
        }

        [Fact]
        public void GivenBlockChain_WhenSerialized_ShouldRoundTrip()
        {
            var blockChain = new BlockChain();

            var block1 = new DataBlock<TextBlock>(DateTime.Now, "blockTypeV1", "blockIdV1", new TextBlock("name", "type", "author", "dataV1"));
            blockChain.Add(block1);
            blockChain.IsValid().Should().BeTrue();

            string blockChainHash = blockChain
                .ToMerkleTree()
                .BuildTree()
                .ToString();

            string json = blockChain.ToJson();

            BlockChain result = json.ToBlockChain();
            result.Blocks.Count.Should().Be(blockChain.Blocks.Count);
            blockChain.IsValid().Should().BeTrue();

            string resultChainHash = result
                .ToMerkleTree()
                .BuildTree()
                .ToString();

            blockChainHash.Should().Be(resultChainHash);
        }

        [Fact]
        public void GivenBlockChain_AppendTwoNode_ShouldRoundTrip()
        {
            var now = DateTime.Now;
            var blockChain = new BlockChain()
            {
                new DataBlock<TextBlock>(now, "blockTypeV1", "blockIdV1", new TextBlock("name", "type", "author", "dataV1")),
                new DataBlock<TextBlock>(now, "blockTypeV2", "blockIdV2", new TextBlock("name", "type", "author", "dataV2")),
            };

            blockChain.IsValid().Should().BeTrue();
            string blockChainHash = blockChain
                .ToMerkleTree()
                .BuildTree()
                .ToString();

            string json = blockChain.ToJson();

            BlockChain result = json.ToBlockChain();
            blockChain.IsValid().Should().BeTrue();
            result.Blocks.Count.Should().Be(blockChain.Blocks.Count);

            string resultChainHash = result
                .ToMerkleTree()
                .BuildTree()
                .ToString();

            blockChainHash.Should().Be(resultChainHash);
        }

        [Fact]
        public void GivenBlockChain_AppendManyNode_ShouldVerify()
        {
            var now = DateTime.Now;
            const int max = 10;
            var blockChain = new BlockChain();

            List<DataBlock<TextBlock>> list = Enumerable.Range(0, max)
                .Select(x => new DataBlock<TextBlock>(now, $"blockTypeV{x}", $"blockIdV{x}", new TextBlock("name", "type", "author", $"dataV{x}")))
                .ToList();

            blockChain.Add(list.ToArray());

            blockChain.IsValid().Should().BeTrue();

            string blockChainHash = blockChain
                .ToMerkleTree()
                .BuildTree()
                .ToString();

            string json = blockChain.ToJson();

            BlockChain result = json.ToBlockChain();
            blockChain.IsValid().Should().BeTrue();

            string resultChainHash = result
                .ToMerkleTree()
                .BuildTree()
                .ToString();

            blockChainHash.Should().Be(resultChainHash);
        }

        [Fact]
        public void GivenSetBlockType_WhenChainCreated_ShouldBeVerified()
        {
            var blockChain = new BlockChain()
            {
                new DataBlock<HeaderBlock>("header", "header_1", new HeaderBlock("Master Contract")),
                new DataBlock<BlobBlock>("contract", "contract_1", new BlobBlock("contract.docx", "docx", "me", Encoding.UTF8.GetBytes("this is a contract between two people"))),
                new DataBlock<TrxBlock>("ContractLedger", "Pmt", new TrxBlock("1", "cr", (MaskDecimal4)100)),
            };

            blockChain.Blocks.Count.Should().Be(4);
            blockChain.IsValid().Should().BeTrue();

            string blockChainHash = blockChain
                .ToMerkleTree()
                .BuildTree()
                .ToString();

            string json = blockChain.ToJson();

            BlockChain result = json.ToBlockChain();
            blockChain.IsValid().Should().BeTrue();
            string resultChainHash = result.ToMerkleTree().BuildTree().ToString();

            blockChainHash.Should().Be(resultChainHash);
        }

        [Fact]
        public void GivenSetBlockType_WhenChainCreated_ShouldSerialize()
        {
            var blockChain = new BlockChain()
            {
                new DataBlock<HeaderBlock>("header", "header_1", new HeaderBlock("Master Contract")),
                new DataBlock<BlobBlock>("contract", "contract_1", new BlobBlock("contract.docx", "docx", "me", Encoding.UTF8.GetBytes("this is a contract between two people"))),
                new DataBlock<TrxBlock>("ContractLedger", "Pmt", new TrxBlock("1", "cr", (MaskDecimal4)100)),
            };

            blockChain.Blocks.Count.Should().Be(4);
            blockChain.IsValid().Should().BeTrue();

            string blockChainHash = blockChain
                .ToMerkleTree()
                .BuildTree()
                .ToString();

            string json = blockChain.ToJson();

            BlockChain result = json.ToBlockChain();
            blockChain.IsValid().Should().BeTrue();
            string resultChainHash = result.ToMerkleTree().BuildTree().ToString();

            blockChainHash.Should().Be(resultChainHash);
        }
    }
}