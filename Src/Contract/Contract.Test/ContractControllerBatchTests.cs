using Contract.sdk.Client;
using Contract.sdk.Models;
using Contract.sdk.Service;
using Contract.Test.Application;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Toolbox.Abstractions;
using Toolbox.DocumentStore;
using Toolbox.Model;
using Xunit;

namespace Contract.Test;

public class ContractControllerBatchTests
{
    [Fact]
    public async Task GivenSingleBatch_WhenRoundTrip_ShouldVerify()
    {
        ContractClient client = TestApplication.GetContractClient();

        var documentId = new DocumentId("test/unit-tests-smart/contract10");
        string principleId = "dev/user/endUser1@default.com";

        await DeleteDocument(client, documentId);


        var contractCreate = new ContractCreateModel
        {
            PrincipleId = "dev/user/endUser1@default.com",
            DocumentId = (string)documentId,
            Creator = "test",
            Description = "test description 10",
            Name = "document10 name",
        };

        await client.Create(contractCreate);

        var payload = new Payload("payloadName", "payloadValue");

        var batch = new DocumentBatch()
            .SetDocumentId(documentId)
            .Add(payload, principleId);

        await client.Append(batch, CancellationToken.None);

        var dataBlock = await client.Get(documentId);
        dataBlock.Blocks.Count.Should().Be(3);

        var blockTypes = new BlockTypeRequest()
            .Add<Payload>(true);

        IReadOnlyList<DataBlockResult> result = await client.Get(documentId, blockTypes);
        result.Should().NotBeNull();

        var payload1Result = result.GetAll<Payload>();
        payload1Result.Should().NotBeNull();
        payload1Result!.Count.Should().Be(1);
        (payload1Result.First() == payload).Should().BeTrue();

        (await client.Delete(documentId)).Should().BeTrue();
    }

    [Fact]
    public async Task GivenContractWithMultipleAppend3_WhenGetLatest_ShouldVerify()
    {
        ContractClient client = TestApplication.GetContractClient();

        var documentId = new DocumentId("test/unit-tests-smart/contract10");
        string principleId = "dev/user/endUser1@default.com";

        await DeleteDocument(client, documentId);


        var contractCreate = new ContractCreateModel
        {
            PrincipleId = "dev/user/endUser1@default.com",
            DocumentId = (string)documentId,
            Creator = "test",
            Description = "test description 10",
            Name = "document10 name",
        };

        await client.Create(contractCreate);

        var payload1 = new Payload("payloadName", "payloadValue");
        var payload2 = new Payload2(105, "105 data");

        var batch = new DocumentBatch()
            .SetDocumentId(documentId)
            .Add(payload1, principleId)
            .Add(payload2, principleId);

        var appendResult = await client.Append(batch, CancellationToken.None);

        var dataBlock = await client.Get(documentId);
        dataBlock.Blocks.Count.Should().Be(4);

        var blockTypes = new BlockTypeRequest()
            .Add<Payload>(true);

        IReadOnlyList<DataBlockResult> result = await client.Get(documentId, blockTypes);
        result.Should().NotBeNull();

        var p1 = result.GetAll<Payload>();
        p1.Should().NotBeNull();
        p1!.Count.Should().Be(1);
        (p1.First() == payload1).Should().BeTrue();

        var p2 = result.GetAll<Payload2>();
        p2.Should().NotBeNull();
        p2!.Count.Should().Be(1);
        (p2.First() == payload2).Should().BeTrue();

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

    private async Task DeleteDocument(ContractClient client, DocumentId documentId)
    {
        var query = new QueryParameter()
        {
            Filter = "test/unit-tests-smart",
            Recursive = false,
        };

        IReadOnlyList<string> search = (await client.Search(query).ReadNext()).Records;
        if (search.Any(x => x == (string)documentId)) await client.Delete(documentId);
    }

    private record Payload(string Name, string Value);
    private record Payload2(int Id, string Data);
}
