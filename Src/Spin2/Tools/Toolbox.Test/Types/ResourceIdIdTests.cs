using FluentAssertions;
using Toolbox.Extensions;
using Toolbox.Types;

namespace Toolbox.Test.Types;

public class ResourceIdIdTests
{
    [Theory]
    [InlineData("user:user1@company3.com")]
    [InlineData("kid:user1@company3.com/path")]
    [InlineData("principal-key:user1@company3.com")]
    [InlineData("principal-private-key:user1@company3.com")]
    [InlineData("tenant:company3.com")]
    [InlineData("subscription:$system/subscriptionId")]
    [InlineData("subscription:subscriptionId")]
    [InlineData("user1@company3.com")]
    public void ValidResourceIds(string id)
    {
        var result = ResourceId.Create(id);
        result.IsOk().Should().BeTrue(result.Error);

        result.Return().ToString().Should().Be(id);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("user1")]
    [InlineData("user1/path")]
    [InlineData("user:us&er1@company3.com")] // Invalid character '&'
    [InlineData("principal-key:-user1@company3.com")] // Invalid start with '-'
    [InlineData("principal-key:user1@2company3.com")] // Invalid start with number, 2
    public void InvalidResourceIds(string id)
    {
        var result = ResourceId.Create(id);
        result.IsError().Should().BeTrue(result.Error);
    }


    [Fact]
    public void SubscriptionTest()
    {
        const string id = "subscription:subscription1";

        var result = ResourceId.Create(id);
        result.IsOk().Should().BeTrue(result.Error);

        ResourceId resourceId = result.Return();

        resourceId.Id.Should().Be(id);
        resourceId.Schema.Should().Be("subscription");
        resourceId.User.Should().BeNull();
        resourceId.Domain.Should().Be("subscription1");
        resourceId.Path.Should().BeNull();
        resourceId.PrincipalId.Should().BeNull();
        resourceId.ToString().Should().Be(id);
    }

    [Fact]
    public void PrincipalTest()
    {
        const string id = "user1@company3.com";

        var result = ResourceId.Create(id);
        result.IsOk().Should().BeTrue(result.Error);

        ResourceId resourceId = result.Return();

        resourceId.Id.Should().Be(id);
        resourceId.Schema.Should().BeNull();
        resourceId.User.Should().Be("user1");
        resourceId.Domain.Should().Be("company3.com");
        resourceId.Path.Should().BeNull();
        resourceId.PrincipalId.Should().Be("user1@company3.com");
        resourceId.ToString().Should().Be(id);
    }

    [Fact]
    public void PrincipalTestWithPathShouldFail()
    {
        const string id = "user1@company3.com/path";  // path is not allowed for PrincipalId

        var result = ResourceId.Create(id);
        result.IsError().Should().BeTrue(result.Error);
    }

    [Fact]
    public void UserTest()
    {
        const string id = "user:user1@company3.com";

        var result = ResourceId.Create(id);
        result.IsOk().Should().BeTrue(result.Error);

        ResourceId resourceId = result.Return();

        resourceId.Id.Should().Be(id);
        resourceId.Schema.Should().Be("user");
        resourceId.User.Should().Be("user1");
        resourceId.Domain.Should().Be("company3.com");
        resourceId.Path.Should().BeNull();
        resourceId.PrincipalId.Should().Be("user1@company3.com");
        resourceId.ToString().Should().Be(id);
    }

    [Fact]
    public void UserTestWithPath()
    {
        const string id = "user:user1@company3.com/path";

        var result = ResourceId.Create(id);
        result.IsOk().Should().BeTrue(result.Error);

        ResourceId resourceId = result.Return();

        resourceId.Id.Should().Be(id);
        resourceId.Schema.Should().Be("user");
        resourceId.User.Should().Be("user1");
        resourceId.Domain.Should().Be("company3.com");
        resourceId.Path.Should().Be("path");
        resourceId.PrincipalId.Should().Be("user1@company3.com");
        resourceId.ToString().Should().Be(id);
    }

    [Fact]
    public void UserTestWithPaths2()
    {
        const string id = "user:user1@company3.com/path1/path2";

        var result = ResourceId.Create(id);
        result.IsOk().Should().BeTrue(result.Error);

        ResourceId resourceId = result.Return();

        resourceId.Id.Should().Be(id);
        resourceId.Schema.Should().Be("user");
        resourceId.User.Should().Be("user1");
        resourceId.Domain.Should().Be("company3.com");
        resourceId.Path.Should().Be("path1/path2");
        resourceId.PrincipalId.Should().Be("user1@company3.com");
        resourceId.ToString().Should().Be(id);
    }

    [Fact]
    public void TenantTest()
    {
        const string id = "tenant:company3.com";

        var result = ResourceId.Create(id);
        result.IsOk().Should().BeTrue(result.Error);

        ResourceId resourceId = result.Return();

        resourceId.Id.Should().Be(id);
        resourceId.Schema.Should().Be("tenant");
        resourceId.User.Should().BeNull();
        resourceId.Domain.Should().Be("company3.com");
        resourceId.Path.Should().BeNull();
        resourceId.PrincipalId.Should().BeNull();
        resourceId.ToString().Should().Be(id);
    }

    [Fact]
    public void TenantWithPath()
    {
        const string id = "tenant:company3.com/path";

        var result = ResourceId.Create(id);
        result.IsOk().Should().BeTrue(result.Error);

        ResourceId resourceId = result.Return();

        resourceId.Id.Should().Be(id);
        resourceId.Schema.Should().Be("tenant");
        resourceId.User.Should().BeNull();
        resourceId.Domain.Should().Be("company3.com");
        resourceId.Path.Should().Be("path");
        resourceId.PrincipalId.Should().BeNull();
        resourceId.ToString().Should().Be(id);
    }

    [Fact]
    public void TenantWithPaths()
    {
        const string id = "tenant:company3.com/path1/path2";

        var result = ResourceId.Create(id);
        result.IsOk().Should().BeTrue(result.Error);

        ResourceId resourceId = result.Return();

        resourceId.Id.Should().Be(id);
        resourceId.Schema.Should().Be("tenant");
        resourceId.User.Should().BeNull();
        resourceId.Domain.Should().Be("company3.com");
        resourceId.Path.Should().Be("path1/path2");
        resourceId.PrincipalId.Should().BeNull();
        resourceId.ToString().Should().Be(id);
    }

    [Fact]
    public void TenantEqualTests()
    {
        const string id = "tenant:company3.com/path1/path2";

        ResourceId result = ResourceId.Create(id).Return();
        ResourceId result2 = ResourceId.Create(id).Return();

        (result == result2).Should().BeTrue();

        result.Id.Should().Be(result2.Id);
        result.Schema.Should().Be(result2.Schema);
        result.User.Should().Be(result2.User);
        result.Domain.Should().Be(result2.Domain);
        result.PrincipalId.Should().BeNull();
        result.Path.Should().Be(result2.Path);
    }

    [Fact]
    public void PrincipalEqualTests()
    {
        const string id = "user1@company3.com";

        ResourceId result = ResourceId.Create(id).Return();
        ResourceId result2 = ResourceId.Create(id).Return();

        (result == result2).Should().BeTrue();

        result.Id.Should().Be(result2.Id);
        result.Schema.Should().Be(result2.Schema);
        result.User.Should().Be(result2.User);
        result.Domain.Should().Be(result2.Domain);
        result.PrincipalId.Should().Be("user1@company3.com");
        result.Path.Should().Be(result2.Path);
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
        result.PrincipalId.Should().BeNull();
    }
}
