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

public class GraphEntityModelTests
{
    [Fact]
    public void EntityGraphPropertiesNoId()
    {
        var entity = new TestEntity
        {
            UserName = "User name",
            Email = "user@domain.com",
            EmailConfirmed = true,
            PasswordHash = "passwordHash",
            Name = "name1-user001",
            LoginProvider = "microsoft",
            ProviderKey = "user001-microsoft-id",
        };

        var commands = entity.GetGraphCommands();
        commands.IsError().Should().BeTrue();
    }

    [Fact]
    public void EntityGraphProperties()
    {
        var entity = new TestEntity
        {
            Id = "user001",
            UserName = "User name",
            Email = "user@domain.com",
            EmailConfirmed = true,
            PasswordHash = "passwordHash",
            Name = "name1-user001",
            LoginProvider = "microsoft",
            ProviderKey = "user001-microsoft-id",
        };

        var commands = entity.GetGraphCommands();
        commands.IsOk().Should().BeTrue();
        commands.Return().Select(x => x.GetAddCommand()).ToArray().Action(x =>
        {
            x.Length.Should().Be(3);
            x[0].Should().Be("upsert node key=user:user001, userEmail=user@domain.com,Name=name1-user001;");
            x[1].Should().Be("upsert node key=logonProvider:microsoft/user001-microsoft-id, uniqueIndex;");
            x[2].Should().Be("add unique edge fromKey=logonProvider:microsoft/user001-microsoft-id, toKey=user:user001, edgeType=uniqueIndex;");
        });
    }

    [Fact]
    public void EntityGraphProperties2()
    {
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

        var commands = entity.GetGraphCommands();
        commands.IsOk().Should().BeTrue();
        commands.Return().Select(x => x.GetAddCommand()).ToArray().Action(x =>
        {
            x.Length.Should().Be(5);
            x[0].Should().Be("upsert node key=user:user001, userEmail=user@domain.com,Name=name1-user001;");
            x[1].Should().Be("upsert node key=userNormalizedUserName:user001-normalized, uniqueIndex;");
            x[2].Should().Be("add unique edge fromKey=userNormalizedUserName:user001-normalized, toKey=user:user001, edgeType=uniqueIndex;");
            x[3].Should().Be("upsert node key=logonProvider:microsoft/user001-microsoft-id, uniqueIndex;");
            x[4].Should().Be("add unique edge fromKey=logonProvider:microsoft/user001-microsoft-id, toKey=user:user001, edgeType=uniqueIndex;");
        });
    }

    [Fact]
    public void IndexPropertiesAllPropertiesMustHaveValue()
    {
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
            //ProviderKey = "user001-microsoft-id",
        };

        var result = entity.GetGraphCommands();
        result.IsOk().Should().BeTrue();
        result.Return().Count.Should().Be(3);
    }

    [Fact]
    public void PropertyNameIncorrect()
    {
        var entity = new TestEntity2
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

        Action a = () => entity.GetGraphCommands();
        a.Should().Throw<ArgumentException>();
    }

    private sealed record TestEntity
    {
        [GraphKey("user")]
        public string Id { get; set; } = null!;

        public string UserName { get; set; } = null!;

        [GraphTag("userEmail")]   // userEmail=user@domain.com
        public string Email { get; set; } = null!;

        [GraphNodeIndex("userNormalizedUserName")]  // nodeKey = "userNormalizedUserName:user001" -> unique edge to "user:user001"
        public string NormalizedUserName { get; set; } = null!;

        public bool EmailConfirmed { get; set; }
        public string PasswordHash { get; set; } = null!;

        [GraphTag()]   // Name=name1-user001
        public string Name { get; set; } = null!;

        [GraphNodeIndex("logonProvider", Format = "{LoginProvider}/{ProviderKey}")]  // nodeKey="logonProvider:microsoft/user001-microsoft-id" -> unique edge to "user:user001"
        public string LoginProvider { get; set; } = null!;
        public string ProviderKey { get; set; } = null!;
    }

    private sealed record TestEntity2
    {
        [GraphKey("user:{Id}")]
        public string Id { get; set; } = null!;

        public string UserName { get; set; } = null!;

        [GraphTag("userEmail")]   // userEmail=user@domain.com
        public string Email { get; set; } = null!;

        [GraphNodeIndex("userNormalizedUserName")]  // nodeKey = "userNormalizedUserName:user001" -> unique edge to "user:user001"
        public string NormalizedUserName { get; set; } = null!;

        public bool EmailConfirmed { get; set; }
        public string PasswordHash { get; set; } = null!;
        public string Name { get; set; } = null!;

        [GraphNodeIndex("logonProvider", Format = "{LoginProvider}/{x-ProviderKey}")]  // nodeKey="logonProvider:microsoft/user001-microsoft-id" -> unique edge to "user:user001"
        public string LoginProvider { get; set; } = null!;
        public string ProviderKey { get; set; } = null!;
    }
}
