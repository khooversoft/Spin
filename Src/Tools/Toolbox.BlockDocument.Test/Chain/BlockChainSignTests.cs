using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Toolbox.Extensions;
using Toolbox.Security;
using Toolbox.Tools;
using Xunit;

namespace Toolbox.BlockDocument.Test.Blocks
{
    public class BlockChainSignTests
    {
        [Fact]
        public void GivenString_WhenHashed_Match()
        {
            string v1 = "Hello";
            string v2 = "Hello";

            string h1 = v1.ToBytes().ToSHA256Hash();
            string h2 = v2.ToBytes().ToSHA256Hash();

            (h1 == h2).VerifyAssert(x => x == true, "Hashes do not match");

            string v3 = "hello1";
            string h3 = v3.ToBytes().ToSHA256Hash();

            (h1 != h3).VerifyAssert(x => x == true, "Hashes match, when different");
        }

        [Fact]
        public void GivenBlockChain_AppendSingleNode_ShouldVerify()
        {
            var now = DateTime.Now;
            var principleSignature = new PrincipleSignature("bobTheIssuer", "bobCustomer", TimeSpan.FromMinutes(1), new RsaPublicPrivateKey());
            var blockChain = new BlockChain();

            var block1 = new DataBlock<TextBlock>(now, "blockTypeV1", "blockIdV1", new TextBlock("name", "type", "author", "dataV1"));
            block1.Validate();
            string block1Digest = block1.Digest;

            block1 = block1.WithSignature(principleSignature);
            blockChain.Add(block1);
            block1.Validate();
            block1Digest.Should().Be(block1.Digest);

            block1.Validate(principleSignature);

            blockChain.IsValid().Should().BeTrue();

            IPrincipleSignatureContainer keyContainer = new PrincipleSignatureContainer()
            {
                principleSignature,
            };

            blockChain.Validate(keyContainer);
        }

        [Fact]
        public void GivenBlockChain_AppendTwoNode_ShouldVerify()
        {
            var now = DateTime.Now;
            var principleSignature = new PrincipleSignature("issuer", "audience", TimeSpan.FromMinutes(1), new RsaPublicPrivateKey());
            var blockChain = new BlockChain();

            var block1 = new DataBlock<TextBlock>(now, "blockTypeV1", "blockIdV1", new TextBlock("name", "type", "author", "dataV1"));
            block1 = block1.WithSignature(principleSignature);
            blockChain.Add(block1);
            block1.Validate(principleSignature);

            var block2 = new DataBlock<TextBlock>(now, "blockTypeV2", "blockIdV2", new TextBlock("name", "type", "author", "dataV2"));
            block2 = block2.WithSignature(principleSignature);
            blockChain.Add(block2);
            block2.Validate(principleSignature);

            blockChain.IsValid().Should().BeTrue();

            IPrincipleSignatureContainer keyContainer = new PrincipleSignatureContainer()
            {
                principleSignature,
            };

            blockChain.Validate(keyContainer);
        }

        [Fact]
        public void GivenBlockChain_AppendManyNode_ShouldVerify()
        {
            var now = DateTime.Now;
            const int max = 10;
            var principleSignature = new PrincipleSignature("issuer", "audience", TimeSpan.FromMinutes(1), new RsaPublicPrivateKey());
            var blockChain = new BlockChain();

            List<DataBlock<TextBlock>> list = Enumerable.Range(0, max)
                .Select(x => new DataBlock<TextBlock>(now, $"blockTypeV{x}", $"blockIdV{x}", new TextBlock("name", "type", "author", $"dataV{x}")).WithSignature(principleSignature))
                .ToList();

            blockChain.Add(list.ToArray());

            blockChain.IsValid().Should().BeTrue();

            IPrincipleSignatureContainer keyContainer = new PrincipleSignatureContainer()
            {
                principleSignature,
            };

            blockChain.Validate(keyContainer);
        }

        [Fact]
        public void GivenSetBlockType_WhenChainCreated_ValuesShouldBeVerified()
        {
            var principleSignature = new PrincipleSignature("issuer", "audience", TimeSpan.FromMinutes(1), new RsaPublicPrivateKey());

            var blockChain = new BlockChain()
            {
                new DataBlock<HeaderBlock>("header", "header_1", new HeaderBlock("Master Contract")).WithSignature(principleSignature),
                new DataBlock<BlobBlock>("contract", "contract_1", new BlobBlock("contract.docx", "docx", "me", Encoding.UTF8.GetBytes("this is a contract between two people"))).WithSignature(principleSignature),
                new DataBlock<TrxBlock>("ContractLedger", "Pmt", new TrxBlock("1", "cr", 100)).WithSignature(principleSignature),
            };

            blockChain.Blocks.Count.Should().Be(4);
            blockChain.IsValid().Should().BeTrue();

            IPrincipleSignatureContainer keyContainer = new PrincipleSignatureContainer()
            {
                principleSignature,
            };

            blockChain.Validate(keyContainer);

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
            IPrincipleSignatureContainer keyContainer = new PrincipleSignatureContainer()
            {
                new PrincipleSignature("issuer", "audience", TimeSpan.FromMinutes(1), new RsaPublicPrivateKey()),
            };

            var blockChain = new BlockChain()
            {
                new DataBlock<HeaderBlock>("header", "header_1", new HeaderBlock("Master Contract")).WithSignature(keyContainer.First()),
                new DataBlock<BlobBlock>("contract", "contract_1", new BlobBlock("contract.docx", "docx", "me", Encoding.UTF8.GetBytes("this is a contract between two people"))).WithSignature(keyContainer.First()),
                new DataBlock<TrxBlock>("ContractLedger", "Pmt", new TrxBlock("1", "cr", 100)).WithSignature(keyContainer.First()),
            };

            blockChain.Blocks.Count.Should().Be(4);
            blockChain.IsValid().Should().BeTrue();
            blockChain.Validate(keyContainer);
            string blockChainDigest = blockChain.GetDigest();
            blockChainDigest.Should().NotBeNullOrEmpty();

            string json = blockChain.ToJson();

            var cloneBlockChain = json.ToBlockChain();
            string cloneBlockChainDigest = cloneBlockChain.GetDigest();
            cloneBlockChainDigest.Should().NotBeNullOrEmpty();

            blockChainDigest.Should().Be(cloneBlockChainDigest);
        }
    }
}