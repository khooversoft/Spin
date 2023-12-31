using System.Text.Json.Serialization;
using FluentAssertions;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Test.Types;

public class TagsToolTests
{
    [Fact]
    public void TagsEmpty()
    {
        var result = Tags.HasTag(null, "a");
        result.Should().BeFalse();

        var result2 = Tags.HasTag(null, "a", "v");
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("t1", "t", false)]
    [InlineData("t1", "t1", true)]
    [InlineData("t1;", "t1", true)]
    [InlineData("t1=v", "t1", true)]
    [InlineData("t1=", "t1", true)]
    [InlineData("t1;t2", "t1", true)]
    [InlineData("t1;t2", "t2", true)]
    [InlineData("t1;t2;t3", "t1", true)]
    [InlineData("t1;t2;t3", "t2", true)]
    [InlineData("t1;t2;t3", "t3", true)]
    [InlineData("t1=v1;t2", "t1", true)]
    [InlineData("t1=v1;t2", "t2", true)]
    [InlineData("t1=v1;t2=v2", "t2", true)]
    public void TestTagHas(string tags, string tag, bool pass)
    {
        Tags.HasTag(tags, tag).Should().Be(pass);
    }

    [Theory]
    [InlineData("t1", "t", "v", false)]
    [InlineData("t1=v", "t1", "v", true)]
    [InlineData("t1=", "t1", "v", false)]
    [InlineData("t1=v2", "t1", "v", false)]
    [InlineData("t1=v2", "t1", "v2", true)]
    [InlineData("t1=v1;t2", "t1", "v1", true)]
    [InlineData("t1=v1;t2;", "t1", "v1", true)]
    [InlineData("t1=v1;t2=v2", "t1", "v1", true)]
    [InlineData("t1=v1;t2=v2", "t2", "v2", true)]
    [InlineData("t1=v1;t2=v2;t3=v3", "t0", "v1", false)]
    [InlineData("t1=v1;t2=v2;t3=v3", "t1", "v0", false)]
    [InlineData("t1=v1;t2=v2;t3=v3", "t1", "v1", true)]
    [InlineData("t1=v1;t2=v2;t3=v3", "t2", "v2", true)]
    [InlineData("t1=v1;t2=v2;t3=v3", "t3", "v1", false)]
    [InlineData("t1=v1;t2=v2;t3=v3", "t3", "v0", false)]
    [InlineData("t1=v1;t2=v2;t3=v3", "t3", "v3", true)]
    public void TestTagHasAndValue(string tags, string tag, string value, bool pass)
    {
        Tags.HasTag(tags, tag, value).Should().Be(pass);
    }
}
