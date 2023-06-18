using System.Text.RegularExpressions;
using FluentAssertions;
using Toolbox.Application;
using Toolbox.Types;

namespace Toolbox.Test.Types;

public class ObjectIdTests
{
    [Fact]
    public void TestTenantId()
    {
        const string id = "schema:@tenant/path";
        ObjectId.IsValid(id);

        var o = new ObjectId(id);
        o.Id.Should().Be(id);
        o.Schema.Should().Be("schema");
        o.Tenant.Should().Be("tenant");
        o.Path.Should().Be("path");
        Enumerable.SequenceEqual(o.Paths, new[] { "path" }).Should().BeTrue();
    }

    [Fact]
    public void TestTenantId2Paths()
    {
        const string id = "schema:@tenant/path1/path2";
        ObjectId.IsValid(id);

        var o = new ObjectId(id);
        o.Id.Should().Be(id);
        o.Schema.Should().Be("schema");
        o.Tenant.Should().Be("tenant");
        o.Path.Should().Be("path1/path2");
        Enumerable.SequenceEqual(o.Paths, new[] { "path1", "path2" }).Should().BeTrue();
    }

    [Fact]
    public void TestSystemId()
    {
        const string id = "schema:path";
        ObjectId.IsValid(id);

        var o = new ObjectId(id);
        o.Id.Should().Be(id);
        o.Schema.Should().Be("schema");
        o.Tenant.Should().BeNull();
        o.Path.Should().Be("path");
        Enumerable.SequenceEqual(o.Paths, new[] { "path" }).Should().BeTrue();
    }

    [Fact]
    public void TestSystemId2Paths()
    {
        const string id = "schema:path1/path2";
        ObjectId.IsValid(id);

        var o = new ObjectId(id);
        o.Id.Should().Be(id);
        o.Schema.Should().Be("schema");
        o.Tenant.Should().BeNull();
        o.Path.Should().Be("path1/path2");
        Enumerable.SequenceEqual(o.Paths, new[] { "path1", "path2" }).Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("schema")]
    [InlineData("schema:")]
    [InlineData("schema:/")]
    [InlineData("schema:@tenant")]
    [InlineData("path:data#")]
    [InlineData("path:@da&ta")]
    [InlineData("5path/path2/")]
    public void TestNegativePatterns(string input)
    {
        ObjectId.IsValid(input).Should().BeFalse();
    }

    [Theory]
    [InlineData("schema:path")]
    [InlineData("schema:path/path2")]
    [InlineData("sch2ema:path/path2")]
    [InlineData("schema:@schema/path/path2")]
    [InlineData("schema:path/path2/")]
    [InlineData("schema:@schema/path/path2/")]
    [InlineData("path:/path2/")]
    [InlineData("path:/.path2/")]
    [InlineData("path:@.path2/path")]
    [InlineData("d:a/b/c/d")]
    [InlineData("-path:/path2")]
    [InlineData(".path:path2")]
    [InlineData("path:/.path2./")]
    public void TestPositivePatterns(string input)
    {
        ObjectId.IsValid(input).Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("schema:/")]
    [InlineData("schema:@")]
    [InlineData("schema:@/")]
    [InlineData("schema:")]
    [InlineData("path")]
    [InlineData("path:/data#")]
    [InlineData(".path/path2")]
    [InlineData("path:/da&ta")]
    [InlineData("5path/path2/")]
    public void TestNegative2Patterns(string input)
    {
        ObjectId.IsValid(input).Should().BeFalse();
    }

    [Theory]
    [InlineData("schema:path", "schema", null, "path")]
    [InlineData("schema:@tenant/path", "schema", "tenant", "path")]
    [InlineData("d1:path", "d1", null, "path")]
    [InlineData("d1:@ten1/path", "d1", "ten1", "path")]
    [InlineData("schema:path/path2", "schema", null, "path/path2")]
    [InlineData("schema:@tenant/path/path2", "schema", "tenant", "path/path2")]
    [InlineData("d:a", "d", null, "a")]
    [InlineData("d:@t/a", "d", "t", "a")]
    [InlineData("d:a/b/c/d", "d", null, "a/b/c/d")]
    [InlineData("d:@t/a/b/c/d", "d", "t", "a/b/c/d")]
    public void TestObjectIdParse(string input, string schema, string? tenant, string? path)
    {
        ObjectId objectId = input.ToObjectId();
        objectId.Schema.Should().Be(schema);
        objectId.Tenant.Should().Be(tenant);
        objectId.Path.Should().Be(path);
    }
}
