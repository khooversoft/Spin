using Contract.sdk.Client;
using Contract.sdk.Models;
using Contract.Test.Application;
using FluentAssertions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Toolbox.Abstractions;
using Toolbox.Block;
using Toolbox.Extensions;
using Toolbox.Model;
using Xunit;

namespace Contract.Test;

public class ContractControllerTests
{
    [Fact]
    public async Task GivenBlockChain_WhenSigned_FailedWithWrongSignature()
    {
        ContractClient client = TestApplication.GetContractClient();

        var documentId = new DocumentId("test/unit-tests-smart/contract1");

        var blkHeader = new BlkHeader
        {
            PrincipleId = "dev/user/endUser1@default.com",
            DocumentId = (string)documentId,
            Creator = "test",
            Description = "test description",
        };

        BlockChain blockChain = new BlockChainBuilder()
            .SetPrincipleId(blkHeader.PrincipleId)
            .Build()
            .Add(blkHeader, blkHeader.PrincipleId);

        BlockChainModel signedBlockChainModel = await client.Sign(blockChain.ToBlockChainModel());
        signedBlockChainModel.Should().NotBeNull();

        _ = await client.Validate(signedBlockChainModel);


        // Modify signature
        signedBlockChainModel.Blocks[1] = signedBlockChainModel.Blocks[1] with { DataBlock = signedBlockChainModel.Blocks[1].DataBlock with { JwtSignature = "junk" } };

        bool isValid = await client.Validate(signedBlockChainModel);
        isValid.Should().BeFalse();
    }

    [Fact]
    public async Task GivenBlockChain_WhenFile_WillRoundTrip()
    {
        ContractClient client = TestApplication.GetContractClient();

        var documentId = new DocumentId("test/unit-tests-smart/contract1");

        await Delete(documentId, false);

        var blkHeader = new BlkHeader
        {
            PrincipleId = "dev/user/endUser1@default.com",
            DocumentId = (string)documentId,
            Creator = "test",
            Description = "test description",
        };

        BlockChain blockChain = new BlockChainBuilder()
            .SetPrincipleId(blkHeader.PrincipleId)
            .Build()
            .Add(blkHeader, blkHeader.PrincipleId);

        BlockChainModel signedBlockChainModel = await client.Sign(blockChain.ToBlockChainModel());
        signedBlockChainModel.Should().NotBeNull();

        await client.Set(documentId, signedBlockChainModel);

        BlockChainModel readBlockChainModel = await client.Get(documentId);
        readBlockChainModel.Should().NotBeNull();

        readBlockChainModel.Blocks.Count.Should().Be(signedBlockChainModel.Blocks.Count);
        readBlockChainModel.Blocks
            .Zip(signedBlockChainModel.Blocks)
            .All(x => x.First == x.Second)
            .Should().BeTrue();

        bool isValid = await client.Validate(documentId);
        isValid.Should().BeTrue();

        isValid = await client.Validate(readBlockChainModel);
        isValid.Should().BeTrue();

        await Delete(documentId, true);
    }

    [Fact]
    public async Task GivenNoContract_WhenCreated_ShouldVerify()
    {
        ContractClient client = TestApplication.GetContractClient();

        var documentId = new DocumentId("test/unit-tests-smart/contract1");

        var query = new QueryParameter()
        {
            Filter = "test/unit-tests-smart",
            Recursive = false,
        };

        IReadOnlyList<string> search = (await client.Search(query).ReadNext()).Records;
        if (search.Any(x => x == (string)documentId)) await client.Delete(documentId);

        var blkHeader = new BlkHeader
        {
            PrincipleId = "dev/user/endUser1@default.com",
            DocumentId = (string)documentId,
            Creator = "test",
            Description = "test description",
        };

        await client.Create(blkHeader);

        BlockChainModel model = await client.Get(documentId);
        model.Should().NotBeNull();
        model.Blocks.Should().NotBeNull();
        model.Blocks.Count.Should().Be(2);

        model.Blocks[0].Should().NotBeNull();
        model.Blocks[0].IsValid().Should().BeTrue();

        model.Blocks[1].Should().NotBeNull();
        model.Blocks[1].IsValid().Should().BeTrue();
        model.Blocks[1].DataBlock.Should().NotBeNull();
        model.Blocks[1].DataBlock.BlockType.Should().Be(typeof(BlkHeader).Name);

        bool isValid = await client.Validate(model);
        isValid.Should().BeTrue();


        BatchSet<string> searchList = await client.Search(query).ReadNext();
        searchList.Should().NotBeNull();
        searchList.Records.Any(x => x.EndsWith(documentId.Path)).Should().BeTrue();

        (await client.Delete(documentId)).Should().BeTrue();

        searchList = await client.Search(query).ReadNext();
        searchList.Should().NotBeNull();
        searchList.Records.Any(x => x.EndsWith(documentId.Path)).Should().BeFalse();
    }

    private async Task Delete(DocumentId documentId, bool shouldExist)
    {
        ContractClient client = TestApplication.GetContractClient();

        var query = new QueryParameter()
        {
            Filter = documentId.Id.Split('/').Reverse().Skip(1).Reverse().Join("/"),
            Recursive = false,
        };

        BatchSet<string> searchList = await client.Search(query).ReadNext();
        searchList.Should().NotBeNull();
        bool exists = searchList.Records.Any(x => x.EndsWith(documentId.Path));
        if (!shouldExist && !exists) return;
        exists.Should().BeTrue();

        (await client.Delete(documentId)).Should().BeTrue();

        searchList = await client.Search(query).ReadNext();
        searchList.Should().NotBeNull();
        searchList.Records.Any(x => x.EndsWith(documentId.Path)).Should().BeFalse();
    }

    private record Payload(string Name, string Value);
}
