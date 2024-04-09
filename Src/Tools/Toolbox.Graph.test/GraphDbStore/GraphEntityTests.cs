using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Toolbox.Extensions;
using Toolbox.Store;
using Toolbox.Types;

namespace Toolbox.Graph.test.GraphDbStore;

public class GraphEntityTests
{
    [Fact]
    public async Task AddEntity()
    {
        IFileStore store = new InMemoryFileStore();
        GraphDb db = new GraphDb(store);

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

        var result = await db.Entity.Set(entity, NullScopeContext.Instance);
        result.IsOk().Should().BeTrue();

        string nodeKey = entity.GetNodeKey();
        (await db.Store.Exist(nodeKey, GraphConstants.EntityName, NullScopeContext.Instance)).ThrowOnError();

        var userCmd = "select (key=userNormalizedUserName:user001-normalized) a1 -> [*] a2 -> (*) a3;";
        var userOption = await db.Graph.ExecuteScalar(userCmd, NullScopeContext.Instance);
        userOption.IsOk().Should().BeTrue();
        GraphQueryResult userResult = userOption.Return();
        userResult.Items.Count.Should().Be(1);
        userResult.Alias.Count.Should().Be(3);

        userResult.Alias["a1"].Action(x =>
        {
            x.Count.Should().Be(1);
            x.OfType<GraphNode>().ToArray().Action(y =>
            {
                y.Length.Should().Be(1);
                y[0].Key.Should().Be("userNormalizedUserName:user001-normalized");
                y[0].Tags.ToString().Should().Be(GraphConstants.UniqueIndexTag);
            });
        });

        userResult.Alias["a2"].Action(x =>
        {
            x.Count.Should().Be(1);
            x.OfType<GraphEdge>().ToArray().Action(y =>
            {
                y.Length.Should().Be(1);
                y[0].FromKey.Should().Be("userNormalizedUserName:user001-normalized");
                y[0].ToKey.Should().Be("user:user001");
                y[0].Tags.Count.Should().Be(0);
            });
        });

        userResult.Alias["a3"].Action(x =>
        {
            x.Count.Should().Be(1);
            x.OfType<GraphNode>().ToArray().Action(y =>
            {
                y.Length.Should().Be(1);
                y[0].Key.Should().Be("user:user001");
                y[0].Tags.Count.Should().Be(2);
                y[0].Tags.ToString().Should().Be("Name=name1-user001,userEmail=user@domain.com");
            });
        });

        // Delete node, should delete all other nodes and linked file
        var deleteREsult
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
