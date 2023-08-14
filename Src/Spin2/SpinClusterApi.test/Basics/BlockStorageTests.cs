using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using SpinCluster.sdk.Actors.Block;
using SpinCluster.sdk.Application;
using SpinClusterApi.test.Application;
using Toolbox.Block;
using Toolbox.Data;
using Toolbox.Security.Principal;
using Toolbox.Types;

namespace SpinClusterApi.test.Basics;

public class BlockStorageTests : IClassFixture<ClusterApiFixture>
{
    private readonly ClusterApiFixture _cluster;
    private readonly ScopeContext _context = new ScopeContext(NullLogger.Instance);

    public BlockStorageTests(ClusterApiFixture fixture)
    {
        _cluster = fixture;
    }

    //[Fact(Skip = "server")]
    [Fact]
    public async Task LifecycleTest()
    {
        BlockClient client = _cluster.ServiceProvider.GetRequiredService<BlockClient>();
        PrincipalId principalId = "user11@test.com";
        ObjectId objectId = $"{SpinConstants.Schema.BlockStorage}/test.com/sc/testblock";
        IPrincipalSignature principleSignature = new PrincipalSignature(principalId, principalId, "userBusiness@domain.com");

        var subscription = await CreateBlock(_cluster.ServiceProvider, objectId, principalId, principleSignature, _context);
        subscription.IsOk().Should().BeTrue();

        Option<BlobPackage> readOption = await client.Get(objectId, _context);
        readOption.IsOk().Should().BeTrue();

        (subscription.Return() == readOption.Return()).Should().BeTrue();

        Option deleteOption = await DeleteBlock(_cluster.ServiceProvider, objectId, _context);
        deleteOption.StatusCode.IsOk().Should().BeTrue();
    }

    public static async Task<Option<BlobPackage>> CreateBlock(IServiceProvider service, ObjectId objectId, PrincipalId principalId, ISign sign, ScopeContext context)
    {
        BlockClient client = service.GetRequiredService<BlockClient>();

        Option<BlobPackage> result = await client.Get(objectId, context);
        if (result.IsOk()) await client.Delete(objectId, context);


        BlockChain blockChain = await new BlockChainBuilder()
            .SetObjectId(objectId)
            .SetPrincipleId(principalId)
            .Build(sign, context)
            .Return();

        BlobPackage block = new BlobPackageBuilder()
            .SetObjectId(objectId)
            .SetContent(blockChain)
            .Build();

        Option setOption = await client.Set(block, context);
        setOption.StatusCode.IsOk().Should().BeTrue();

        return block;
    }

    public static async Task<Option> DeleteBlock(IServiceProvider service, ObjectId objectId, ScopeContext context)
    {
        BlockClient client = service.GetRequiredService<BlockClient>();

        Option deleteOption = await client.Delete(objectId, context);
        deleteOption.StatusCode.IsOk().Should().BeTrue();

        return StatusCode.OK;
    }
}
