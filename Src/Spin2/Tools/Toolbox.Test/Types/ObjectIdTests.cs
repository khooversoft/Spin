using FluentAssertions;
using Toolbox.Types;

namespace Toolbox.Test.Types;

public class ObjectIdTests
{
    [Fact]
    public void TestTenantId()
    {
        const string id = "schema/tenant/path";
        ObjectId.IsValid(id).Should().BeTrue();

        var o = ObjectId.Parse(id).Return(throwOnNoValue: true);
        o.Id.Should().Be(id);
        o.Schema.Should().Be("schema");
        o.Tenant.Should().Be("tenant");
        o.Path.Should().Be("path");
        Enumerable.SequenceEqual(o.Paths, new[] { "path" }).Should().BeTrue();
    }

    [Fact]
    public void TestTenantId2Paths()
    {
        const string id = "schema/tenant/path1/path2";
        ObjectId.IsValid(id).Should().BeTrue();

        var o = ObjectId.Parse(id).Return(throwOnNoValue: true);
        o.Id.Should().Be(id);
        o.Schema.Should().Be("schema");
        o.Tenant.Should().Be("tenant");
        o.Path.Should().Be("path1/path2");
        Enumerable.SequenceEqual(o.Paths, new[] { "path1", "path2" }).Should().BeTrue();
    }

    [Theory]
    [InlineData("schema/tenant/path1")]
    [InlineData("s1ch2ema/p3a4th/p5at7h2")]
    [InlineData("schema/sch@e.ma/pa-th/pa_th2")]
    [InlineData("schema/schema/path/path2")]
    [InlineData("schema/path/path2/")]
    [InlineData("schema/schema/path/path2/")]
    [InlineData("path/tenant/path2/")]
    [InlineData("path/.tenant/_path")]
    [InlineData("path/.path2/path")]
    [InlineData("d/a/b/c/d")]
    [InlineData("path/.tenant./path2")]
    [InlineData(".path/path2")]
    [InlineData("schema/@")]
    [InlineData("schema/@/")]
    [InlineData("schema/path")]
    [InlineData("schema/schema/path/pa*th2")]
    [InlineData("5path/path2/")]
    public void TestPositivePatterns(string input)
    {
        ObjectId.IsValid(input).Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("schema")]
    [InlineData("schema/")]
    [InlineData("schema//")]
    [InlineData("schema///tenant")]
    [InlineData("path/data#")]
    [InlineData("path///da&ta")]
    [InlineData("sche!ma/sch#ema/path/path2")]
    [InlineData("schema/schema/p%ath/path2")]
    [InlineData("schema///")]
    [InlineData("path")]
    [InlineData("path//data#")]
    [InlineData("path//da&ta")]
    [InlineData("path///.path2./")]
    [InlineData("-path///path2")]
    public void TestNegativePatterns(string input)
    {
        ObjectId.IsValid(input).Should().BeFalse();
    }

    [Theory]
    [InlineData("schema/tenant/path", "schema", "tenant", "path")]
    [InlineData("d1/ten1/path", "d1", "ten1", "path")]
    [InlineData("schema/tenant/path/path2", "schema", "tenant", "path/path2")]
    [InlineData("d/t/a", "d", "t", "a")]
    [InlineData("d/t/a/b/c/d", "d", "t", "a/b/c/d")]
    public void TestObjectIdParse(string input, string schema, string tenant, string? path)
    {
        ObjectId objectId = input.ToObjectId();
        objectId.Schema.Should().Be(schema);
        objectId.Tenant.Should().Be(tenant);
        objectId.Path.Should().Be(path);
    }
}
