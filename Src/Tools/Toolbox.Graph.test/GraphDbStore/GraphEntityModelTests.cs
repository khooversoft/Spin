using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
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

        var commands = entity.GetGraphAddCommands();
        commands.Count.Should().Be(0);
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

        var commands = entity.GetGraphAddCommands();
        commands.Count.Should().Be(3);
        commands[0].Should().Be("upsert node key=user:user001, userEmail=user@domain.com");
        commands[1].Should().Be("upsert node key=logonProvider:microsoft/user001-microsoft-id");
        commands[2].Should().Be("add unique edge fromKey=logonProvider:microsoft/user001-microsoft-id, toKey=user:user001;");
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

        var commands = entity.GetGraphAddCommands();
        commands.Count.Should().Be(5);
        commands[0].Should().Be("upsert node key=user:user001, userEmail=user@domain.com");
        commands[1].Should().Be("upsert node key=userNormalizedUserName:user001-normalized");
        commands[2].Should().Be("add unique edge fromKey=userNormalizedUserName:user001-normalized, toKey=user:user001;");
        commands[3].Should().Be("upsert node key=logonProvider:microsoft/user001-microsoft-id");
        commands[4].Should().Be("add unique edge fromKey=logonProvider:microsoft/user001-microsoft-id, toKey=user:user001;");
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

        Action a = () => entity.GetGraphAddCommands();
        a.Should().Throw<ArgumentException>();
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

        Action a = () => entity.GetGraphAddCommands();
        a.Should().Throw<ArgumentException>();
    }

    private sealed record TestEntity
    {
        [GraphKey("user:{Id}")]
        public string Id { get; set; } = null!;

        public string UserName { get; set; } = null!;

        [GraphTag("userEmail")]   // userEmail=user@domain.com
        public string Email { get; set; } = null!;

        [GraphNodeIndex("userNormalizedUserName:{NormalizedUserName}")]  // nodeKey = "userNormalizedUserName:user001" -> unique edge to "user:user001"
        public string NormalizedUserName { get; set; } = null!;

        public bool EmailConfirmed { get; set; }
        public string PasswordHash { get; set; } = null!;
        public string Name { get; set; } = null!;

        [GraphNodeIndex("logonProvider:{LoginProvider}/{ProviderKey}")]  // nodeKey="logonProvider:microsoft/user001-microsoft-id" -> unique edge to "user:user001"
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

        [GraphNodeIndex("userNormalizedUserName:{NormalizedUserName}")]  // nodeKey = "userNormalizedUserName:user001" -> unique edge to "user:user001"
        public string NormalizedUserName { get; set; } = null!;

        public bool EmailConfirmed { get; set; }
        public string PasswordHash { get; set; } = null!;
        public string Name { get; set; } = null!;

        [GraphNodeIndex("logonProvider:{LoginProvider}/{x-ProviderKey}")]  // nodeKey="logonProvider:microsoft/user001-microsoft-id" -> unique edge to "user:user001"
        public string LoginProvider { get; set; } = null!;
        public string ProviderKey { get; set; } = null!;
    }
}
