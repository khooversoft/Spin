using FluentAssertions;
using Toolbox.Extensions;
using Toolbox.Types;

namespace Toolbox.Test.Types;

public class ResourceIdIdTests
{
    [Theory]
    [InlineData("subscription:name", "subscription", "name")]
    [InlineData("agent:LocalAgent", "agent", "LocalAgent")]
    public void ValidSubscription(string id, string schema, string systemName)
    {
        var result = ResourceId.Create(id);
        result.IsOk().Should().BeTrue(result.Error);

        ResourceId resourceId = result.Return();
        resourceId.ToString().Should().Be(id);
        resourceId.Type.Should().Be(ResourceType.System);
        resourceId.Schema.Should().Be(schema);
        resourceId.User.Should().BeNull();
        resourceId.SystemName.Should().Be(systemName);
        resourceId.Domain.Should().BeNull();
        resourceId.Path.Should().BeNull();
        resourceId.PrincipalId.Should().BeNull();
        resourceId.AccountId.Should().BeNull();
    }

    [Theory]
    [InlineData("tenant:domain.com")]
    public void ValidTenant(string id)
    {
        var result = ResourceId.Create(id);
        result.IsOk().Should().BeTrue(result.Error);

        ResourceId resourceId = result.Return();
        resourceId.ToString().Should().Be(id);
        resourceId.Type.Should().Be(ResourceType.Tenant);
        resourceId.Schema.Should().Be("tenant");
        resourceId.User.Should().BeNull();
        resourceId.SystemName.Should().BeNull();
        resourceId.Domain.Should().Be("domain.com");
        resourceId.Path.Should().BeNull();
        resourceId.PrincipalId.Should().BeNull();
        resourceId.AccountId.Should().BeNull();
    }

    [Theory]
    [InlineData("userId@domain.com", "userId", "domain.com", "userId@domain.com")]
    [InlineData("user1@domain7.com", "user1", "domain7.com", "user1@domain7.com")]
    public void ValidPrincipal(string id, string user, string domain, string principalId)
    {
        var result = ResourceId.Create(id);
        result.IsOk().Should().BeTrue(result.Error);

        ResourceId resourceId = result.Return();
        resourceId.ToString().Should().Be(id);
        resourceId.Type.Should().Be(ResourceType.Principal);
        resourceId.Schema.Should().BeNull();
        resourceId.User.Should().Be(user);
        resourceId.SystemName.Should().BeNull();
        resourceId.Domain.Should().Be(domain);
        resourceId.Path.Should().BeNull();
        resourceId.PrincipalId.Should().Be(principalId);
        resourceId.AccountId.Should().BeNull();
    }

    [Theory]
    [InlineData("principal-key:user@domain.com/path", "principal-key", "user", "domain.com", "path")]
    [InlineData("principal-key:user@domain.com/path/path2", "principal-key", "user", "domain.com", "path/path2")]
    [InlineData("kid:user1@domain.com/path/path2/path3", "kid", "user1", "domain.com", "path/path2/path3")]
    [InlineData("user:userId@domain.com", "user", "userId", "domain.com", null)]
    [InlineData("user:userId@company7.com", "user", "userId", "company7.com", null)]
    public void ValidOwned(string id, string schema, string user, string domain, string? path)
    {
        var result = ResourceId.Create(id);
        result.IsOk().Should().BeTrue(result.Error);

        ResourceId resourceId = result.Return();
        resourceId.ToString().Should().Be(id);
        resourceId.Type.Should().Be(ResourceType.Owned);
        resourceId.Schema.Should().Be(schema);
        resourceId.User.Should().Be(user);
        resourceId.SystemName.Should().BeNull();
        resourceId.Domain.Should().Be(domain);
        resourceId.Path.Should().Be(path);
        resourceId.PrincipalId.Should().Be($"{user}@{domain}");
        resourceId.AccountId.Should().Be(path != null ? $"{domain}/{path}" : null);
    }

    [Theory]
    [InlineData("contract:domain.com/path", "contract", "domain.com", "path")]
    [InlineData("contract:domain.com/path/path2", "contract", "domain.com", "path/path2")]
    [InlineData("contract:domain.com/contract1", "contract", "domain.com", "contract1")]
    public void ValidAccount(string id, string schema, string domain, string path)
    {
        var result = ResourceId.Create(id);
        result.IsOk().Should().BeTrue(result.Error);

        ResourceId resourceId = result.Return();
        resourceId.ToString().Should().Be(id);
        resourceId.Type.Should().Be(ResourceType.DomainOwned);
        resourceId.Schema.Should().Be(schema);
        resourceId.User.Should().BeNull();
        resourceId.SystemName.Should().BeNull();
        resourceId.Domain.Should().Be(domain);
        resourceId.Path.Should().Be(path);
        resourceId.PrincipalId.Should().BeNull();
        resourceId.AccountId.Should().Be($"{domain}/{path}");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("name")]
    [InlineData("1schema:system")]
    [InlineData("schema:1system")]
    [InlineData("schema:user@system")]
    [InlineData("schema:1user@system.com")]
    [InlineData("user1")]
    [InlineData("user1/path")]
    [InlineData("user:us&er1@company3.com")] // Invalid character '&'
    [InlineData("principal-key:-user1@company3.com")] // Invalid start with '-'
    [InlineData("principal-key:user1@2company3.com")] // Invalid start with number, 2
    [InlineData("contract:domain.com/user@domain.com")] // Invalid start with number, 2
    public void InvalidResourceId(string? id)
    {
        var result = ResourceId.Create(id!);
        result.IsError().Should().BeTrue();
    }

    [Fact]
    public void TenantEqualTests()
    {
        const string id = "tenant:company3.com/path1/path2";

        ResourceId result = ResourceId.Create(id).Return();
        ResourceId result2 = ResourceId.Create(id).Return();

        (result == result2).Should().BeTrue();
    }

    [Fact]
    public void PrincipalEqualTests()
    {
        const string id = "user1@company3.com";

        ResourceId result = ResourceId.Create(id).Return();
        ResourceId result2 = ResourceId.Create(id).Return();

        (result == result2).Should().BeTrue();
    }

    [Fact]
    public void TenantWithPathsSerialization()
    {
        const string id = "tenant:company3.com/path1/path2";

        ResourceId result = ResourceId.Create(id).Return();
        string json = result.ToJson();

        ResourceId result2 = json.ToObject<ResourceId>();

        (result == result2).Should().BeTrue();

        result.Id.Should().Be(result2.Id);
        result.Schema.Should().Be(result2.Schema);
        result.User.Should().Be(result2.User);
        result.Domain.Should().Be(result2.Domain);
        result.Path.Should().Be(result2.Path);
        result.AccountId.Should().Be("company3.com/path1/path2");
        result.PrincipalId.Should().BeNull();
    }

    [Fact]
    public void WorkIdTest()
    {
        const string schema = "schedulerwork";
        string systemName = "WKID-" + Guid.NewGuid().ToString();

        var id = $"{schema}:{systemName}";
        ResourceId.IsValid(id, ResourceType.System, schema).Should().BeTrue();

        Option<ResourceId> resourceIdOption = ResourceId.Create(id);
        resourceIdOption.IsOk().Should().BeTrue();

        ResourceId resourceId = resourceIdOption.Return();
        resourceId.Id.Should().Be(id);
        resourceId.Schema.Should().Be(schema);
        resourceId.SystemName.Should().Be(systemName);
        resourceId.User.Should().BeNull();
        resourceId.Domain.Should().BeNull();
        resourceId.Path.Should().BeNull();
        resourceId.PrincipalId.Should().BeNull();
        resourceId.AccountId.Should().BeNull();
    }
}
