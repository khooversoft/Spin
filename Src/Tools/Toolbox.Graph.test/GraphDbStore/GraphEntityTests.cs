using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
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
