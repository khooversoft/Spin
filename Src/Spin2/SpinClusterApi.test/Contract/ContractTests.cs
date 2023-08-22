using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using SpinCluster.sdk.Actors.Contract;
using SpinCluster.sdk.Actors.PrincipalKey;
using SpinCluster.sdk.Actors.User;
using SpinCluster.sdk.Application;
using SpinClusterApi.test.Application;
using SpinClusterApi.test.Basics;
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
        if (existOption.IsOk()) await contractClient.Delete(contractId, principalId, _context).ThrowOnError();

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

        await contractClient.Delete(contractId, principalId, _context).ThrowOnError();
    }
}