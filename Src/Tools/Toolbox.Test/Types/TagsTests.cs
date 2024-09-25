using FluentAssertions;
using Toolbox.Extensions;
using Toolbox.Types;

namespace Toolbox.Test.Types;

public class TagsTests
{
    [Theory]
    [InlineData("a=v;b=v", false)]
    [InlineData("a=v/b=v", false)]
    [InlineData("a=v-b=v", false)]
    [InlineData("*", true)]
    [InlineData("a", true)]
    [InlineData("a=v", true)]
    [InlineData("a,b", true)]
    [InlineData("a=v,b", true)]
    [InlineData("a,b=v", true)]
    [InlineData("a=v,b=v", true)]
    [InlineData("a,b,c", true)]
    [InlineData("a=v,b,c", true)]
    [InlineData("a=v,b=v,c", true)]
    [InlineData("a=v,b=v,c=v", true)]
    [InlineData("a,b=v,c=v", true)]
    [InlineData("a,b,c=v", true)]
    [InlineData("  a=v   ,b  =  v  ,c =   v   ", true)]
    public void IsSetValid(string? key, bool expected)
    {
        var result = TagsTool.Parse(key);
        result.IsOk().Should().Be(expected);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("k")]
    [InlineData("key=node1")]
    [InlineData("key=node1, t1, t2=v2")]
    [InlineData("key=node1, t1, t2=v2, i:logonProvider={LoginProvider}/{ProviderKey}")]
    [InlineData("key=node1, t1, t2=v2, unique:logonProvider={LoginProvider}/{ProviderKey}")]
    [InlineData("key=node1, t1, t2=v2, unique:logonProvider=microsoft/user001-microsoft-id")]
    [InlineData("key=node1, t1, t2=v2, unique:logonProvider='microsoft user001-microsoft-id'")]
    public void EqualPatterns(string? line)
    {
        var t1 = line.ToTags();
        var t2 = line.ToTags();

        t1.DeepEquals(t2).Should().BeTrue();
    }

    [Fact]
    public void TagsSingleAndNotEqual()
    {
        var tags = "key1".ToTags().Action(x =>
        {
            x.Count.Should().Be(1);
            x.ContainsKey("key1").Should().BeTrue();
            x["key1"].Should().BeNull();
            x.Has("key1").Should().BeTrue();
            x.Has("key1", "value").Should().BeFalse();
            x.Has("fake").Should().BeFalse();
            x.Has("key1", "fake1").Should().BeFalse();
            x.Has((string?)null).Should().BeFalse();
        });

        var tags2 = "key1=value1".ToTags().Action(x =>
        {
            x.Count.Should().Be(1);
            x.ContainsKey("key1").Should().BeTrue();
            x["key1"].Should().Be("value1");
            x.Has("key1").Should().BeTrue();
            x.Has("key1", "value1").Should().BeTrue();
            x.Has("fake").Should().BeFalse();
            x.Has("key1", "fake1").Should().BeFalse();
        });

        tags.DeepEquals(tags2).Should().BeFalse();
    }

    [Fact]
    public void TagsKeyValue()
    {
        var tags = new Dictionary<string, string?>
        {
            ["key1"] = "value1",
            ["key2"] = "value2",
        }.ToTags();

        tags.Count.Should().Be(2);
        tags.ContainsKey("key1").Should().BeTrue();
        tags.ContainsKey("key2").Should().BeTrue();

        var tags2 = "key2=value2,key1=value1".ToTags();

        tags.DeepEquals(tags2).Should().BeTrue();
    }

    [Fact]
    public void RemoveTag()
    {
        var tags = new Dictionary<string, string?>
        {
            ["key1"] = "value1",
            ["key2"] = "value2",
        }.ToTags();

        tags.ContainsKey("key1").Should().BeTrue();
        tags.ContainsKey("key2").Should().BeTrue();

        var tags2 = tags.ProcessTags([new KeyValuePair<string, string?>("-key2", null)]);

        tags2.ContainsKey("key1").Should().BeTrue();
        tags2.ContainsKey("key2").Should().BeFalse();
    }
}
