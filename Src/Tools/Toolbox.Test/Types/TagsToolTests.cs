using FluentAssertions;
using Toolbox.Types;

namespace Toolbox.Test.Types;

public class TagsToolTests
{
    [Fact]
    public void TagsEmpty()
    {
        var result = TagsTool.HasTag(null, "a");
        result.Should().BeFalse();

        var result2 = TagsTool.HasTag(null, "a", "v");
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
        TagsTool.HasTag(tags, tag).Should().Be(pass);
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
        TagsTool.HasTag(tags, tag, value).Should().Be(pass);
    }

    [Theory]
    [InlineData("t1", "t", "v", false, false)]
    [InlineData("t1=v", "t1", "v", true, true)]
    [InlineData("t1=", "t1", "v", false, false)]
    [InlineData("t1=v2", "t1", "v", true, false)]
    [InlineData("t1=v2", "t1", "v2", true, true)]
    [InlineData("t1=v1;t2", "t1", "v1", true, true)]
    [InlineData("t1=v1;t2;", "t1", "v1", true, true)]
    [InlineData("t1=v1;t2=v2", "t1", "v1", true, true)]
    [InlineData("t1=v1;t2=v2", "t2", "v2", true, true)]
    [InlineData("t1=v1;t2=v2;t3=v3", "t0", "v1", false, false)]
    [InlineData("t1=v1;t2=v2;t3=v3", "t1", "v0", true, false)]
    [InlineData("t1=v1;t2=v2;t3=v3", "t1", "v1", true, true)]
    [InlineData("t1=v1;t2=v2;t3=v3", "t2", "v2", true, true)]
    [InlineData("t1=v1;t2=v2;t3=v3", "t3", "v1", true, false)]
    [InlineData("t1=v1;t2=v2;t3=v3", "t3", "v0", true, false)]
    [InlineData("t1=v1;t2=v2;t3=v3", "t3", "v3", true, true)]
    public void TryGetValue(string tags, string tag, string value, bool shouldFind, bool shouldHaveValue)
    {
        bool found = TagsTool.TryGetValue(tags, tag, out var foundValue);
        found.Should().Be(shouldFind);
        if (found)
        {
            if (shouldHaveValue)
                foundValue.Should().Be(value);
            else
                foundValue.Should().NotBe(value);
        }
    }
}
