using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Toolbox.Extensions;
using Toolbox.Types;

namespace Toolbox.Graph.test.GraphDbStore;

public class GraphEntityTests
{
    [Fact]
    public async Task AddEntityWithIndexes()
    {
        GraphTestClient testClient = GraphTestStartup.CreateGraphTestHost();
        GraphMap map = testClient.ServiceProvider.GetRequiredService<GraphMap>();
        IGraphFileStore fileStore = testClient.ServiceProvider.GetRequiredService<IGraphFileStore>();

        var entity = new TestEntity
        {
            Id = "user001",
            UserName = "User name",
            Email = "user@domain.com",
            NormalizedUserName = "user001-normalized",
            EmailConfirmed = true,
            PasswordHash = "passwordHash",
            Name = "name1-user001",
            LoginProvider = "microsoft",
            ProviderKey = "user001-microsoft-id",
        };

        var result = await testClient.SetEntity(entity, NullScopeContext.Instance);
        result.IsOk().Should().BeTrue();
        map.Nodes.Count.Should().Be(3);
        map.Edges.Count.Should().Be(2);

        string nodeKey = entity.GetNodeKey();
        string normalizeUserNameNodeKey = "userNormalizedUserName:user001-normalized";
        string logonProviderNodeKey = "logonProvider:microsoft/user001-microsoft-id";
        string nodeTags = "Name=name1-user001,userEmail=user@domain.com";

        var userCmd = $"select (key={normalizeUserNameNodeKey}) a1 -> [*] a2 -> (*) a3;";
        await TestIndex(testClient, userCmd, nodeKey, normalizeUserNameNodeKey, nodeTags);

        var logonProviderCmd = $"select (key={logonProviderNodeKey}) a1 -> [*] a2 -> (*) a3;";
        await TestIndex(testClient, logonProviderCmd, nodeKey, logonProviderNodeKey, nodeTags);

        // Delete node, should delete all other nodes and linked file
        (await testClient.ExecuteScalar($"delete (key={nodeKey});", NullScopeContext.Instance)).IsOk().Should().BeTrue();
        map.Nodes.Count.Should().Be(0);
        map.Edges.Count.Should().Be(0);

        var userOption = await testClient.ExecuteScalar(userCmd, NullScopeContext.Instance);
        userOption.IsOk().Should().BeTrue(userOption.ToString());
        var userResult = userOption.Return();
        userResult.Items.Length.Should().Be(0);
        userResult.Alias.Count.Should().Be(3);
        userResult.Alias.All(x => x.Value.Length == 0).Should().BeTrue();

        // Verify no nodes or edges are left
        await VerifyExist(testClient, $"select (key={normalizeUserNameNodeKey});", 0);
        await VerifyExist(testClient, $"select (key={logonProviderNodeKey});", 0);
        await VerifyExist(testClient, "select [*];", 0);
        await VerifyExist(testClient, $"select (key={nodeKey});", 0);
    }

    private async Task TestIndex(GraphTestClient testClient, string cmd, string nodeKey, string indexKeyNode, string tags)
    {
        var userOption = await testClient.ExecuteScalar(cmd, NullScopeContext.Instance);
        userOption.IsOk().Should().BeTrue();
        GraphQueryResult userResult = userOption.Return();
        userResult.Items.Length.Should().Be(1);
        userResult.Alias.Count.Should().Be(3);

        userResult.Alias["a1"].Action(x =>
        {
            x.Length.Should().Be(1);
            x.OfType<GraphNode>().ToArray().Action(y =>
            {
                y.Length.Should().Be(1);
                y[0].Key.Should().Be(indexKeyNode);
                y[0].Tags.ToTagsString().Should().Be(GraphConstants.UniqueIndexTag);
            });
        });

        userResult.Alias["a2"].Action(x =>
        {
            x.Length.Should().Be(1);
            x.OfType<GraphEdge>().ToArray().Action(y =>
            {
                y.Length.Should().Be(1);
                y[0].FromKey.Should().Be(indexKeyNode);
                y[0].ToKey.Should().Be(nodeKey);
                y[0].Tags.Count.Should().Be(0);
            });
        });

        userResult.Alias["a3"].Action(x =>
        {
            x.Length.Should().Be(1);
            x.OfType<GraphNode>().ToArray().Action(y =>
            {
                y.Length.Should().Be(1);
                y[0].Key.Should().Be(nodeKey);
                y[0].Tags.Count.Should().Be(2);
                y[0].Tags.ToTagsString().Should().Be(tags);
            });
        });
    }

    private async Task VerifyExist(GraphTestClient testClient, string cmd, int expectedCount)
    {
        var result = await testClient.ExecuteScalar(cmd, NullScopeContext.Instance);
        result.IsOk().Should().BeTrue();
        result.Return().Items.Length.Should().Be(expectedCount);
    }

    private sealed record TestEntity
    {
        [GraphKey("user")]
        public string Id { get; set; } = null!;

        public string UserName { get; set; } = null!;

        [GraphTag("userEmail")]
        public string Email { get; set; } = null!;

        [GraphNodeIndex("userNormalizedUserName")]
        public string NormalizedUserName { get; set; } = null!;

        public bool EmailConfirmed { get; set; }
        public string PasswordHash { get; set; } = null!;

        [GraphTag()]
        public string Name { get; set; } = null!;

        [GraphNodeIndex("logonProvider", Format = "{LoginProvider}/{ProviderKey}")]
        public string LoginProvider { get; set; } = null!;
        public string ProviderKey { get; set; } = null!;
    }
}
