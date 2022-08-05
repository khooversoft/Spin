using Contract.sdk.Client;
using Contract.sdk.Models;
using Contract.sdk.Service;
using Contract.Test.Application;
using FluentAssertions;
using System;
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

        var contractCreate = new ContractCreateModel
        {
            PrincipleId = "dev/user/endUser1@default.com",
            DocumentId = (string)documentId,
            Creator = "test",
            Description = "test description",
            Name = "document name",
        };

        await client.Create(contractCreate);

        BlockChainModel model = await client.Get(documentId);
        model.Should().NotBeNull();
        model.Blocks.Should().NotBeNull();
        model.Blocks.Count.Should().Be(2);

        model.Blocks[0].Should().NotBeNull();
        model.Blocks[0].IsValid().Should().BeTrue();

        model.Blocks[1].Should().NotBeNull();
        model.Blocks[1].IsValid().Should().BeTrue();
        model.Blocks[1].DataBlock.Should().NotBeNull();
        model.Blocks[1].DataBlock.BlockType.Should().Be(typeof(ContractCreateModel).Name);

        var readContract = model.Blocks[1].DataBlock.ToObject<ContractCreateModel>();
        (contractCreate == readContract).Should().BeTrue();

        bool verified = await client.Validate(documentId);
        verified.Should().BeTrue();

        BatchQuerySet<string> searchList = await client.Search(query).ReadNext();
        searchList.Should().NotBeNull();
        searchList.Records.Any(x => x.EndsWith(documentId.Path)).Should().BeTrue();

        (await client.Delete(documentId)).Should().BeTrue();

        searchList = await client.Search(query).ReadNext();
        searchList.Should().NotBeNull();
        searchList.Records.Any(x => x.EndsWith(documentId.Path)).Should().BeFalse();
    }

    [Fact]
    public async Task GivenContract_WhenAppend_ShouldVerify()
    {
        ContractClient client = TestApplication.GetContractClient();

        var documentId = new DocumentId("test/unit-tests-smart/contract2");

        var query = new QueryParameter()
        {
            Filter = "test/unit-tests-smart",
            Recursive = false,
        };

        IReadOnlyList<string> search = (await client.Search(query).ReadNext()).Records;
        if (search.Any(x => x == (string)documentId)) await client.Delete(documentId);

        var contractCreate = new ContractCreateModel
        {
            PrincipleId = "dev/user/endUser1@default.com",
            DocumentId = (string)documentId,
            Creator = "test",
            Description = "test description2",
            Name = "document2 name",
        };

        await client.Create(contractCreate);

        var payload = new Payload("payloadName", "payloadValue");
        await client.Append(documentId, payload, contractCreate.PrincipleId);

        bool verified = await client.Validate(documentId);
        verified.Should().BeTrue();


        BlockChainModel model = await client.Get(documentId);
        model.Should().NotBeNull();
        model.Blocks.Should().NotBeNull();
        model.Blocks.Count.Should().Be(3);

        model.Blocks[0].Should().NotBeNull();
        model.Blocks[0].IsValid().Should().BeTrue();

        model.Blocks.Skip(1).ForEach((x, i) =>
        {
            x.Should().NotBeNull();
            x.IsValid().Should().BeTrue();
            x.DataBlock.Should().NotBeNull();

            switch (i)
            {
                case 0:
                    x.DataBlock.BlockType.Should().Be(typeof(ContractCreateModel).Name);
                    x.DataBlock.ObjectClass.Should().Be(typeof(ContractCreateModel).Name);
                    break;

                case 1:
                    x.DataBlock.BlockType.Should().Be(typeof(Payload).Name);
                    x.DataBlock.ObjectClass.Should().Be(typeof(Document).Name);
                    break;

                default:
                    throw new Exception("oops");
            }
        });

        (await client.Delete(documentId)).Should().BeTrue();
    }


    [Fact]
    public async Task GivenContractWithMultipleAppend_WhenGetLatest_ShouldVerify()
    {
        ContractClient client = TestApplication.GetContractClient();

        var documentId = new DocumentId("test/unit-tests-smart/contract3");

        var query = new QueryParameter()
        {
            Filter = "test/unit-tests-smart",
            Recursive = false,
        };

        IReadOnlyList<string> search = (await client.Search(query).ReadNext()).Records;
        if (search.Any(x => x == (string)documentId)) await client.Delete(documentId);

        var contractCreate = new ContractCreateModel
        {
            PrincipleId = "dev/user/endUser1@default.com",
            DocumentId = (string)documentId,
            Creator = "test",
            Description = "test description2",
            Name = "document2 name",
        };

        await client.Create(contractCreate);

        var payloadList = new[]
        {
            new Payload("payloadName", "payloadValue"),
            new Payload("Pay2", "value2"),
        };

        var payloadList2 = new[]
        {
            new Payload2(10, "2-1"),
            new Payload2(20, "2-2"),
            new Payload2(30, "2-3")
        };

        await AppendPayload(documentId, client, contractCreate.PrincipleId, () => payloadList[0]);
        await AppendPayload(documentId, client, contractCreate.PrincipleId, () => payloadList2[0]);
        await AppendPayload(documentId, client, contractCreate.PrincipleId, () => payloadList[1]);
        await AppendPayload(documentId, client, contractCreate.PrincipleId, () => payloadList2[1]);
        await AppendPayload(documentId, client, contractCreate.PrincipleId, () => payloadList2[2]);

        var rawBlock = await client.Get(documentId);

        var blockTypes = new BlockTypeRequest()
            .Add<Payload>(true)
            .Add<Payload2>(true);

        IReadOnlyList<DataBlockResult> result = await client.Get(documentId, blockTypes);
        result.Should().NotBeNull();

        var payloadResultList = result.GetAll<Payload>();
        payloadResultList.Should().NotBeNull();
        payloadResultList!.Count.Should().Be(2);

        payloadList.OrderBy(x => x.Name)
            .Zip(payloadResultList.OrderBy(x => x.Name))
            .All(x => x.First == x.Second)
            .Should().BeTrue();

        var payload2ResultList = result.GetAll<Payload2>();
        payload2ResultList.Should().NotBeNull();
        payload2ResultList!.Count.Should().Be(3);

        payloadList2.OrderBy(x => x.Id)
            .Zip(payload2ResultList.OrderBy(x => x.Id))
            .All(x => x.First == x.Second)
            .Should().BeTrue();

        (await client.Delete(documentId)).Should().BeTrue();
    }

    [Fact]
    public async Task GivenContractWithMultipleAppend2_WhenGetLatest_ShouldVerify()
    {
        ContractClient client = TestApplication.GetContractClient();

        var documentId = new DocumentId("test/unit-tests-smart/contract3");

        var query = new QueryParameter()
        {
            Filter = "test/unit-tests-smart",
            Recursive = false,
        };

        IReadOnlyList<string> search = (await client.Search(query).ReadNext()).Records;
        if (search.Any(x => x == (string)documentId)) await client.Delete(documentId);

        var contractCreate = new ContractCreateModel
        {
            PrincipleId = "dev/user/endUser1@default.com",
            DocumentId = (string)documentId,
            Creator = "test",
            Description = "test description2",
            Name = "document2 name",
        };

        await client.Create(contractCreate);

        var payloadList = new[]
        {
            new Payload("payloadName", "payloadValue"),
            new Payload("Pay2", "value2"),
        };

        var payloadList2 = new[]
        {
            new Payload2(10, "2-1"),
            new Payload2(20, "2-2"),
            new Payload2(30, "2-3")
        };

        await AppendPayload(documentId, client, contractCreate.PrincipleId, () => payloadList[0]);
        await AppendPayload(documentId, client, contractCreate.PrincipleId, () => payloadList2[0]);
        await AppendPayload(documentId, client, contractCreate.PrincipleId, () => payloadList[1]);
        await AppendPayload(documentId, client, contractCreate.PrincipleId, () => payloadList2[1]);
        await AppendPayload(documentId, client, contractCreate.PrincipleId, () => payloadList2[2]);

        var dataBlock = await client.Get(documentId);
        dataBlock.Blocks.Count.Should().Be(7);

        var blockTypes = new BlockTypeRequest()
            .Add<Payload>(true)
            .Add<Payload2>(true);

        IReadOnlyList<DataBlockResult> result = await client.Get(documentId, blockTypes);
        result.Should().NotBeNull();

        var payloadResultList = result.GetAll<Payload>();
        payloadResultList.Should().NotBeNull();
        payloadResultList!.Count.Should().Be(2);

        payloadList.OrderBy(x => x.Name)
            .Zip(payloadResultList.OrderBy(x => x.Name))
            .All(x => x.First == x.Second)
            .Should().BeTrue();

        var payload2ResultList = result.GetAll<Payload2>();
        payload2ResultList.Should().NotBeNull();
        payload2ResultList!.Count.Should().Be(3);

        payloadList2.OrderBy(x => x.Id)
            .Zip(payload2ResultList.OrderBy(x => x.Id))
            .All(x => x.First == x.Second)
            .Should().BeTrue();

        (await client.Delete(documentId)).Should().BeTrue();
    }

    private async Task AppendPayload<T>(DocumentId documentId, ContractClient client, string principleId, Func<T> create) where T : class
    {
        var payload = create();
        await client.Append(documentId, payload, principleId);

        bool verified = await client.Validate(documentId);
        verified.Should().BeTrue();

        T? readPayload = await client.GetLatest<T>(documentId);
        readPayload.Should().NotBeNull();
        payload.Equals(readPayload).Should().BeTrue();
    }


    private record Payload(string Name, string Value);
    private record Payload2(int Id, string Data);
}
