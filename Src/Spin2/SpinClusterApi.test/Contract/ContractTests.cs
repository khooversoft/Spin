using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using SpinCluster.sdk.Actors.Contract;
using SpinCluster.sdk.Actors.PrincipalKey;
using SpinClusterApi.test.Application;
using Toolbox.Block;
using Toolbox.Extensions;
using Toolbox.Types;

namespace SpinClusterApi.test.Contract;

public class ContractTests : IClassFixture<ClusterApiFixture>
{
    private readonly ClusterApiFixture _cluster;
    private readonly ScopeContext _context = new ScopeContext(NullLogger.Instance);
    private readonly SetupTools _setupTools;

    public ContractTests(ClusterApiFixture fixture)
    {
        _cluster = fixture;
        _setupTools = new SetupTools(_cluster, _context);
    }

    //[Fact(Skip = "server")]
    [Fact]
    public async Task LifecycleTest()
    {
        string subscriptionId = "Company7Subscription";
        string tenantId = "company7.com";
        string principalId = "user1@company7.com";
        string contractId = "contract:company7.com/contract1";

        await _setupTools.DeleteUser(_cluster.ServiceProvider, subscriptionId, tenantId, principalId);
        await _setupTools.CreateUser(_cluster.ServiceProvider, subscriptionId, tenantId, principalId);

        ContractClient contractClient = _cluster.ServiceProvider.GetRequiredService<ContractClient>();

        var existOption = await contractClient.Exist(contractId, _context);
        if (existOption.IsOk()) await contractClient.Delete(contractId, _context).ThrowOnError();

        var createModel = new ContractCreateModel
        {
            DocumentId = contractId,
            PrincipalId = principalId,
        };

        var createOption = await contractClient.Create(createModel, _context);
        createOption.IsOk().Should().BeTrue();

        var query = new ContractQuery
        {
            DocumentId = contractId,
            PrincipalId = principalId,
        };

        var blocks = await contractClient.Query(query, _context);
        blocks.IsOk().Should().BeTrue();
        blocks.Return().Count.Should().Be(1);

        // Add blocks
        const string blockType = "ledger";

        var payloads = new[]
        {
            new Payload { Name = "Name1", Value = 1, Price = 1.5f },
            new Payload { Name = "Name2", Value = 2, Price = 2.5f },
            new Payload { Name = "Name2-offset", Value = 5, Price = 5.5f },
        };

        SignatureClient signatureClient = _cluster.ServiceProvider.GetRequiredService<SignatureClient>();

        foreach (var payloadBlock in payloads)
        {
            var signedBlockOption = await payloadBlock.ToDataBlock(principalId, blockType).Sign(signatureClient, _context);
            signedBlockOption.IsOk().Should().BeTrue();

            DataBlock signedBlock = signedBlockOption.Return();
            var writeResponse = await contractClient.Append(contractId, signedBlock, _context);
            writeResponse.IsOk().Should().BeTrue();
        }

        var blocks2 = await contractClient.Query(query, _context);
        blocks2.IsOk().Should().BeTrue();
        blocks2.Return().Count.Should().Be(4);

        var properties = await contractClient.GetProperties(contractId, principalId, _context);
        properties.IsOk().Should().BeTrue();

        ContractPropertyModel propertyModel = properties.Return();
        propertyModel.DocumentId.Should().Be(contractId);
        propertyModel.OwnerPrincipalId.Should().Be(principalId);
        propertyModel.BlockAcl.Should().NotBeNull();
        propertyModel.BlockAcl.Count.Should().Be(0);
        propertyModel.BlockCount.Should().Be(4);

        await contractClient.Delete(contractId, _context).ThrowOnError();
    }

    private record Payload
    {
        public string? Name { get; set; }
        public int Value { get; set; }
        public float Price { get; set; }
    }
}