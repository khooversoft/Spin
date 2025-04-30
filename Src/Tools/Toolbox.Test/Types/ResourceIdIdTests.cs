using Toolbox.Extensions;
using Toolbox.Tools;
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
        result.IsOk().BeTrue(result.Error);

        ResourceId resourceId = result.Return();
        resourceId.ToString().Be(id);
        resourceId.Type.Be(ResourceType.System);
        resourceId.Schema.Be(schema);
        resourceId.User.BeNull();
        resourceId.SystemName.Be(systemName);
        resourceId.Domain.BeNull();
        resourceId.Path.BeNull();
        resourceId.PrincipalId.BeNull();
        resourceId.AccountId.BeNull();
    }

    [Theory]
    [InlineData("tenant:domain.com")]
    public void ValidTenant(string id)
    {
        var result = ResourceId.Create(id);
        result.IsOk().BeTrue(result.Error);

        ResourceId resourceId = result.Return();
        resourceId.ToString().Be(id);
        resourceId.Type.Be(ResourceType.Tenant);
        resourceId.Schema.Be("tenant");
        resourceId.User.BeNull();
        resourceId.SystemName.BeNull();
        resourceId.Domain.Be("domain.com");
        resourceId.Path.BeNull();
        resourceId.PrincipalId.BeNull();
        resourceId.AccountId.BeNull();
    }

    [Theory]
    [InlineData("userId@domain.com", "userId", "domain.com", "userId@domain.com")]
    [InlineData("user1@domain7.com", "user1", "domain7.com", "user1@domain7.com")]
    public void ValidPrincipal(string id, string user, string domain, string principalId)
    {
        var result = ResourceId.Create(id);
        result.IsOk().BeTrue(result.Error);

        ResourceId resourceId = result.Return();
        resourceId.ToString().Be(id);
        resourceId.Type.Be(ResourceType.Principal);
        resourceId.Schema.BeNull();
        resourceId.User.Be(user);
        resourceId.SystemName.BeNull();
        resourceId.Domain.Be(domain);
        resourceId.Path.BeNull();
        resourceId.PrincipalId.Be(principalId);
        resourceId.AccountId.BeNull();
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
        result.IsOk().BeTrue(result.Error);

        ResourceId resourceId = result.Return();
        resourceId.ToString().Be(id);
        resourceId.Type.Be(ResourceType.Owned);
        resourceId.Schema.Be(schema);
        resourceId.User.Be(user);
        resourceId.SystemName.BeNull();
        resourceId.Domain.Be(domain);
        resourceId.Path.Be(path);
        resourceId.PrincipalId.Be($"{user}@{domain}");
        resourceId.AccountId.Be(path != null ? $"{domain}/{path}" : null);
    }

    [Theory]
    [InlineData("contract:domain.com/path", "contract", "domain.com", "path")]
    [InlineData("contract:domain.com/path/path2", "contract", "domain.com", "path/path2")]
    [InlineData("contract:domain.com/contract1", "contract", "domain.com", "contract1")]
    public void ValidAccount(string id, string schema, string domain, string path)
    {
        var result = ResourceId.Create(id);
        result.IsOk().BeTrue(result.Error);

        ResourceId resourceId = result.Return();
        resourceId.ToString().Be(id);
        resourceId.Type.Be(ResourceType.DomainOwned);
        resourceId.Schema.Be(schema);
        resourceId.User.BeNull();
        resourceId.SystemName.BeNull();
        resourceId.Domain.Be(domain);
        resourceId.Path.Be(path);
        resourceId.PrincipalId.BeNull();
        resourceId.AccountId.Be($"{domain}/{path}");
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
        result.IsError().BeTrue();
    }

    [Fact]
    public void TenantEqualTests()
    {
        const string id = "tenant:company3.com/path1/path2";

        ResourceId result = ResourceId.Create(id).Return();
        ResourceId result2 = ResourceId.Create(id).Return();

        (result == result2).BeTrue();
    }

    [Fact]
    public void PrincipalEqualTests()
    {
        const string id = "user1@company3.com";

        ResourceId result = ResourceId.Create(id).Return();
        ResourceId result2 = ResourceId.Create(id).Return();

        (result == result2).BeTrue();
    }

    [Fact]
    public void TenantWithPathsSerialization()
    {
        const string id = "tenant:company3.com/path1/path2";

        ResourceId result = ResourceId.Create(id).Return();
        string json = result.ToJson();

        ResourceId result2 = json.ToObject<ResourceId>();

        (result == result2).BeTrue();

        result.Id.Be(result2.Id);
        result.Schema.Be(result2.Schema);
        result.User.Be(result2.User);
        result.Domain.Be(result2.Domain);
        result.Path.Be(result2.Path);
        result.AccountId.Be("company3.com/path1/path2");
        result.PrincipalId.BeNull();
    }

    [Fact]
    public void WorkIdTest()
    {
        const string schema = "schedulerwork";
        string systemName = "WKID-" + Guid.NewGuid().ToString();

        var id = $"{schema}:{systemName}";
        ResourceId.IsValid(id, ResourceType.System, schema).BeTrue();

        Option<ResourceId> resourceIdOption = ResourceId.Create(id);
        resourceIdOption.IsOk().BeTrue();

        ResourceId resourceId = resourceIdOption.Return();
        resourceId.Id.Be(id);
        resourceId.Schema.Be(schema);
        resourceId.SystemName.Be(systemName);
        resourceId.User.BeNull();
        resourceId.Domain.BeNull();
        resourceId.Path.BeNull();
        resourceId.PrincipalId.BeNull();
        resourceId.AccountId.BeNull();
    }
}
