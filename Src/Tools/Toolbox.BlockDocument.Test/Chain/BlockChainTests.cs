using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Toolbox.Types;
using Xunit;

namespace Toolbox.BlockDocument.Test.Blocks
{
    public class BlockChainTests
    {
        [Fact]
        public void GivenBlockChain_AppendSingleNode_ShouldVerify()
        {
            var now = DateTimeOffset.UtcNow;
            var blockChain = new BlockChain();

            var block1 = new DataBlock<TextBlock>(now, "blockTypeV1", "blockIdV1", new TextBlock("name", "type", "author", "dataV1"));
            blockChain.Add(block1);

            blockChain.IsValid().Should().BeTrue();
        }

        [Fact]
        public void GivenBlockChain_AppendTwoNode_ShouldVerify()
        {
            var now = DateTimeOffset.Now;
            var blockChain = new BlockChain();

            var block1 = new DataBlock<TextBlock>(now, "blockTypeV1", "blockIdV1", new TextBlock("name", "type", "author", "dataV1"));
            blockChain.Add(block1);

            var block2 = new DataBlock<TextBlock>(now, "blockTypeV2", "blockIdV2", new TextBlock("name", "type", "author", "dataV2"));
            blockChain.Add(block2);

            blockChain.IsValid().Should().BeTrue();
        }

        [Fact]
        public void GivenBlockChain_AppendManyNode_ShouldVerify()
        {
            var now = DateTimeOffset.Now;
            const int max = 10;
            var blockChain = new BlockChain();

            List<DataBlock<TextBlock>> list = Enumerable.Range(0, max)
                .Select(x => new DataBlock<TextBlock>(now, $"blockTypeV{x}", $"blockIdV{x}", new TextBlock("name", "type", "author", $"dataV{x}")))
                .ToList();

            blockChain.Add(list.ToArray());

            blockChain.IsValid().Should().BeTrue();
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

            blockChain.Blocks
                .Select((x, i) => (x, i))
                .All(x => x.x.Index == x.i)
                .Should().BeTrue();

            blockChain.Blocks[0].BlockData.As<DataBlock<HeaderBlock>>().BlockType.Should().Be("genesis");
            blockChain.Blocks[0].BlockData.As<DataBlock<HeaderBlock>>().BlockId.Should().Be("0");

            blockChain.Blocks[1].BlockData.As<DataBlock<HeaderBlock>>().BlockType.Should().Be("header");
            blockChain.Blocks[1].BlockData.As<DataBlock<HeaderBlock>>().BlockId.Should().Be("header_1");

            blockChain.Blocks[2].BlockData.As<DataBlock<BlobBlock>>().BlockType.Should().Be("contract");
            blockChain.Blocks[2].BlockData.As<DataBlock<BlobBlock>>().BlockId.Should().Be("contract_1");

            blockChain.Blocks[3].BlockData.As<DataBlock<TrxBlock>>().BlockType.Should().Be("ContractLedger");
            blockChain.Blocks[3].BlockData.As<DataBlock<TrxBlock>>().BlockId.Should().Be("Pmt");
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

            //string

            blockChain.Blocks
                .Select((x, i) => (x, i))
                .All(x => x.x.Index == x.i)
                .Should().BeTrue();

            blockChain.Blocks[0].BlockData.As<DataBlock<HeaderBlock>>().BlockType.Should().Be("genesis");
            blockChain.Blocks[0].BlockData.As<DataBlock<HeaderBlock>>().BlockId.Should().Be("0");

            blockChain.Blocks[1].BlockData.As<DataBlock<HeaderBlock>>().BlockType.Should().Be("header");
            blockChain.Blocks[1].BlockData.As<DataBlock<HeaderBlock>>().BlockId.Should().Be("header_1");

            blockChain.Blocks[2].BlockData.As<DataBlock<BlobBlock>>().BlockType.Should().Be("contract");
            blockChain.Blocks[2].BlockData.As<DataBlock<BlobBlock>>().BlockId.Should().Be("contract_1");

            blockChain.Blocks[3].BlockData.As<DataBlock<TrxBlock>>().BlockType.Should().Be("ContractLedger");
            blockChain.Blocks[3].BlockData.As<DataBlock<TrxBlock>>().BlockId.Should().Be("Pmt");
        }
    }
}